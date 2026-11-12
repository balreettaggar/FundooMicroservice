using Fundoo.Notes.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundoo.Notes.Infrastructure.Repository
{
    public class NoteLabelRepository : INoteLabelRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public NoteLabelRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> AttachAsync(
            Guid noteId,
            Guid labelId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(
                    cancellationToken);

            const string sql = """
            INSERT INTO note_labels (note_id, label_id)
            SELECT n.id, l.id
            FROM notes n
            CROSS JOIN labels l
            WHERE n.id = @noteId
              AND n.user_id = @userId
              AND l.id = @labelId
              AND l.user_id = @userId
            ON CONFLICT (note_id, label_id) DO NOTHING
            RETURNING note_id;
            """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var parameters = new (string Name, object Value)[]
            {
            ("noteId", noteId),
            ("labelId", labelId),
            ("userId", userId)
            };

            foreach (var (name, value) in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = value;
                command.Parameters.Add(parameter);
            }

            var result =
                await command.ExecuteScalarAsync(cancellationToken);

            return result is not null;
        }

        public async Task<bool> RemoveAsync(
            Guid noteId,
            Guid labelId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(
                    cancellationToken);

            const string sql = """
            DELETE FROM note_labels nl
            WHERE nl.note_id = @noteId
              AND nl.label_id = @labelId
              AND EXISTS (
                  SELECT 1
                  FROM notes n
                  WHERE n.id = nl.note_id
                    AND n.user_id = @userId
              )
              AND EXISTS (
                  SELECT 1
                  FROM labels l
                  WHERE l.id = nl.label_id
                    AND l.user_id = @userId
              );
            """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var parameters = new (string Name, object Value)[]
            {
            ("noteId", noteId),
            ("labelId", labelId),
            ("userId", userId)
            };

            foreach (var (name, value) in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = value;
                command.Parameters.Add(parameter);
            }

            var rowsAffected =
                await command.ExecuteNonQueryAsync(cancellationToken);

            return rowsAffected > 0;
        }
    }
}
