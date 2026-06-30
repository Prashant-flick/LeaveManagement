using Auth.Application.Interfaces;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Repository;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Auth.Domain.Common.Interfaces;
using Auth.API.Common.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

// ✅ Add Controllers (IMPORTANT)
builder.Services.AddControllers();

// ✅ Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsHistoryTable("__EFMigrationsHistory", "auth")
    ));

builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

// Register MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Auth.Application.Features.Auth.Login.LoginCommand).Assembly);
    cfg.AddOpenBehavior(typeof(Auth.Application.Common.Behaviors.LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(Auth.Application.Common.Behaviors.ValidationBehavior<,>));
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Add validators
builder.Services.AddValidatorsFromAssembly(typeof(Auth.Application.Features.Auth.Login.LoginCommand).Assembly);

// for better error response for validators
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .ToDictionary(
                x => x.Key,
                x => x.Value?.Errors.Select(e => e.ErrorMessage)
            );

        return new BadRequestObjectResult(new
        {
            message = "Validation failed",
            errors
        });
    };
});

// ✅ JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
        )
    };
});

// adding client
builder.Services.AddHttpClient<IEmployeeClient, EmployeeClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5001");
}); 

// ✅ Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Database Initialization Strategy
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    var initStrategy = builder.Configuration["DatabaseSettings:InitStrategy"] ?? "None";

    try
    {
        if (initStrategy.Equals("Recreate", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("InitStrategy is 'Recreate'. Dropping schema 'auth'...");
            await dbContext.Database.ExecuteSqlRawAsync("DROP SCHEMA IF EXISTS auth CASCADE;");
            await dbContext.Database.ExecuteSqlRawAsync("CREATE SCHEMA auth;");
            
            logger.LogInformation("Applying migrations to recreate schema...");
            await dbContext.Database.MigrateAsync();
            
            logger.LogInformation("Seeding default data...");
            await AuthDataSeeder.SeedAdminUserAsync(dbContext, logger);
        }
        else if (initStrategy.Equals("Update", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("InitStrategy is 'Update'. Applying pending migrations...");
            await dbContext.Database.MigrateAsync();
            
            logger.LogInformation("Seeding default data...");
            await AuthDataSeeder.SeedAdminUserAsync(dbContext, logger);
        }
        else
        {
            logger.LogInformation("InitStrategy is '{Strategy}'. Skipping database initialization.", initStrategy);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database initialization.");
    }
}

// ✅ Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
// ✅ MIDDLEWARE ORDER (VERY IMPORTANT 🔥)
app.UseAuthentication();
app.UseAuthorization();

// ✅ Map Controllers (THIS IS WHAT YOU ASKED ✅)
app.MapControllers();

app.Run();