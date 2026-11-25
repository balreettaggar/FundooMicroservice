
using Confluent.Kafka;
using System.Text.Json;
using Fundoo.User.Api.Contract;

namespace Fundoo.User.Api.Kafka;

public class PasswordResetProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PasswordResetProducer> _logger;

    public PasswordResetProducer(
        IProducer<string, string> producer,
        IConfiguration configuration,
        ILogger<PasswordResetProducer> logger)
    {
        _producer = producer;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task PublishPasswordResetEventAsync(
        string recipientEmail,
        string resetToken,
        CancellationToken cancellationToken)
    {
        var resetEvent = new PasswordResetRequestedEvent
        {
            EventId = Guid.NewGuid(),
            RecipientEmail = recipientEmail,
            ResetToken = resetToken,
            OccurredAtUtc = DateTimeOffset.UtcNow
        };

        var message = new Message<string, string>
        {
            Key = resetEvent.EventId.ToString(),
            Value = JsonSerializer.Serialize(resetEvent)
        };

        var topic = _configuration["Kafka:Topic"]
            ?? throw new InvalidOperationException(
                "Kafka topic is not configured.");

        var result = await _producer.ProduceAsync(
            topic,
            message,
            cancellationToken);

        _logger.LogInformation(
            "Password reset event published. EventId: {EventId}, Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
            resetEvent.EventId,
            result.Topic,
            result.Partition.Value,
            result.Offset.Value);
    }
}
