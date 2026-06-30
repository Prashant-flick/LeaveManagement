using Employee.Domain.Interfaces;
using Employee.Infrastructure.Data;
using Employee.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Employee.Domain.Common.Interfaces;
using Employee.API.Common.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsHistoryTable("__EFMigrationsHistory", "employee")
    ));

builder.Services.AddScoped<IEmployeeRepository,EmployeeRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Employee.Application.Features.Employees.Commands.CreateEmployee.CreateEmployeeCommand).Assembly);
    cfg.AddOpenBehavior(typeof(Employee.Application.Common.Behaviors.LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(Employee.Application.Common.Behaviors.ValidationBehavior<,>));
});

// Add validators
builder.Services.AddValidatorsFromAssembly(typeof(Employee.Application.Features.Employees.Commands.CreateEmployee.CreateEmployeeCommand).Assembly);

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
            logger.LogInformation("InitStrategy is 'Recreate'. Dropping schema 'employee'...");
            await dbContext.Database.ExecuteSqlRawAsync("DROP SCHEMA IF EXISTS employee CASCADE;");
            await dbContext.Database.ExecuteSqlRawAsync("CREATE SCHEMA employee;");
            
            logger.LogInformation("Applying migrations to recreate schema...");
            await dbContext.Database.MigrateAsync();
            
            logger.LogInformation("Seeding default data...");
            await DataSeeder.SeedRolesAndAdminAsync(dbContext, logger);
        }
        else if (initStrategy.Equals("Update", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("InitStrategy is 'Update'. Applying pending migrations...");
            await dbContext.Database.MigrateAsync();
            
            logger.LogInformation("Seeding default data...");
            await DataSeeder.SeedRolesAndAdminAsync(dbContext, logger);
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

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// app.UseHttpsRedirection();

app.Run();
