using Leave.Infrastructure.Data;
using Leave.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using Serilog;
using Leave.Domain.Interfaces;
using Leave.API.Common.Middleware;
using Leave.Application.Interfaces;
using Leave.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsHistoryTable("__EFMigrationsHistory", "leave")
    ));

builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();
builder.Services.AddScoped<IEmployeeClient, EmployeeClient>();

// Register MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Leave.Application.Features.Leaves.Commands.CreateLeave.CreateLeaveCommand).Assembly);
    cfg.AddOpenBehavior(typeof(Leave.Application.Common.Behaviors.LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(Leave.Application.Common.Behaviors.ValidationBehavior<,>));
});

builder.Services.AddControllers();

// Add validators
builder.Services.AddValidatorsFromAssembly(typeof(Leave.Application.Features.Leaves.Commands.CreateLeave.CreateLeaveCommand).Assembly);

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

builder.Services.AddHttpClient<IEmployeeClient, EmployeeClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5001");
});

builder.Services.AddAuthorization();

// Add Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

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
            logger.LogInformation("InitStrategy is 'Recreate'. Dropping schema 'leave'...");
            await dbContext.Database.ExecuteSqlRawAsync("DROP SCHEMA IF EXISTS \"leave\" CASCADE;");
            await dbContext.Database.ExecuteSqlRawAsync("CREATE SCHEMA \"leave\";");
            
            logger.LogInformation("Applying migrations to recreate schema...");
            await dbContext.Database.MigrateAsync();
        }
        else if (initStrategy.Equals("Update", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("InitStrategy is 'Update'. Applying pending migrations...");
            await dbContext.Database.MigrateAsync();
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// app.UseHttpsRedirection();

app.Run();
