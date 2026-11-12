namespace Fundoo.User.Api.Contract
{
    public class RegisterResponse
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }
    }
}
