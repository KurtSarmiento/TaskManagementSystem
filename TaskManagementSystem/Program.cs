using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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

        // THESE TWO LINES ARE THE FIX:
        NameClaimType = "unique_name",
        RoleClaimType = "role"
    };
});

builder.Services.AddRateLimiter(options => //to limit the rate of requests to the api
{
    options.AddPolicy("LoginPolicy", context =>
    {
        var apiKey = context.Request.Headers["X-API-Key"].ToString();
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: apiKey ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            });
    });
    options.AddPolicy("GetTasks", context =>
    {
        var apiKey = context.Request.Headers["X-API-Key"].ToString();
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: apiKey ?? "Employee",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
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

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
