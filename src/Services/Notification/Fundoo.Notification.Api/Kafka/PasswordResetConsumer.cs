
using Confluent.Kafka;
using Fundoo.Notification.Application;
using System.Text.Json;

namespace Fundoo.Notification.Api.Kafka;

public class PasswordResetConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PasswordResetConsumer> _logger;

    public PasswordResetConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<PasswordResetConsumer> logger)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers =
                _configuration["Kafka:BootstrapServers"],

            GroupId =
                _configuration["Kafka:GroupId"],

            AutoOffsetReset = AutoOffsetReset.Latest,

            EnableAutoCommit = false,

            EnableAutoOffsetStore = false
        };

        using var consumer =
            new ConsumerBuilder<string, string>(config).Build();

        consumer.Subscribe(
            _configuration["Kafka:Topic"]);

        _logger.LogInformation(
            "Kafka consumer started. Subscribed to {Topic}",
            _configuration["Kafka:Topic"]);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);

                var resetEvent =
                    JsonSerializer.Deserialize<PasswordResetRequestedEvent>(
                        result.Message.Value,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (resetEvent == null ||
                    string.IsNullOrWhiteSpace(resetEvent.RecipientEmail) ||
                    string.IsNullOrWhiteSpace(resetEvent.ResetToken))
                {
                    throw new InvalidOperationException(
                        "Invalid password-reset event received.");
                }

                using var scope = _scopeFactory.CreateScope();

                var emailSender = scope.ServiceProvider
                    .GetRequiredService<IAccessTokenEmailSender>();

                await emailSender.SendPasswordResetEmailAsync(
                    resetEvent.RecipientEmail,
                    resetEvent.ResetToken,
                    stoppingToken);

                // Commit only after successful email delivery.
                consumer.Commit(result);

                _logger.LogInformation(
                    "Password-reset email successfully processed for {Email}. EventId: {EventId}",
                    resetEvent.RecipientEmail,
                    resetEvent.EventId);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Kafka consumer is shutting down.");
        }
        finally
        {
            consumer.Close();
        }
    }
}

