using Fundoo.User.Infrastructure;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Register controllers and API documentation.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register PostgreSQL, repository, password hasher and JWT service.
builder.Services.AddUserInfrastructure(builder.Configuration);



builder.Services
    .AddHttpClient("NotificationClient")
    .AddStandardResilienceHandler(options =>
    {
        // 1. Attempt Timeout
        options.AttemptTimeout.Timeout =
            TimeSpan.FromSeconds(5); // change to 5 to test timeout

        //// 2. Total Request Timeout
        options.TotalRequestTimeout.Timeout =
            TimeSpan.FromSeconds(5); // change to 5 to test timeout

        // 3. Rate Limiter
        options.RateLimiter.DefaultRateLimiterOptions.PermitLimit = 5;
        options.RateLimiter.DefaultRateLimiterOptions.QueueLimit = 0;

        // 4. Retry Strategy (Polly)
        options.Retry.MaxRetryAttempts = 3;

        options.Retry.BackoffType =
            DelayBackoffType.Exponential;

        options.Retry.Delay =
            TimeSpan.FromSeconds(1);

        options.Retry.UseJitter = true;

        //// 5. Circuit Breaker
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.MinimumThroughput = 4;

        options.CircuitBreaker.SamplingDuration =
            TimeSpan.FromSeconds(30);

        options.CircuitBreaker.BreakDuration =
            TimeSpan.FromSeconds(15);
    });




var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

// Simple health endpoint.
app.MapGet("/health", () => Results.Ok(new
{
    Service = "Fundoo.User.Api",
    Status = "Healthy"
}));

app.Run();

