using Fundoo.User.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundoo.User.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AppUser> RegisterAsync(string firstName,string lastName,string email, string password, CancellationToken cancellationToken);

        Task<string?> LoginAsync(string email,string password,CancellationToken cancellationToken);


        Task<string?> RequestPasswordResetAsync(string email, CancellationToken cancellationToken);

        Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken);

        Task<string?> LoginWithExternalProviderAsync(string provider, string providerSubject, string email, string firstName, string lastName, CancellationToken cancellationToken);

    }
}
