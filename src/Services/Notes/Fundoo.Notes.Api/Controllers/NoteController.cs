using Fundoo.Notes.Application.DTOs;
using Fundoo.Notes.Application.Interfaces;
using Fundoo.Notes.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fundoo.Notes.Api.Controllers
{
    [ApiController]
    [Route("api/notes")]
    [Authorize]
    public class NotesController : ControllerBase
    {
        private readonly INoteRepository _noteRepository;
        private readonly INoteLabelRepository _noteLabelRepository;

        public NotesController(INoteRepository noteRepository, INoteLabelRepository noteLabelRepository)
        {
            _noteRepository = noteRepository;
            _noteLabelRepository = noteLabelRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateNoteRequest request,
            CancellationToken cancellationToken)
        {
            var userIdClaim =
    User.FindFirst("sub")?.Value
    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The JWT does not contain a valid GUID user ID."
                });
            }

            var note = new Note
            {
                UserId = userId,
                Title = request.Title.Trim(),
                Description = request.Description,
                Color = request.Color
            };

            var createdNote = await _noteRepository.CreateAsync(
                note, cancellationToken);

            return Created($"/api/notes/{createdNote.NoteId}", createdNote);
        }


        [HttpGet]
        public async Task<IActionResult> GetAll(
            CancellationToken cancellationToken)
        {
            var userIdClaim =
                User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The JWT does not contain a valid GUID user ID."
                });
            }

            var notes = await _noteRepository.GetAllByUserIdAsync(
                userId,
                cancellationToken);

            return Ok(notes);
        }


        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var userIdClaim =
                User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The JWT does not contain a valid GUID user ID."
                });
            }

            var note = await _noteRepository.GetByIdAsync(
                id,
                userId,
                cancellationToken);

            if (note is null)
            {
                return NotFound(new
                {
                    Message = "Note not found."
                });
            }

            return Ok(note);
        }

        [HttpGet("archived")]
        public async Task<IActionResult> GetArchived(
    CancellationToken cancellationToken)
        {
            var userIdClaim =
                User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The JWT does not contain a valid GUID user ID."
                });
            }

            var notes = await _noteRepository.GetArchivedByUserIdAsync(
                userId,
                cancellationToken);

            return Ok(notes);
        }

        [HttpGet("trash")]
        public async Task<IActionResult> GetTrashed(
    CancellationToken cancellationToken)
        {
            var userIdClaim =
                User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The JWT does not contain a valid GUID user ID."
                });
            }

            var notes = await _noteRepository.GetTrashedByUserIdAsync(
                userId,
                cancellationToken);

            return Ok(notes);
        }


        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateNoteRequest request,
            CancellationToken cancellationToken)
        {
            var userIdClaim =
                User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The JWT does not contain a valid GUID user ID."
                });
            }

            var note = new Note
            {
                NoteId = id,
                UserId = userId,
                Title = request.Title.Trim(),
                Description = request.Description,
                Color = request.Color,
                IsPinned = request.IsPinned
            };

            var updatedNote = await _noteRepository.UpdateAsync(
                id,
                userId,
                note,
                cancellationToken);

            if (updatedNote is null)
            {
                return NotFound(new
                {
                    Message = "Note not found."
                });
            }

            return Ok(updatedNote);
        }

        [HttpPut("{id:guid}/archive")]
        public async Task<IActionResult> Archive(
    Guid id,
    CancellationToken cancellationToken)
        {
            var userIdClaim =
                User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The JWT does not contain a valid GUID user ID."
                });
            }

            var success = await _noteRepository.SetArchivedAsync(
                id, userId, true, cancellationToken);

            if (!success)
            {
                return NotFound(new { Message = "Note not found." });
            }

            return NoContent();
        }

        [HttpPut("{id:guid}/unarchive")]
        public async Task<IActionResult> Unarchive(
    Guid id,
    CancellationToken cancellationToken)
        {
            var userIdClaim =
                User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The JWT does not contain a valid GUID user ID."
                });
            }

            var success = await _noteRepository.SetArchivedAsync(
                id, userId, false, cancellationToken);

            if (!success)
            {
                return NotFound(new { Message = "Note not found." });
            }

            return NoContent();
        }


        [HttpPut("{id:guid}/trash")]
        public async Task<IActionResult> Trash(
            Guid id,
            CancellationToken cancellationToken)
        {
            var userIdClaim =
                User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The JWT does not contain a valid GUID user ID."
                });
            }

            var success = await _noteRepository.SetTrashedAsync(
                id, userId, true, cancellationToken);

            if (!success)
            {
                return NotFound(new { Message = "Note not found." });
            }

            return NoContent();
        }


        [HttpPut("{id:guid}/restore")]
        public async Task<IActionResult> Restore(
            Guid id,
            CancellationToken cancellationToken)
        {
            var userIdClaim =
                User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The JWT does not contain a valid GUID user ID."
                });
            }

            var success = await _noteRepository.SetTrashedAsync(
                id, userId, false, cancellationToken);

            if (!success)
            {
                return NotFound(new { Message = "Note not found." });
            }

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePermanently(
    Guid id,
    CancellationToken cancellationToken)
        {
            var userIdClaim =
                User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The JWT does not contain a valid GUID user ID."
                });
            }

            var success = await _noteRepository.DeletePermanentlyAsync(
                id, userId, cancellationToken);

            if (!success)
            {
                return NotFound(new
                {
                    Message = "Trashed note not found."
                });
            }

            return NoContent();
        }


        [HttpPut("{noteId:guid}/labels/{labelId:guid}")]
        public async Task<IActionResult> AttachLabel(
            Guid noteId,
            Guid labelId,
            CancellationToken cancellationToken)
        {
            var userIdClaim =
                User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The JWT does not contain a valid GUID user ID."
                });
            }

            var attached = await _noteLabelRepository.AttachAsync(
                noteId, labelId, userId, cancellationToken);

            if (!attached)
            {
                return Conflict(new
                {
                    Message =
                        "Unable to attach label. Verify that the note and label belong to you and that the association does not already exist."
                });
            }

            return NoContent();
        }


        [HttpDelete("{noteId:guid}/labels/{labelId:guid}")]
        public async Task<IActionResult> RemoveLabel(
            Guid noteId,
            Guid labelId,
            CancellationToken cancellationToken)
        {
            var userIdClaim =
                User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The JWT does not contain a valid GUID user ID."
                });
            }

            var removed = await _noteLabelRepository.RemoveAsync(
                noteId, labelId, userId, cancellationToken);

            if (!removed)
            {
                return NotFound(new
                {
                    Message =
                        "Association not found, or the note or label does not belong to you."
                });
            }

            return NoContent();
        }



    }
}
