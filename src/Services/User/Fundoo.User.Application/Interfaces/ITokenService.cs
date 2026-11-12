using Fundoo.User.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundoo.User.Application.Interfaces
{
    public interface ITokenService
    {
        string CreateAccessToken(AppUser user);
    }
}
