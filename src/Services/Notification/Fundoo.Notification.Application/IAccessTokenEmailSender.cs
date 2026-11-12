namespace Fundoo.Notification.Application;

public interface IAccessTokenEmailSender
{
    Task SendAccessTokenEmailAsync(
        string recipientEmail,
        string accessToken,
        CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(
    string recipientEmail,
    string resetToken,
    CancellationToken cancellationToken = default);
}
