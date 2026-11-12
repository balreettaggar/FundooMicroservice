using Fundoo.User.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundoo.User.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<AppUser?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

        Task<AppUser?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

        Task AddAsync(AppUser user, CancellationToken cancellationToken);

        Task SaveChangesAsync(CancellationToken cancellationToken);

        Task AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken cancellationToken);

        Task<PasswordResetToken?> FindPasswordResetTokenAsync(Guid userId, string tokenHash, CancellationToken cancellationToken);
    }
}
