using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundoo.Notes.Application.Interfaces
{
    public interface IDbConnectionFactory
    {
        Task<DbConnection> CreateConnectionAsync(
            CancellationToken cancellationToken = default);
    }
}
