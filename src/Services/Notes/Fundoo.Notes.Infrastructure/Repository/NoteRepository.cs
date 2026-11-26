using Fundoo.Notes.Application.Interfaces;
using Fundoo.Notes.Domain.Entities;
using Npgsql;
using Fundoo.Notes.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundoo.Notes.Infrastructure.Repository
{
    public class NoteRepository : INoteRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public NoteRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Note> CreateAsync(
            Note note,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(cancellationToken);

            const string sql = """
    INSERT INTO notes
        (id, user_id, title, description, color)
    VALUES
        (@id, @userId, @title, @description, @color)
    RETURNING
        id,
        user_id,
        title,
        description,
        color,
        is_archived,
        is_trashed,
        is_pinned,
        created_at_utc,
        updated_at_utc,
        version;
    """;

            //await using var command = new NpgsqlCommand(sql, connection);

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "id";
            idParameter.Value = Guid.NewGuid();
            command.Parameters.Add(idParameter);

            var userIdParameter = command.CreateParameter();
            userIdParameter.ParameterName = "userId";
            userIdParameter.Value = note.UserId;
            command.Parameters.Add(userIdParameter);

            var titleParameter = command.CreateParameter();
            titleParameter.ParameterName = "title";
            titleParameter.Value = note.Title;
            command.Parameters.Add(titleParameter);

            var descriptionParameter = command.CreateParameter();
            descriptionParameter.ParameterName = "description";
            descriptionParameter.Value = note.Description;
            command.Parameters.Add(descriptionParameter);

            var colorParameter = command.CreateParameter();
            colorParameter.ParameterName = "color";
            colorParameter.Value = note.Color;
            command.Parameters.Add(colorParameter);
            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "The note was inserted, but no row was returned.");
            }

            var createdNote = new Note
            {
                NoteId = reader.GetGuid(reader.GetOrdinal("id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                Title = reader.GetString(reader.GetOrdinal("title")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                Color = reader.IsDBNull(reader.GetOrdinal("color"))
         ? "#FFFFFF"
         : reader.GetString(reader.GetOrdinal("color")),
                IsArchived = reader.GetBoolean(reader.GetOrdinal("is_archived")),
                IsDeleted = reader.GetBoolean(reader.GetOrdinal("is_trashed")),
                IsPinned = reader.GetBoolean(reader.GetOrdinal("is_pinned")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at_utc")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at_utc")),
                Version = reader.GetInt64(reader.GetOrdinal("version"))
            };

            return createdNote;
        }


        public async Task<IReadOnlyList<Note>> GetAllByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(
                    cancellationToken);

            const string sql = """
        SELECT
            id,
            user_id,
            title,
            description,
            color,
            is_archived,
            is_trashed,
            is_pinned,
            created_at_utc,
            updated_at_utc,
            version
        FROM notes
        WHERE (notes.user_id = @userId OR EXISTS (
                  SELECT 1 FROM note_collaborators nc
                  WHERE nc.note_id = notes.id AND nc.collaborator_user_id = @userId
              ))
          AND notes.is_archived = FALSE
          AND notes.is_trashed = FALSE
        ORDER BY notes.created_at_utc DESC;
        """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var userIdParameter = command.CreateParameter();
            userIdParameter.ParameterName = "userId";
            userIdParameter.Value = userId;
            command.Parameters.Add(userIdParameter);

            var notes = new List<Note>();

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var note = new Note
                {
                    NoteId = reader.GetGuid(reader.GetOrdinal("id")),

                    UserId = reader.GetGuid(
                        reader.GetOrdinal("user_id")),

                    Title = reader.GetString(
                        reader.GetOrdinal("title")),

                    Description = reader.IsDBNull(
                        reader.GetOrdinal("description"))
                        ? string.Empty
                        : reader.GetString(
                            reader.GetOrdinal("description")),

                    Color = reader.IsDBNull(
                        reader.GetOrdinal("color"))
                        ? "#FFFFFF"
                        : reader.GetString(
                            reader.GetOrdinal("color")),

                    IsArchived = reader.GetBoolean(
                        reader.GetOrdinal("is_archived")),

                    IsDeleted = reader.GetBoolean(
                        reader.GetOrdinal("is_trashed")),

                    IsPinned = reader.GetBoolean(
                        reader.GetOrdinal("is_pinned")),

                    CreatedAt = reader.GetDateTime(
                        reader.GetOrdinal("created_at_utc")),

                    UpdatedAt = reader.GetDateTime(
                        reader.GetOrdinal("updated_at_utc")),
                    Version = reader.GetInt64(reader.GetOrdinal("version"))
                };

                notes.Add(note);
            }

            return notes;
        }


        public async Task<Note?> GetByIdAsync(
            Guid noteId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(
                    cancellationToken);

            const string sql = """
        SELECT
            id,
            user_id,
            title,
            description,
            color,
            is_archived,
            is_trashed,
            is_pinned,
            created_at_utc,
            updated_at_utc,
            version
        FROM notes
        WHERE notes.id = @noteId
        AND notes.is_trashed = FALSE
        AND (
            notes.user_id = @userId
            OR EXISTS (
                SELECT 1
                FROM note_collaborators nc
                WHERE nc.note_id = notes.id
                  AND nc.collaborator_user_id = @userId
            )
        );
        """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var noteIdParameter = command.CreateParameter();
            noteIdParameter.ParameterName = "noteId";
            noteIdParameter.Value = noteId;
            command.Parameters.Add(noteIdParameter);

            var userIdParameter = command.CreateParameter();
            userIdParameter.ParameterName = "userId";
            userIdParameter.Value = userId;
            command.Parameters.Add(userIdParameter);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return new Note
            {
                NoteId = reader.GetGuid(reader.GetOrdinal("id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                Title = reader.GetString(reader.GetOrdinal("title")),

                Description = reader.IsDBNull(
                    reader.GetOrdinal("description"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("description")),

                Color = reader.IsDBNull(reader.GetOrdinal("color"))
                    ? "#FFFFFF"
                    : reader.GetString(reader.GetOrdinal("color")),

                IsArchived = reader.GetBoolean(
                    reader.GetOrdinal("is_archived")),

                IsDeleted = reader.GetBoolean(
                    reader.GetOrdinal("is_trashed")),

                IsPinned = reader.GetBoolean(
                    reader.GetOrdinal("is_pinned")),

                CreatedAt = reader.GetDateTime(
                    reader.GetOrdinal("created_at_utc")),

                UpdatedAt = reader.GetDateTime(
                    reader.GetOrdinal("updated_at_utc")),
                Version = reader.GetInt64(reader.GetOrdinal("version"))
            };
        }


        public async Task<IReadOnlyList<Note>> GetArchivedByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(
                    cancellationToken);

            const string sql = """
        SELECT
            id,
            user_id,
            title,
            description,
            color,
            is_archived,
            is_trashed,
            is_pinned,
            created_at_utc,
            updated_at_utc,
            version
        FROM notes
        WHERE
            (
                notes.user_id = @userId
                OR EXISTS (
                    SELECT 1
                    FROM note_collaborators nc
                    WHERE nc.note_id = notes.id
                      AND nc.collaborator_user_id = @userId
                )
            )
            AND notes.is_archived = TRUE
            AND notes.is_trashed = FALSE
        ORDER BY notes.updated_at_utc DESC;
        """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var parameter = command.CreateParameter();
            parameter.ParameterName = "userId";
            parameter.Value = userId;
            command.Parameters.Add(parameter);

            var notes = new List<Note>();

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                notes.Add(new Note
                {
                    NoteId = reader.GetGuid(reader.GetOrdinal("id")),
                    UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                    Title = reader.GetString(reader.GetOrdinal("title")),
                    Description = reader.IsDBNull(reader.GetOrdinal("description"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("description")),
                    Color = reader.IsDBNull(reader.GetOrdinal("color"))
                        ? "#FFFFFF"
                        : reader.GetString(reader.GetOrdinal("color")),
                    IsArchived = reader.GetBoolean(reader.GetOrdinal("is_archived")),
                    IsDeleted = reader.GetBoolean(reader.GetOrdinal("is_trashed")),
                    IsPinned = reader.GetBoolean(reader.GetOrdinal("is_pinned")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at_utc")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at_utc")),
                    Version = reader.GetInt64(reader.GetOrdinal("version"))
                });
            }

            return notes;
        }

        public async Task<IReadOnlyList<Note>> GetTrashedByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(
                    cancellationToken);

            const string sql = """
    SELECT
        id,
        user_id,
        title,
        description,
        color,
        is_archived,
        is_trashed,
        is_pinned,
        created_at_utc,
        updated_at_utc,
        version
    FROM notes
    WHERE user_id = @userId
      AND is_trashed = TRUE
    ORDER BY updated_at_utc DESC;
    """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var parameter = command.CreateParameter();
            parameter.ParameterName = "userId";
            parameter.Value = userId;
            command.Parameters.Add(parameter);

            var notes = new List<Note>();

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                notes.Add(new Note
                {
                    NoteId = reader.GetGuid(reader.GetOrdinal("id")),
                    UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                    Title = reader.GetString(reader.GetOrdinal("title")),
                    Description = reader.IsDBNull(reader.GetOrdinal("description"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("description")),
                    Color = reader.IsDBNull(reader.GetOrdinal("color"))
                        ? "#FFFFFF"
                        : reader.GetString(reader.GetOrdinal("color")),
                    IsArchived = reader.GetBoolean(reader.GetOrdinal("is_archived")),
                    IsDeleted = reader.GetBoolean(reader.GetOrdinal("is_trashed")),
                    IsPinned = reader.GetBoolean(reader.GetOrdinal("is_pinned")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at_utc")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at_utc")),
                    Version = reader.GetInt64(reader.GetOrdinal("version"))
                });
            }

            return notes;
        }

        public async Task<bool> IsOwnedByUserAsync(
    Guid userId,
    Guid noteId,
    CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(cancellationToken);

            const string sql = """
        SELECT EXISTS (
            SELECT 1
            FROM notes
            WHERE id = @noteId
              AND user_id = @userId
              AND is_trashed = FALSE
        );
        """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var noteIdParameter = command.CreateParameter();
            noteIdParameter.ParameterName = "noteId";
            noteIdParameter.Value = noteId;
            command.Parameters.Add(noteIdParameter);

            var userIdParameter = command.CreateParameter();
            userIdParameter.ParameterName = "userId";
            userIdParameter.Value = userId;
            command.Parameters.Add(userIdParameter);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is true;
        }

        public async Task<bool> AddCollaboratorAsync(
            Guid ownerUserId,
            Guid noteId,
            Guid collaboratorUserId,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(cancellationToken);

            const string sql = """
        INSERT INTO note_collaborators
            (note_id, collaborator_user_id, created_at_utc)
        SELECT
            @noteId, @collaboratorUserId, CURRENT_TIMESTAMP
        WHERE EXISTS (
            SELECT 1
            FROM notes
            WHERE id = @noteId
              AND user_id = @ownerUserId
              AND is_trashed = FALSE
        )
        ON CONFLICT (note_id, collaborator_user_id) DO NOTHING;
        """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var noteIdParameter = command.CreateParameter();
            noteIdParameter.ParameterName = "noteId";
            noteIdParameter.Value = noteId;
            command.Parameters.Add(noteIdParameter);

            var collaboratorParameter = command.CreateParameter();
            collaboratorParameter.ParameterName = "collaboratorUserId";
            collaboratorParameter.Value = collaboratorUserId;
            command.Parameters.Add(collaboratorParameter);

            var ownerParameter = command.CreateParameter();
            ownerParameter.ParameterName = "ownerUserId";
            ownerParameter.Value = ownerUserId;
            command.Parameters.Add(ownerParameter);

            var rowsInserted = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsInserted > 0;
        }

        public async Task<IReadOnlyList<Guid>?> GetCollaboratorsAsync(
            Guid ownerUserId,
            Guid noteId,
            CancellationToken cancellationToken = default)
        {
            if (!await IsOwnedByUserAsync(ownerUserId, noteId, cancellationToken))
            {
                return null;
            }

            await using var connection =
                await _connectionFactory.CreateConnectionAsync(cancellationToken);

            const string sql = """
        SELECT collaborator_user_id
        FROM note_collaborators
        WHERE note_id = @noteId
        ORDER BY created_at_utc;
        """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var noteIdParameter = command.CreateParameter();
            noteIdParameter.ParameterName = "noteId";
            noteIdParameter.Value = noteId;
            command.Parameters.Add(noteIdParameter);

            var collaboratorIds = new List<Guid>();

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                collaboratorIds.Add(reader.GetGuid(0));
            }

            return collaboratorIds;
        }

        public async Task<bool> RemoveCollaboratorAsync(
            Guid ownerUserId,
            Guid noteId,
            Guid collaboratorUserId,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(cancellationToken);

            const string sql = """
        DELETE FROM note_collaborators nc
        USING notes n
        WHERE nc.note_id = n.id
          AND n.id = @noteId
          AND n.user_id = @ownerUserId
          AND nc.collaborator_user_id = @collaboratorUserId;
        """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var noteIdParameter = command.CreateParameter();
            noteIdParameter.ParameterName = "noteId";
            noteIdParameter.Value = noteId;
            command.Parameters.Add(noteIdParameter);

            var ownerParameter = command.CreateParameter();
            ownerParameter.ParameterName = "ownerUserId";
            ownerParameter.Value = ownerUserId;
            command.Parameters.Add(ownerParameter);

            var collaboratorParameter = command.CreateParameter();
            collaboratorParameter.ParameterName = "collaboratorUserId";
            collaboratorParameter.Value = collaboratorUserId;
            command.Parameters.Add(collaboratorParameter);

            var rowsDeleted = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsDeleted > 0;
        }
        public async Task<NoteUpdateResult> UpdateAsync(
            Guid noteId,
            Guid userId,
            Note note,
            long expectedVersion,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(
                    cancellationToken);

            const string sql = """
        UPDATE notes
        SET
            title = @title,
            description = @description,
            color = @color,
            is_pinned = @isPinned,
            updated_at_utc = CURRENT_TIMESTAMP,
            version = version + 1
        WHERE notes.id = @noteId
          AND notes.version = @expectedVersion
          AND notes.is_trashed = FALSE
          AND (
              notes.user_id = @userId
              OR EXISTS (
                  SELECT 1
                  FROM note_collaborators nc
                  WHERE nc.note_id = notes.id
                    AND nc.collaborator_user_id = @userId
              )
          )
        RETURNING
            id,
            user_id,
            title,
            description,
            color,
            is_archived,
            is_trashed,
            is_pinned,
            created_at_utc,
            updated_at_utc,
            version;
        """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var parameters = new (string Name, object Value)[]
            {
        ("noteId", noteId),
        ("userId", userId),
        ("title", note.Title),
        ("description", note.Description),
        ("color", note.Color),
        ("isPinned", note.IsPinned),
        ("expectedVersion", expectedVersion)
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
                await reader.DisposeAsync();
                await using var versionCommand = connection.CreateCommand();
                versionCommand.CommandText = """
                    SELECT n.version
                    FROM notes n
                    WHERE n.id = @noteId
                      AND (
                          n.user_id = @userId
                          OR EXISTS (
                              SELECT 1 FROM note_collaborators nc
                              WHERE nc.note_id = n.id
                                AND nc.collaborator_user_id = @userId
                          )
                      );
                    """;

                var versionNoteId = versionCommand.CreateParameter();
                versionNoteId.ParameterName = "noteId";
                versionNoteId.Value = noteId;
                versionCommand.Parameters.Add(versionNoteId);

                var versionUserId = versionCommand.CreateParameter();
                versionUserId.ParameterName = "userId";
                versionUserId.Value = userId;
                versionCommand.Parameters.Add(versionUserId);

                var currentVersion = await versionCommand.ExecuteScalarAsync(cancellationToken);
                return currentVersion is long version
                    ? new NoteUpdateResult(NoteUpdateStatus.Conflict, CurrentVersion: version)
                    : new NoteUpdateResult(NoteUpdateStatus.NotFound);
            }

            var updatedNote = new Note
            {
                NoteId = reader.GetGuid(reader.GetOrdinal("id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                Title = reader.GetString(reader.GetOrdinal("title")),

                Description = reader.IsDBNull(
                    reader.GetOrdinal("description"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("description")),

                Color = reader.IsDBNull(reader.GetOrdinal("color"))
                    ? "#FFFFFF"
                    : reader.GetString(reader.GetOrdinal("color")),

                IsArchived = reader.GetBoolean(
                    reader.GetOrdinal("is_archived")),

                IsDeleted = reader.GetBoolean(
                    reader.GetOrdinal("is_trashed")),

                IsPinned = reader.GetBoolean(
                    reader.GetOrdinal("is_pinned")),

                CreatedAt = reader.GetDateTime(
                    reader.GetOrdinal("created_at_utc")),

                UpdatedAt = reader.GetDateTime(
                    reader.GetOrdinal("updated_at_utc")),
                Version = reader.GetInt64(reader.GetOrdinal("version"))
            };

            return new NoteUpdateResult(NoteUpdateStatus.Updated, updatedNote);
        }


        public async Task<bool> SetArchivedAsync(
            Guid noteId,
            Guid userId,
            bool isArchived,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(
                    cancellationToken);

            const string sql = """
        UPDATE notes
        SET
            is_archived = @isArchived,
            updated_at_utc = CURRENT_TIMESTAMP,
            version = version + 1
        WHERE id = @noteId
          AND user_id = @userId
          AND is_trashed = FALSE;
        """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var parameters = new (string Name, object Value)[]
            {
        ("noteId", noteId),
        ("userId", userId),
        ("isArchived", isArchived)
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


        public async Task<bool> SetTrashedAsync(
            Guid noteId,
            Guid userId,
            bool isTrashed,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(
                    cancellationToken);

            const string sql = """
        UPDATE notes
        SET
            is_trashed = @isTrashed,
            updated_at_utc = CURRENT_TIMESTAMP,
            version = version + 1
        WHERE id = @noteId
          AND user_id = @userId
          AND is_trashed = @expectedTrashed;
        """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var parameters = new (string Name, object Value)[]
            {
        ("noteId", noteId),
        ("userId", userId),
        ("isTrashed", isTrashed),
        ("expectedTrashed", !isTrashed)
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


        public async Task<bool> DeletePermanentlyAsync(
            Guid noteId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync(
                    cancellationToken);

            const string sql = """
        DELETE FROM notes
        WHERE id = @noteId
          AND user_id = @userId
          AND is_trashed = TRUE;
        """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var noteIdParameter = command.CreateParameter();
            noteIdParameter.ParameterName = "noteId";
            noteIdParameter.Value = noteId;
            command.Parameters.Add(noteIdParameter);

            var userIdParameter = command.CreateParameter();
            userIdParameter.ParameterName = "userId";
            userIdParameter.Value = userId;
            command.Parameters.Add(userIdParameter);

            var rowsAffected =
                await command.ExecuteNonQueryAsync(cancellationToken);

            return rowsAffected > 0;
        }


    }
}
