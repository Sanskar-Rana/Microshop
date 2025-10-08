using System.Security.Claims;
using System.Text;
using FluentValidation;
using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using Microshop.Catalog.Domain.Entities;
using Microshop.Catalog.Domain.Validators;
using Microshop.Catalog.Infrastructure.Data;
using Microshop.Catalog.Infrastructure.DepedencyInjection;
using Microshop.Catalog.Infrastructure.DependencyInjection;
using Microshop.Catalog.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Micrsoshop.Catalog.Application.Dtos.Category;
using Micrsoshop.Catalog.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthCheckService(builder.Configuration);
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opts =>
{
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Validator
builder.Services.AddScoped<IValidator<Product>, ProductValidator>();
builder.Services.AddScoped<IValidator<Category>, CategoryValidator>();


//Adding JWT Configuration

var tokenValidation = new TokenValidationParameters()
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JWT:Secret"])),
    ValidateIssuer = true,
    ValidIssuer = builder.Configuration["JWT:Issuer"],
    ValidateAudience = true,
    ValidAudience = builder.Configuration["JWT:Audience"],
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero
};

builder.Services.AddAuthentication(opts =>
{
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opts =>
{
    opts.SaveToken = true;
    opts.RequireHttpsMetadata = false;
    opts.TokenValidationParameters = tokenValidation;

    opts.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "JWT authentication failed");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Authentication failed");
            Console.WriteLine($"Type: {context.Exception.GetType().Name}");
            Console.WriteLine($"Message: {context.Exception.Message}");
            Console.ResetColor();
            return Task.CompletedTask;
        },

        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("JWT token validated for {User}", context.Principal.Identity?.Name);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Token Validated Successfully");
            Console.WriteLine($"User: {context.Principal.Identity?.Name}");
            Console.WriteLine($"   Roles: {string.Join(',', context.Principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");
            Console.ResetColor();
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddSingleton(tokenValidation);

/*
builder.Services.Configure<MemoryCheckOptions>("Feedback Service Memory Check", opts =>
{
    opts.Threshold = 1_000_000_000;
});

//Health Check
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), healthQuery: "select 1", name: "SQL Server", failureStatus: HealthStatus.Unhealthy)
    .AddCheck<RemoteHealthCheck>("Remote Endpoints Health Check", failureStatus: HealthStatus.Unhealthy)
    .AddCheck<MemoryHealthCheck>("Feedback Service Memory Check", failureStatus: HealthStatus.Unhealthy, tags: new[]{"Feedback Service"})
    .AddUrlGroup(new Uri("http://localhost:5095/api/health/self"),name: "base URL", failureStatus: HealthStatus.Unhealthy);
//HealthCheck UI
builder.Services.AddHealthChecksUI(opts =>
{
    opts.SetEvaluationTimeInSeconds(15);
    opts.MaximumHistoryEntriesPerEndpoint(60);
    opts.SetApiMaxActiveRequests(1);
    opts.AddHealthCheckEndpoint("feedback api", "/api/health");
}) .AddInMemoryStorage();*/

//builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

var app = builder.Build();
app.UseInfrastructure();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseHealthCheckService();

/*
app.MapHealthChecks("/api/health", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("api/health/self", new HealthCheckOptions
{
    Predicate = _ => false
});

app.UseHealthChecksUI(delegate(Options options)
{
    options.UIPath = "/healthcheck-ui";
   
});*/

app.Run();