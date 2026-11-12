namespace Fundoo.User.Api.Contract
{
    public class ForgotPasswordResponse
    {
        public string Message { get; set; } = string.Empty;

        // Development only. Remove before production deployment.
        public string? ResetToken { get; set; }

        public DateTime? ExpiresAtUtc { get; set; }
    }
}
