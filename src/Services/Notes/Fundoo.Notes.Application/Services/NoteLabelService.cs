using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Fundoo.Notes.Application.Interfaces;

namespace Fundoo.Notes.Application.Services;

public class NoteLabelService : INoteLabelService
{
    private readonly INoteLabelRepository _repository;
    private readonly INoteCache _noteCache;

    public NoteLabelService(INoteLabelRepository repository, INoteCache noteCache)
    {
        _repository = repository;
        _noteCache = noteCache;
    }

    public async Task<bool> AttachAsync(
        Guid noteId,
        Guid labelId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var changed = await _repository.AttachAsync(
            noteId, labelId, userId, cancellationToken);
        if (changed)
        {
            await _noteCache.RemoveAsync(userId, noteId, cancellationToken);
        }

        return changed;
    }

    public async Task<bool> RemoveAsync(
        Guid noteId,
        Guid labelId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var changed = await _repository.RemoveAsync(
            noteId, labelId, userId, cancellationToken);
        if (changed)
        {
            await _noteCache.RemoveAsync(userId, noteId, cancellationToken);
        }

        return changed;
    }
}

