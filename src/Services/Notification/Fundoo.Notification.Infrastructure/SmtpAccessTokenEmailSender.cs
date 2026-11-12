using Fundoo.Notification.Application;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Fundoo.Notification.Infrastructure;

public class SmtpAccessTokenEmailSender : IAccessTokenEmailSender
{
    private readonly IConfiguration _configuration;

    public SmtpAccessTokenEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendAccessTokenEmailAsync(
        string recipientEmail,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var host = _configuration["Email:SmtpHost"]
            ?? throw new InvalidOperationException("SMTP host is not configured.");

        var port = int.Parse(
            _configuration["Email:SmtpPort"]
            ?? throw new InvalidOperationException("SMTP port is not configured."));

        var username = _configuration["Email:Username"]
            ?? throw new InvalidOperationException("Email username is not configured.");

        var password = _configuration["Email:Password"]
            ?? throw new InvalidOperationException("Email password is not configured.");

        var fromAddress = _configuration["Email:FromAddress"]
            ?? throw new InvalidOperationException("Sender email is not configured.");

        var fromName = _configuration["Email:FromName"] ?? "Fundoo Notifications";

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        message.Subject = "Your Fundoo Access Token";

        message.Body = new TextPart("plain")
        {
            Text = $"""
                Hello,

                Your Fundoo access token is:

                {accessToken}

                Keep this token private. Anyone who obtains it may be able to access your account
                until it expires.

                If you did not request this, please contact support.
                """
        };

        using var client = new SmtpClient();

        await client.ConnectAsync(
            host,
            port,
            SecureSocketOptions.StartTls,
            cancellationToken);

        await client.AuthenticateAsync(
            username,
            password,
            cancellationToken);

        await client.SendAsync(message, cancellationToken);

        await client.DisconnectAsync(true, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(
    string recipientEmail,
    string resetToken,
    CancellationToken cancellationToken = default)
    {
        var host = _configuration["Email:SmtpHost"]
            ?? throw new InvalidOperationException("SMTP host is not configured.");

        var port = int.Parse(
            _configuration["Email:SmtpPort"]
            ?? throw new InvalidOperationException("SMTP port is not configured."));

        var username = _configuration["Email:Username"]
            ?? throw new InvalidOperationException("Email username is not configured.");

        var password = _configuration["Email:Password"]
            ?? throw new InvalidOperationException("Email password is not configured.");

        var fromAddress = _configuration["Email:FromAddress"]
            ?? throw new InvalidOperationException("Sender email is not configured.");

        var fromName = _configuration["Email:FromName"] ?? "Fundoo Notifications";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        message.Subject = "Fundoo Password Reset";

        message.Body = new TextPart("plain")
        {
            Text = $"""
            Hello,

            We received a request to reset your Fundoo password.

            Your password-reset token is:

            {resetToken}

            This token expires in 15 minutes. If you did not request
            a password reset, you can ignore this email.

            Fundoo Team
            """
        };

        using var client = new SmtpClient();

        await client.ConnectAsync(
            host, port, SecureSocketOptions.StartTls, cancellationToken);

        await client.AuthenticateAsync(
            username, password, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
