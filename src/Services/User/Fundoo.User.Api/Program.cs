using Fundoo.User.Infrastructure;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Serilog;
using System.Net;
using Fundoo.Observability;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Register controllers and API documentation.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register PostgreSQL, repository, password hasher and JWT service.
builder.Services.AddUserInfrastructure(builder.Configuration);



builder.Services
    .AddHttpClient("NotificationClient");
//    .AddStandardResilienceHandler(options =>
//    {
//        // 1. Attempt Timeout
//        // options.AttemptTimeout.Timeout =
//          //  TimeSpan.FromSeconds(5); // change to 5 to test timeout

//        //// 2. Total Request Timeout
//       // options.TotalRequestTimeout.Timeout =
//        //    TimeSpan.FromSeconds(5); // change to 5 to test timeout

//        // 3. Rate Limiter
//        options.RateLimiter.DefaultRateLimiterOptions.PermitLimit = 5;
//        options.RateLimiter.DefaultRateLimiterOptions.QueueLimit = 0;

//        // 4. Retry Strategy (Polly)
//        options.Retry.MaxRetryAttempts = 3;

//        options.Retry.BackoffType =
//            DelayBackoffType.Exponential;

//        options.Retry.Delay =
//            TimeSpan.FromSeconds(1);

//        options.Retry.UseJitter = true;

//        //// 5. Circuit Breaker
//        options.CircuitBreaker.FailureRatio = 0.5;
//        options.CircuitBreaker.MinimumThroughput = 4;

//        options.CircuitBreaker.SamplingDuration =
//            TimeSpan.FromSeconds(30);

//        options.CircuitBreaker.BreakDuration =
//            TimeSpan.FromSeconds(15);
    //});


builder.Host.UseSerilog((context, _, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("ServiceName", "Fundoo.User.Api"));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

const string ExternalCookieScheme = "ExternalCookie";
const string GoogleOidcScheme = "GoogleOIDC";

builder.Services
    .AddAuthentication()
    .AddCookie(ExternalCookieScheme, options =>
    {
        options.Cookie.Name = "Fundoo.External";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    })
    .AddOpenIdConnect(GoogleOidcScheme, options =>
    {
        options.SignInScheme = ExternalCookieScheme;
        options.Authority = "https://accounts.google.com";
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
            ?? throw new InvalidOperationException("Google ClientId is missing.");
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
            ?? throw new InvalidOperationException("Google ClientSecret is missing.");

        options.CallbackPath = "/signin-oidc";
        options.ResponseType = "code";
        options.UsePkce = true;
        options.SaveTokens = false;
        options.GetClaimsFromUserInfoEndpoint = true;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
    });

builder.Services.AddAuthorization();

builder.Services.AddFundooCorrelationIds();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

// Simple health endpoint.
app.MapGet("/health", () => Results.Ok(new
{
    Service = "Fundoo.User.Api",
    Status = "Healthy"
}));

app.Run();

