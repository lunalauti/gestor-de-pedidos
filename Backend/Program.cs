using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Orders.Application.Services;
using Orders.Application.Events;
using Orders.Domain.Repositories;
using Orders.Infrastructure.Data;
using Orders.Infrastructure.Repositories;
using Orders.Infrastructure.Events;
using Connection.Infrastructure.RabbitMQ;
using Connection.Infrastructure.Publishers;
using Connection.Infrastructure.Middleware;
using Connection.Domain.Services;
using Connection.Infrastructure.Services;
using Backend.Modules.Users.Application.Queries;
using Backend.Modules.Users.Application.Services;
using Backend.Modules.Users.Infrastructure.Persistence;
using Backend.Modules.Users.Application.Interfaces;
using Backend.Shared.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// PostgreSQL Database Configuration
var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseNpgsql(baseConnectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "orders");
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
    });
});

// UsersDbContext
builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql($"{baseConnectionString};Search Path=auth"));

// RabbitMQ Configuration
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<RabbitMQSettings>(provider =>
{
    var settings = new RabbitMQSettings();
    builder.Configuration.GetSection("RabbitMQ").Bind(settings);
    return settings;
});
builder.Services.AddSingleton<IRabbitMQConnection>(provider =>
{
    var settings = provider.GetRequiredService<RabbitMQSettings>();
    return new RabbitMQConnection(settings);
});

// Connection Module - Message Services
builder.Services.AddScoped<IMessagePublisher, RabbitMQPublisher>();
builder.Services.AddScoped<IMessageConsumer, RabbitMQConsumer>();

// Dependency Injection - Orders Module
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderEventPublisher, OrderEventPublisher>();

// Dependency Injection - Users Module
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<IUserQueries, UserQueries>();
builder.Services.AddScoped<IRoleQueries, RoleQueries>();

// Background Services
builder.Services.AddHostedService<MessageConsumerService>();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var jwtKey = builder.Configuration["Jwt:Key"];
var key = Encoding.ASCII.GetBytes(jwtKey);

// Logging Configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// Global Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

// Database Initialization
using (var scope = app.Services.CreateScope())
{
    try
    {
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Initializing databases...");
        
        // Inicializar UsersDbContext
        await usersDbContext.Database.MigrateAsync();
        logger.LogInformation("Users database initialized successfully");
        
        // Inicializar OrderDbContext
        await context.Database.MigrateAsync();
        logger.LogInformation("Orders database initialized successfully");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the databases");
        throw;
    }
}

app.Run();