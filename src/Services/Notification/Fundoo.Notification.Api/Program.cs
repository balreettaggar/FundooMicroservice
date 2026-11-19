using Fundoo.Notification.Application;
using Fundoo.Notification.Infrastructure;
using Serilog;
using Fundoo.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<
    IAccessTokenEmailSender,
    SmtpAccessTokenEmailSender>();

builder.Host.UseSerilog((context, _, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("ServiceName", "Fundoo.User.Api"));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();