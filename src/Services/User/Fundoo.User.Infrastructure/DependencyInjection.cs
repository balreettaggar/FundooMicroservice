using Fundoo.User.Application.Interfaces;
using Fundoo.User.Infrastructure.Persistence;
using Fundoo.User.Infrastructure.Repositories;
using Fundoo.User.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fundoo.User.Application.Services;

namespace Fundoo.User.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddUserInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("UserDb")
                ?? throw new InvalidOperationException(
                    "UserDb connection string is missing.");

            services.AddDbContext<UserDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
