using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fundoo.Notes.Domain.Entities;

namespace Fundoo.Notes.Application.Interfaces
{
    public interface ILabelRepository
    {
        Task<Label> CreateAsync(
            Label label,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Label>> GetAllByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<Label?> RenameAsync(
    Guid labelId,
    Guid userId,
    string newName,
    CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            Guid labelId,
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
