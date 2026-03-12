using KeycloakGateway.Application;
using KeycloakGateway.Application.DTOs.Auth;
using KeycloakGateway.Extensions;
using KeycloakGateway.Infrastructure;
using KeycloakGateway.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using StackExchange.Redis;


var builder = WebApplication.CreateBuilder(args);

// ================= SERVICES =================

// Controllers
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFE", policy =>
    {
        policy
            .WithOrigins("http://localhost:9000", "http://localhost:5000", "http://localhost:5174", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    //options.Authority = "http://localhost:8080/realms/sso-realm";
    options.Authority = "http://172.16.1.11:8080/realms/DThu";
    options.RequireHttpsMetadata = false; // dev only

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        //ValidIssuer = "http://localhost:8080/realms/sso-realm",
        ValidIssuer = "http://172.16.1.11:8080/realms/DThu",
        ValidateAudience = false
    };
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = ConfigurationOptions.Parse("172.16.1.11:6379");
    config.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(config);
});

builder.Services.AddAuthorization();

builder.Services.AddMemoryCache();
// Application layer
builder.Services.AddApplication();

// Infrastructure layer
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<KeycloakOptions>(
    builder.Configuration.GetSection("Keycloak"));

builder.Services.Configure<KeycloakClients>(
    builder.Configuration.GetSection("KeycloakClients"));

builder.Services.AddHttpClient();
// Swagger
builder.Services.AddSwaggerServices();

// ================= BUILD =================

var app = builder.Build();

// ================= MIDDLEWARE PIPELINE =================

// Global exception handler (đặt sớm)
app.UseMiddleware<ExceptionMiddleware>();


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowFE");

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SwaggerAuthMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerServices();
}

app.MapControllers();

app.Run();