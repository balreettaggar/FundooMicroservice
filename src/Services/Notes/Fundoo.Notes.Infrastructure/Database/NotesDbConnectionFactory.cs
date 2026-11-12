using Fundoo.Notes.Application.Interfaces;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundoo.Notes.Infrastructure.Database
{
    public class NotesDbConnectionFactory(string connectionString)
    : IDbConnectionFactory
    {
        public async Task<DbConnection> CreateConnectionAsync(
            CancellationToken cancellationToken = default)
        {
            var connection = new NpgsqlConnection(connectionString);

            try
            {
                await connection.OpenAsync(cancellationToken);
                return connection;
            }
            catch
            {
                await connection.DisposeAsync();
                throw;
            }
        }
    }
}
