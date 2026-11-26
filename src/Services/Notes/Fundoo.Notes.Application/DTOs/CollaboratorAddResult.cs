using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundoo.Notes.Application.DTOs;

public enum CollaboratorAddStatus
{
    Added,
    NoteNotFound,
    UserNotFound,
    CannotAddSelf,
    AlreadyCollaborator
}

public sealed record CollaboratorAddResult(
    CollaboratorAddStatus Status,
    Guid? CollaboratorUserId = null);
