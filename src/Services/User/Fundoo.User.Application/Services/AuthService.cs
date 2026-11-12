using Fundoo.User.Application.Interfaces;
using Fundoo.User.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundoo.User.Application.Services
{
    public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IAuthService
    {
        public async Task<AppUser> RegisterAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            CancellationToken cancellationToken)
        {
            var normalizedEmail = email.Trim().ToUpperInvariant();

            var existingUser =
                await userRepository.FindByNormalizedEmailAsync(
                    normalizedEmail, cancellationToken);

            if (existingUser is not null)
            {
                throw new InvalidOperationException(
                    "An account with this email already exists.");
            }

            var now = DateTime.UtcNow;

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = email.Trim(),
                NormalizedEmail = normalizedEmail,
                PasswordHash = passwordHasher.Hash(password),
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            await userRepository.AddAsync(user, cancellationToken);
            await userRepository.SaveChangesAsync(cancellationToken);

            return user;
        }

        public async Task<string?> LoginAsync(
            string email,
            string password,
            CancellationToken cancellationToken)
        {
            var normalizedEmail = email.Trim().ToUpperInvariant();

            var user = await userRepository.FindByNormalizedEmailAsync(
                normalizedEmail, cancellationToken);

            if (user is null ||
                !user.IsActive ||
                !passwordHasher.Verify(password, user.PasswordHash))
            {
                return null;
            }

            return tokenService.CreateAccessToken(user);
        }

        public async Task<string?> RequestPasswordResetAsync(
            string email,
            CancellationToken cancellationToken)
        {
            var normalizedEmail = email.Trim().ToUpperInvariant();

            var user = await userRepository.FindByNormalizedEmailAsync(
                normalizedEmail, cancellationToken);

            if (user is null || !user.IsActive)
            {
                return null;
            }

            // Generate a cryptographically secure random token.
            var tokenBytes =
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);

            var rawToken = Convert.ToBase64String(tokenBytes);

            // Store only the SHA-256 hash in PostgreSQL.
            var tokenHash = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(rawToken)));

            var expiresAtUtc = DateTime.UtcNow.AddMinutes(15);

            var resetToken = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAtUtc = expiresAtUtc,
                UsedAtUtc = null
            };

            await userRepository.AddPasswordResetTokenAsync(
                resetToken, cancellationToken);

            await userRepository.SaveChangesAsync(cancellationToken);

            // Development only: return the raw token to the caller.
            return rawToken;
        }


        public async Task<bool> ResetPasswordAsync(
            string email,
            string token,
            string newPassword,
            CancellationToken cancellationToken)
        {
            var normalizedEmail = email.Trim().ToUpperInvariant();

            var user = await userRepository.FindByNormalizedEmailAsync(
                normalizedEmail, cancellationToken);

            if (user is null || !user.IsActive)
            {
                return false;
            }

            var tokenHash = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(token)));

            var resetToken = await userRepository.FindPasswordResetTokenAsync(
                user.Id, tokenHash, cancellationToken);

            if (resetToken is null)
            {
                return false;
            }

            user.PasswordHash = passwordHasher.Hash(newPassword);
            user.UpdatedAtUtc = DateTime.UtcNow;
            resetToken.UsedAtUtc = DateTime.UtcNow;

            await userRepository.SaveChangesAsync(cancellationToken);

            return true;
        }

    }
}
