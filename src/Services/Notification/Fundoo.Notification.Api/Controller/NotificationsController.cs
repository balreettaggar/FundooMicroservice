
using Fundoo.Notification.Application;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace Fundoo.Notification.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly IAccessTokenEmailSender _emailSender;
    private static int passwordResetAttempts = 0;

    public NotificationsController(
        IAccessTokenEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    // Existing functionality: email the login access token.
    //[HttpPost("access-token")]
    //public async Task<IActionResult> SendAccessTokenEmail(
    //    [FromBody] SendAccessTokenEmailRequest request,
    //    CancellationToken cancellationToken)
    //{
    //    if (string.IsNullOrWhiteSpace(request.RecipientEmail) ||
    //        string.IsNullOrWhiteSpace(request.AccessToken))
    //    {
    //        return BadRequest(new
    //        {
    //            message = "Recipient email and access token are required."
    //        });
    //    }

    //    await _emailSender.SendAccessTokenEmailAsync(
    //        request.RecipientEmail,
    //        request.AccessToken,
    //        cancellationToken);

    //    return Ok(new
    //    {
    //        message = "Access token email sent successfully."
    //    });
    //}

    // New functionality: email the password-reset token.
    [HttpPost("password-reset")]
    public async Task<IActionResult> SendPasswordResetEmail(
        [FromBody] SendPasswordResetEmailRequest request,
        CancellationToken cancellationToken)
    {
        //int attempt = Interlocked.Increment(
        //ref passwordResetAttempts);

        //Console.WriteLine(
        //    $"Notification API: Password reset attempt {attempt}");

        //if (attempt <= 2)
        //{
        //    Console.WriteLine("Returning 503 Service Unavailable");

        //    return StatusCode(503, new
        //    {
        //        Message = "Temporary service unavailable",
        //        Attempt = attempt
        //    });
        //}

        Console.WriteLine("Returning 200 OK");

        if (string.IsNullOrWhiteSpace(request.RecipientEmail) ||
            string.IsNullOrWhiteSpace(request.ResetToken))
        {
            return BadRequest(new
            {
                message = "Recipient email and reset token are required."
            });
        }

        Console.WriteLine("Notification API: Delay started");

        await Task.Delay(
            TimeSpan.FromSeconds(10),
            cancellationToken);

        Console.WriteLine("Notification API: Delay completed");

        //await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

        await _emailSender.SendPasswordResetEmailAsync(
            request.RecipientEmail,
            request.ResetToken,
            cancellationToken);

        return Ok(new
        {
            message = "Password-reset email sent successfully."
        });
    }

    //private static int retryTestAttempts = 0;

    //[HttpGet("retry-test")]
    //public IActionResult RetryTest()
    //{
    //    int attempt = Interlocked.Increment(
    //        ref retryTestAttempts
    //    );

    //    Console.WriteLine(
    //        $"Notification API received request #{attempt}"
    //    );

    //    if (attempt <= 2)
    //    {
    //        Console.WriteLine("Returning 503 Service Unavailable");

    //        return StatusCode(503, new
    //        {
    //            Message = "Temporary service unavailable",
    //            Attempt = attempt
    //        });
    //    }

    //    Console.WriteLine("Returning 200 OK");

    //    return Ok(new
    //    {
    //        Message = "Notification API is now available",
    //        Attempt = attempt
    //    });
    //}
}
