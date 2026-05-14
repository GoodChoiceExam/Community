using FitLife.Community.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NLog;
using NLog.Web;
using FitLife.Community.Api.Messaging;

var logger = LogManager.Setup().LoadConfigurationFromFile("NLog.config").GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "fitlife-identity";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "fitlife";
    var jwksUrl = builder.Configuration["Jwt:JwksUrl"] ?? "http://localhost:5244/.well-known/jwks.json";

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeyResolver = (_, _, _, _) => JwksSigningKeyResolver.GetSigningKeys(jwksUrl)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    logger.Warn(context.Exception, "JWT authentication failed");
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    logger.Warn("Unauthorized request to {Path}", context.Request.Path);
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
            policy.WithOrigins("http://localhost:5271")
                .AllowAnyHeader()
                .AllowAnyMethod());
    });

    builder.Services.AddSingleton<ICommunityService, CommunityService>();
    builder.Services.AddHostedService<MemberCreatedConsumer>();
    
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter a valid JWT Bearer token from the Identity service."
        });

        options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", null, null),
                []
            }
        });
    });

    var app = builder.Build();
    logger.Info("FitLife Community API starting");

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors("Frontend");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/healthz", (ILogger<Program> log) =>
    {
        log.LogInformation("Health check requested");
        return Results.Ok(new { status = "healthy" });
    });
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    LogManager.Shutdown();
}

static class JwksSigningKeyResolver
{
    private static readonly HttpClient Client = new();
    private static DateTime _expiresAt = DateTime.MinValue;
    private static IReadOnlyCollection<SecurityKey> _cachedKeys = [];

    public static IEnumerable<SecurityKey> GetSigningKeys(string jwksUrl)
    {
        if (_cachedKeys.Count > 0 && DateTime.UtcNow < _expiresAt)
            return _cachedKeys;

        var json = Client.GetStringAsync(jwksUrl).GetAwaiter().GetResult();
        var jwks = new JsonWebKeySet(json);

        _cachedKeys = jwks.Keys.Cast<SecurityKey>().ToArray();
        _expiresAt = DateTime.UtcNow.AddMinutes(5);
        return _cachedKeys;
    }
}

public partial class Program;
