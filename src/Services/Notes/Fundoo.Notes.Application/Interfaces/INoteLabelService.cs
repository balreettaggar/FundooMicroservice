using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Fundoo.Notes.Application.Interfaces;

public interface INoteLabelService
{
    Task<bool> AttachAsync(
        Guid noteId,
        Guid labelId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(
        Guid noteId,
        Guid labelId,
        Guid userId,
        CancellationToken cancellationToken = default);
}

