namespace Fundoo.User.Api.Contract;

public class PasswordResetRequestedEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();

    public string RecipientEmail { get; set; } = string.Empty;

    public string ResetToken { get; set; } = string.Empty;

    public DateTimeOffset OccurredAtUtc { get; set; }
        = DateTimeOffset.UtcNow;
}
