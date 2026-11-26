using Fundoo.Notes.Domain.Entities;

namespace Fundoo.Notes.Application.DTOs;

public enum NoteUpdateStatus
{
    Updated,
    Conflict,
    NotFound
}

public sealed record NoteUpdateResult(
    NoteUpdateStatus Status,
    Note? Note = null,
    long? CurrentVersion = null);
