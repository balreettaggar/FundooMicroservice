using Fundoo.User.Application.Interfaces;
using Fundoo.User.Domain;
using Fundoo.User.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundoo.User.Infrastructure.Repositories
{
    public class UserRepository(UserDbContext dbContext)
    : IUserRepository
    {
        public async Task<AppUser?> FindByNormalizedEmailAsync(
            string normalizedEmail,
            CancellationToken cancellationToken)
        {
            return await dbContext.Users
                .FirstOrDefaultAsync(
                    user => user.NormalizedEmail == normalizedEmail,
                    cancellationToken);
        }

        public async Task<AppUser?> FindByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await dbContext.Users
                .FirstOrDefaultAsync(
                    user => user.Id == id,
                    cancellationToken);
        }

        public async Task AddAsync(
            AppUser user,
            CancellationToken cancellationToken)
        {
            await dbContext.Users.AddAsync(user, cancellationToken);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }


        public async Task AddPasswordResetTokenAsync(
            PasswordResetToken token,
            CancellationToken cancellationToken)
        {
            await dbContext.PasswordResetTokens.AddAsync(
                token, cancellationToken);
        }

        public async Task<PasswordResetToken?> FindPasswordResetTokenAsync(
            Guid userId,
            string tokenHash,
            CancellationToken cancellationToken)
        {
            return await dbContext.PasswordResetTokens
                .FirstOrDefaultAsync(
                    token => token.UserId == userId
                        && token.TokenHash == tokenHash
                        && token.UsedAtUtc == null
                        && token.ExpiresAtUtc > DateTime.UtcNow,
                    cancellationToken);
        }

    }
}
