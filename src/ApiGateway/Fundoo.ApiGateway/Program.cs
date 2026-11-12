
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "ocelot.json",
    optional: false,
    reloadOnChange: true);

builder.Services
    .AddOcelot(builder.Configuration)
    .AddPolly();

builder.Services.AddSwaggerForOcelot(builder.Configuration);
builder.Services.AddHttpClient("NotificationClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

// Redirect the Gateway root to its combined Swagger UI.
app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

app.UseSwaggerForOcelotUI(options =>
{
    options.PathToSwaggerGenerator = "/swagger/docs";
});

await app.UseOcelot();

app.Run();


//using System.Threading.RateLimiting;
//using Microsoft.AspNetCore.RateLimiting;

//using MMLib.SwaggerForOcelot.DependencyInjection;
//using MMLib.SwaggerForOcelot.Middleware;
//using Ocelot.DependencyInjection;
//using Ocelot.Middleware;

//var builder = WebApplication.CreateBuilder(args);

//builder.Configuration.AddJsonFile(
//    "ocelot.json",
//    optional: false,
//    reloadOnChange: true);

//builder.Services.AddOcelot(builder.Configuration);
//builder.Services.AddSwaggerForOcelot(builder.Configuration);

//// NEW: Configure ASP.NET Core's built-in rate limiter.
//builder.Services.AddRateLimiter(options =>
//{
//    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

//    options.GlobalLimiter =
//        PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
//            RateLimitPartition.GetFixedWindowLimiter(
//                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString()
//                    ?? "unknown",
//                factory: partition => new FixedWindowRateLimiterOptions
//                {
//                    PermitLimit = 5,
//                    Window = TimeSpan.FromSeconds(30),
//                    QueueLimit = 0,
//                    AutoReplenishment = true
//                }));
//});

//var app = builder.Build();

//// Keep your existing Swagger root redirect.
//app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

//app.UseSwaggerForOcelotUI(options =>
//{
//    options.PathToSwaggerGenerator = "/swagger/docs";
//});

//// NEW: Apply rate limiting before Ocelot forwards requests.
//app.UseRateLimiter();

//await app.UseOcelot();

//app.Run();
