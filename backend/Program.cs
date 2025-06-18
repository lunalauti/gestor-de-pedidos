using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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

using Users.Application.Queries;
using Users.Application.Services;
using Users.Infrastructure.Persistence;
using Users.Application.Interfaces;

// Notification Module Imports
using Notifications.Application.Services;
using Notifications.Infrastructure.Data;
using Notifications.Domain.Interfaces;
using Notifications.Infrastructure.Repositories;
using Notifications.Infrastructure.Services;
using Notifications.Infrastructure.BackgroundServices;

using Backend.Shared.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// PostgreSQL Database Configuration
var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Orders Database Context
builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseNpgsql(baseConnectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "orders");
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
    });
});

// Users Database Context
builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql($"{baseConnectionString};Search Path=auth"));

// Notifications Database Context
builder.Services.AddDbContext<NotificationDbContext>(options =>
{
    options.UseNpgsql(baseConnectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "notifications");
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
    });
});

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

// Dependency Injection - Notifications Module
builder.Services.AddScoped<IDeviceTokenRepository, DeviceTokenRepository>();
builder.Services.AddScoped<INotificationService, FirebaseNotificationService>();
builder.Services.AddScoped<INotificationApplicationService, NotificationApplicationService>();

// Background Services
builder.Services.AddHostedService<MessageConsumerService>();
builder.Services.AddHostedService<TokenCleanupService>();

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

// JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured");
}
var key = Encoding.ASCII.GetBytes(jwtKey);

// Logging Configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Authentication Configuration
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

// Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Global Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Database Initialization
using (var scope = app.Services.CreateScope())
{
    try
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Initializing databases...");
        
        // Initialize Users Database
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        await usersDbContext.Database.MigrateAsync();
        logger.LogInformation("Users database initialized successfully");
        
        // Initialize Orders Database
        var ordersDbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        await ordersDbContext.Database.MigrateAsync();
        logger.LogInformation("Orders database initialized successfully");

        // Initialize Notifications Database
        var notificationDbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        await notificationDbContext.Database.MigrateAsync();
        logger.LogInformation("Notifications database initialized successfully");
        
        logger.LogInformation("All databases initialized successfully");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the databases");
        throw;
    }
}

app.Run();