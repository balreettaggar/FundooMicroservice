using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Fundoo.Notes.Application.Interfaces;
using Fundoo.Notes.Domain.Entities;

namespace Fundoo.Notes.Application.Services;

public class LabelService : ILabelService
{
    private readonly ILabelRepository _labelRepository;

    public LabelService(ILabelRepository labelRepository)
    {
        _labelRepository = labelRepository;
    }

    public Task<Label> CreateAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Label name cannot be empty.");

        var label = new Label
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim()
        };

        return _labelRepository.CreateAsync(label, cancellationToken);
    }

    public Task<IReadOnlyList<Label>> GetAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _labelRepository.GetAllByUserIdAsync(
            userId, cancellationToken);
    }

    public Task<Label?> RenameAsync(
        Guid labelId,
        Guid userId,
        string newName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Label name cannot be empty.");

        return _labelRepository.RenameAsync(
            labelId, userId, newName.Trim(), cancellationToken);
    }

    public Task<bool> DeleteAsync(
        Guid labelId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _labelRepository.DeleteAsync(
            labelId, userId, cancellationToken);
    }
}

