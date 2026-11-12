using System.ComponentModel.DataAnnotations;

namespace Fundoo.User.Api.Contract
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
