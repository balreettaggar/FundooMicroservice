using Fundoo.Notes.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundoo.Notes.Application.Interfaces
{
    public interface INoteRepository
    {
        Task<Note> CreateAsync(
            Note note,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Note>> GetAllByUserIdAsync(
           Guid userId,
           CancellationToken cancellationToken = default);

        Task<Note?> GetByIdAsync(
    Guid noteId,
    Guid userId,
    CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Note>> GetArchivedByUserIdAsync(
    Guid userId,
    CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Note>> GetTrashedByUserIdAsync(
    Guid userId,
    CancellationToken cancellationToken = default);

        Task<Note?> UpdateAsync(
    Guid noteId,
    Guid userId,
    Note note,
    CancellationToken cancellationToken = default);

        Task<bool> SetArchivedAsync(
    Guid noteId,
    Guid userId,
    bool isArchived,
    CancellationToken cancellationToken = default);

    Task<bool> SetTrashedAsync(
    Guid noteId,
    Guid userId,
    bool isTrashed,
    CancellationToken cancellationToken = default);

        Task<bool> DeletePermanentlyAsync(
    Guid noteId,
    Guid userId,
    CancellationToken cancellationToken = default);

    }
}
