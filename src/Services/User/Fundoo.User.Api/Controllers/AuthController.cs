using Fundoo.User.Api.Contract;
using Fundoo.User.Api.Kafka;
using Fundoo.User.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polly.Timeout;

namespace Fundoo.User.Api.Controllers
{
    [ApiController] 
    [Route("api/auth")]
    public class AuthController(
    IAuthService authService,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    PasswordResetProducer passwordResetProducer) : ControllerBase
    // primary constructors arrived in c#12, we can directly use the variables, available throughout
    // the class body
    {
        [HttpPost("register")]
        // the next three are producesresponsetype examples showed swagger, example purposes
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RegisterResponse>> Register( // async because db operations
            // Task is like a promise that ensures a process will complete, and generic type returns the
            // result in that order

            // flexibility to return either: A RegisterResponse object.
            // An HTTP result such as Created(), BadRequest(), Conflict(), etc.

            [FromBody] RegisterRequest request, // Get the value of this parameter from the HTTP request body.
            CancellationToken cancellationToken)
        {
            try
            {
                var user = await authService.RegisterAsync(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Password,
                    cancellationToken);

                var response = new RegisterResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    CreatedAtUtc = user.CreatedAtUtc
                };

                return Created($"/api/users/{user.Id}", response);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Registration failed",
                    Detail = ex.Message
                });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponse>> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            var token = await authService.LoginAsync(
                request.Email,
                request.Password,
                cancellationToken);

            if (token is null)
            {
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Authentication failed",
                    Detail = "Invalid email or password."
                });
            }

            var expiryMinutes = int.TryParse(
                configuration["Jwt:ExpiryMinutes"],
                out var minutes) && minutes > 0
                    ? minutes
                    : 30;

            var response = new AuthResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes)
            };

            return Ok(response);
        }


        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword(
            [FromBody] ForgotPasswordRequest request,
            CancellationToken cancellationToken)
        {
            var token = await authService.RequestPasswordResetAsync(
                request.Email, cancellationToken);

            // Return the same generic response whether the account exists or not.
            var response = new ForgotPasswordResponse
            {
                Message = "If the account exists, a password reset email will be sent.",
                ResetToken = null,
                ExpiresAtUtc = null
            };

            if (token is null)
            {
                return Ok(response);
            }

            // Publish password-reset event to Kafka.
            await passwordResetProducer.PublishPasswordResetEventAsync(
                request.Email,
                token,
                cancellationToken);

            return Ok(response);
        }



        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MessageResponse>> ResetPassword(
            [FromBody] ResetPasswordRequest request,
            CancellationToken cancellationToken)
        {
            var success = await authService.ResetPasswordAsync(
                request.Email,
                request.Token,
                request.NewPassword,
                cancellationToken);

            if (!success)
            {
                return BadRequest(new MessageResponse
                {
                    Message = "The reset token is invalid, expired, or already used."
                });
            }

            return Ok(new MessageResponse
            {
                Message = "Password reset successfully."
           
            });
        }

        [HttpGet("retry-test")]
        public async Task<IActionResult> RetryTest()
        {
            var client = httpClientFactory.CreateClient("RetryTestClient");

            var notificationUrl =
                "http://localhost:5062/api/notifications/retry-test";

            try
            {
                var response = await client.GetAsync(notificationUrl);

                var responseBody =
                    await response.Content.ReadAsStringAsync();

                return StatusCode(
                    (int)response.StatusCode,
                    responseBody);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new
                {
                    Message = "Retry attempts failed.",
                    Error = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleComplete), "Auth")
            };

            return Challenge(properties, "GoogleOIDC");
        }

        [AllowAnonymous]
        [HttpGet("google-complete")]
        public async Task<IActionResult> GoogleComplete()
        {
            var result = await HttpContext.AuthenticateAsync("ExternalCookie");
            if (!result.Succeeded || result.Principal is null)
            {
                return Unauthorized(new { Message = "Google sign-in failed." });
            }
            try
            {
                var principal = result.Principal;
                var subject = principal.FindFirst("sub")?.Value
                    ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                    ?? principal.FindFirst("email")?.Value;
                var verified = principal.FindFirst("email_verified")?.Value;
                var firstName = principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value
                    ?? principal.FindFirst("given_name")?.Value ?? string.Empty;
                var lastName = principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value
                    ?? principal.FindFirst("family_name")?.Value ?? string.Empty;

                if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(email) ||
                    !string.Equals(verified, "true", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { Message = "Google identity or verified email is missing." });

                var token = await authService.LoginWithExternalProviderAsync(
                    "Google", subject, email, firstName, lastName, HttpContext.RequestAborted);
                if (token is null) return Unauthorized(new { Message = "Fundoo account is inactive." });

                return Ok(new AuthResponse
                {
                    AccessToken = token,
                    TokenType = "Bearer",
                    ExpiresAtUtc = DateTime.UtcNow.AddMinutes(configuration.GetValue("Jwt:ExpiryMinutes", 30))
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ProblemDetails { Status = 409, Title = "Google account cannot be linked", Detail = ex.Message });
            }
            finally
            {
                await HttpContext.SignOutAsync("ExternalCookie");
            }
        }

    }

}

