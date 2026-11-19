using Fundoo.User.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundoo.User.Infrastructure.Persistence
{
    public class UserDbContext(
        DbContextOptions<UserDbContext> options) : DbContext(options)
    {
        public DbSet<AppUser> Users => Set<AppUser>();

        public DbSet<PasswordResetToken> PasswordResetTokens =>
            Set<PasswordResetToken>();
        public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasKey(user => user.Id);

                entity.HasIndex(user => user.NormalizedEmail)
                    .IsUnique();

                entity.Property(user => user.FirstName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(user => user.LastName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(user => user.Email)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(user => user.NormalizedEmail)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(user => user.PasswordHash)
                    .IsRequired();
            });

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(token => token.Id);

                entity.HasIndex(token => token.TokenHash)
                    .IsUnique();

                entity.HasIndex(token => token.UserId);

                entity.Property(token => token.TokenHash)
                    .IsRequired();
            });

            modelBuilder.Entity<ExternalLogin>(entity =>
            {
                entity.HasKey(login => login.Id);
                entity.HasIndex(login => new { login.Provider, login.ProviderSubject }).IsUnique();
                entity.Property(login => login.Provider).HasMaxLength(100).IsRequired();
                entity.Property(login => login.ProviderSubject).HasMaxLength(256).IsRequired();
                entity.HasOne(login => login.User)
                    .WithMany(user => user.ExternalLogins)
                    .HasForeignKey(login => login.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
