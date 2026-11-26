
using Fundoo.Notes.Application.Interfaces;
using Fundoo.Notes.Domain.Entities;
using Fundoo.Notes.Application.DTOs;

namespace Fundoo.Notes.Application.Services;

public class NoteService : INoteService
{
    private readonly INoteRepository _noteRepository;
    private readonly INoteCache _noteCache;
    private readonly IUserDirectoryClient _userDirectoryClient;

    public NoteService(
    INoteRepository noteRepository,
    INoteCache noteCache,
    IUserDirectoryClient userDirectoryClient)
    {
        _noteRepository = noteRepository;
        _noteCache = noteCache;
        _userDirectoryClient = userDirectoryClient;
    }

    // CREATE NOTE
    public async Task<Note> CreateAsync(
        Guid userId,
        string title,
        string description,
        string color,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.");

        var note = new Note
        {
            UserId = userId,
            Title = title.Trim(),
            Description = description,
            Color = color
        };

        var createdNote = await _noteRepository.CreateAsync(
            note, cancellationToken);

        // Store newly created note in Redis
        await _noteCache.SetAsync(
            createdNote, cancellationToken);

        return createdNote;
    }

    // GET ALL NOTES
    public Task<IReadOnlyList<Note>> GetAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _noteRepository.GetAllByUserIdAsync(
            userId, cancellationToken);
    }

    // GET NOTE BY ID - CACHE ASIDE
    public async Task<Note?> GetByIdAsync(
        Guid noteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Check Redis
        var cachedNote = await _noteCache.GetAsync(
            userId, noteId, cancellationToken);

        // Step 2: Cache hit
        if (cachedNote is not null)
        {
            return cachedNote;
        }

        // Step 3: Cache miss - fetch from PostgreSQL
        var note = await _noteRepository.GetByIdAsync(
            noteId, userId, cancellationToken);

        // Step 4: Store result in Redis
        if (note is not null)
        {
            await _noteCache.SetAsync(
                note, cancellationToken);
        }

        return note;
    }

    // GET ARCHIVED NOTES
    public Task<IReadOnlyList<Note>> GetArchivedByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _noteRepository.GetArchivedByUserIdAsync(
            userId, cancellationToken);
    }

    // GET TRASHED NOTES
    public Task<IReadOnlyList<Note>> GetTrashedByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _noteRepository.GetTrashedByUserIdAsync(
            userId, cancellationToken);
    }

    // UPDATE NOTE
    public async Task<NoteUpdateResult> UpdateAsync(
        Guid noteId,
        Guid userId,
        string title,
        string description,
        string color,
        bool isPinned,
        long expectedVersion,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.");

        var note = new Note
        {
            NoteId = noteId,
            UserId = userId,
            Title = title.Trim(),
            Description = description,
            Color = color,
            IsPinned = isPinned
        };

        var result = await _noteRepository.UpdateAsync(
            noteId, userId, note, expectedVersion, cancellationToken);

        if (result.Note is not null)
        {
            await _noteCache.SetAsync(
                result.Note, cancellationToken);
        }
        else
        {
            await _noteCache.RemoveAsync(
                userId, noteId, cancellationToken);
        }

        return result;
    }

    // ARCHIVE / UNARCHIVE NOTE
    public async Task<bool> SetArchivedAsync(
        Guid noteId,
        Guid userId,
        bool isArchived,
        CancellationToken cancellationToken = default)
    {
        var result = await _noteRepository.SetArchivedAsync(
            noteId, userId, isArchived, cancellationToken);

        if (result)
        {
            await _noteCache.RemoveAsync(
                userId, noteId, cancellationToken);
        }

        return result;
    }

    // TRASH / RESTORE NOTE
    public async Task<bool> SetTrashedAsync(
        Guid noteId,
        Guid userId,
        bool isTrashed,
        CancellationToken cancellationToken = default)
    {
        var result = await _noteRepository.SetTrashedAsync(
            noteId, userId, isTrashed, cancellationToken);

        if (result)
        {
            await _noteCache.RemoveAsync(
                userId, noteId, cancellationToken);
        }

        return result;
    }

    // PERMANENTLY DELETE NOTE
    public async Task<bool> DeletePermanentlyAsync(
        Guid noteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await _noteRepository.DeletePermanentlyAsync(
            noteId, userId, cancellationToken);

        if (result)
        {
            await _noteCache.RemoveAsync(
                userId, noteId, cancellationToken);
        }

        return result;
    }

    public async Task<CollaboratorAddResult> AddCollaboratorAsync(
    Guid ownerUserId,
    Guid noteId,
    string collaboratorEmail,
    CancellationToken cancellationToken = default)
    {
        var noteIsOwned = await _noteRepository.IsOwnedByUserAsync(
            ownerUserId,
            noteId,
            cancellationToken);

        if (!noteIsOwned)
        {
            return new CollaboratorAddResult(
                CollaboratorAddStatus.NoteNotFound);
        }

        var collaboratorUserId =
            await _userDirectoryClient.FindUserIdByEmailAsync(
                collaboratorEmail,
                cancellationToken);

        if (collaboratorUserId is null)
        {
            return new CollaboratorAddResult(
                CollaboratorAddStatus.UserNotFound);
        }

        if (collaboratorUserId.Value == ownerUserId)
        {
            return new CollaboratorAddResult(
                CollaboratorAddStatus.CannotAddSelf);
        }

        var wasAdded = await _noteRepository.AddCollaboratorAsync(
            ownerUserId,
            noteId,
            collaboratorUserId.Value,
            cancellationToken);

        if (!wasAdded)
        {
            // Recheck ownership in case the note changed while we were
            // looking up the collaborator.
            var noteIsStillOwned = await _noteRepository.IsOwnedByUserAsync(
                ownerUserId,
                noteId,
                cancellationToken);

            return new CollaboratorAddResult(
                noteIsStillOwned
                    ? CollaboratorAddStatus.AlreadyCollaborator
                    : CollaboratorAddStatus.NoteNotFound);
        }

        return new CollaboratorAddResult(
            CollaboratorAddStatus.Added,
            collaboratorUserId.Value);
    }

    public Task<IReadOnlyList<Guid>?> GetCollaboratorsAsync(
        Guid ownerUserId,
        Guid noteId,
        CancellationToken cancellationToken = default)
    {
        return _noteRepository.GetCollaboratorsAsync(
            ownerUserId,
            noteId,
            cancellationToken);
    }

    public Task<bool> RemoveCollaboratorAsync(
        Guid ownerUserId,
        Guid noteId,
        Guid collaboratorUserId,
        CancellationToken cancellationToken = default)
    {
        return _noteRepository.RemoveCollaboratorAsync(
            ownerUserId,
            noteId,
            collaboratorUserId,
            cancellationToken);
    }
}
