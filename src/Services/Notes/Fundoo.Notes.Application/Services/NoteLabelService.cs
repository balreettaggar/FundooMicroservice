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

    public NoteLabelService(INoteLabelRepository repository)
    {
        _repository = repository;
    }

    public Task<bool> AttachAsync(
        Guid noteId,
        Guid labelId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _repository.AttachAsync(
            noteId, labelId, userId, cancellationToken);
    }

    public Task<bool> RemoveAsync(
        Guid noteId,
        Guid labelId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _repository.RemoveAsync(
            noteId, labelId, userId, cancellationToken);
    }
}

