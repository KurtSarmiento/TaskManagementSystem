using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = "9f3a7c2e5b1d8a4f6c0e92b7d5a3f8c1";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddRateLimiter(options => //to limit the rate of requests to the api
{
    options.OnRejected = async (context, token) =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Rate limit exceeded for IP {IP}", context.HttpContext.Connection.RemoteIpAddress);

        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Too many requests!", token);
    };
    options.AddPolicy("LoginPolicy", context =>
    {
        var role = context.User?.FindFirst(ClaimTypes.Role)?.Value;

        int limit = role switch
        {
            "Admin" => 15,
            "Manager" => 10,
            "Employee" => 5,
            _ => 5
        };

        var apiKey = context.Request.Headers["X-API-Key"].ToString();
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: apiKey ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limit,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            });
    });
    options.AddPolicy("GetTasks", context =>
    {
        var role = context.User?.FindFirst(ClaimTypes.Role)?.Value;

        int limit = role switch
        {
            "Admin" => 30,
            "Manager" => 25,
            "Employee" => 20,
            _ => 20
        };

        var apiKey = context.Request.Headers["X-API-Key"].ToString();
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: apiKey ?? "Employee",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limit,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            });
    });
    options.AddPolicy("PostTasks", context =>
    {
        var apiKey = context.Request.Headers["X-API-Key"].ToString();
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: apiKey ?? "Manager",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            });
    });
    options.AddPolicy("AdminTasks", context =>
    {
        var apiKey = context.Request.Headers["X-API-Key"].ToString();
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: apiKey ?? "Admin",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            });
    });
    options.AddPolicy("RoleBasedPolicy", context =>
    {
        var role = context.User?.FindFirst(ClaimTypes.Role)?.Value;

        int limit = role switch
        {
            "Admin" => 50,
            "Manager" => 30,
            "Employee" => 10,
            _ => 5
        };

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"{context.Connection.RemoteIpAddress}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limit,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.Run();
