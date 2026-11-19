using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundoo.User.Domain
{
    public class AppUser
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        // Email.Trim().ToUpperInvariant()
        public string NormalizedEmail { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }

        public ICollection<ExternalLogin> ExternalLogins { get; set; } = new List<ExternalLogin>();
    }
}
