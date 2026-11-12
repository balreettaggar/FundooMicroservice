using Fundoo.Notes.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fundoo.Notes.Application.Interfaces;
using Fundoo.Notes.Domain.Entities;


namespace Fundoo.Notes.Infrastructure.Repository
{
    public class LabelRepository : ILabelRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public LabelRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Label> CreateAsync(
            Label label,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(
                    cancellationToken);

            const string sql = """
            INSERT INTO labels (id, user_id, name)
            VALUES (@id, @userId, @name)
            RETURNING id, user_id, name, created_at_utc;
            """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var parameters = new (string Name, object Value)[]
            {
            ("id", label.Id),
            ("userId", label.UserId),
            ("name", label.Name)
            };

            foreach (var (name, value) in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = value;
                command.Parameters.Add(parameter);
            }

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "Label was inserted, but no row was returned.");
            }

            return new Label
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                CreatedAtUtc = reader.GetDateTime(
                    reader.GetOrdinal("created_at_utc"))
            };
        }

        public async Task<IReadOnlyList<Label>> GetAllByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(
                    cancellationToken);

            const string sql = """
            SELECT id, user_id, name, created_at_utc
            FROM labels
            WHERE user_id = @userId
            ORDER BY name;
            """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var parameter = command.CreateParameter();
            parameter.ParameterName = "userId";
            parameter.Value = userId;
            command.Parameters.Add(parameter);

            var labels = new List<Label>();

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                labels.Add(new Label
                {
                    Id = reader.GetGuid(reader.GetOrdinal("id")),
                    UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    CreatedAtUtc = reader.GetDateTime(
                        reader.GetOrdinal("created_at_utc"))
                });
            }

            return labels;
        }


        public async Task<Label?> RenameAsync(
            Guid labelId,
            Guid userId,
            string newName,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(
                    cancellationToken);

            const string sql = """
        UPDATE labels
        SET name = @name
        WHERE id = @labelId
          AND user_id = @userId
        RETURNING id, user_id, name, created_at_utc;
        """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var parameters = new (string Name, object Value)[]
            {
        ("labelId", labelId),
        ("userId", userId),
        ("name", newName)
            };

            foreach (var (name, value) in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = value;
                command.Parameters.Add(parameter);
            }

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return new Label
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                CreatedAtUtc = reader.GetDateTime(
                    reader.GetOrdinal("created_at_utc"))
            };
        }


        public async Task<bool> DeleteAsync(
            Guid labelId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(
                    cancellationToken);

            const string sql = """
        DELETE FROM labels
        WHERE id = @labelId
          AND user_id = @userId;
        """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var parameters = new (string Name, object Value)[]
            {
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
