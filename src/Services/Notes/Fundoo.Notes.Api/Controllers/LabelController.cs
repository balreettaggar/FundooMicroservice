using Fundoo.Notes.Application.DTOs;
using Fundoo.Notes.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Security.Claims;
using Fundoo.Notes.Domain.Entities;

namespace Fundoo.Notes.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/labels")]
    public class LabelsController : ControllerBase
    {
        private readonly ILabelRepository _labelRepository;

        public LabelsController(ILabelRepository labelRepository)
        {
            _labelRepository = labelRepository;
        }

        private bool TryGetUserId(out Guid userId)
        {
            var claim = User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(claim, out userId);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The JWT does not contain a valid GUID user ID."
                });
            }

            var labels = await _labelRepository.GetAllByUserIdAsync(
                userId, cancellationToken);

            return Ok(labels);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateLabelRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The JWT does not contain a valid GUID user ID."
                });
            }

            var name = request.Name.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new
                {
                    Message = "Label name cannot be empty."
                });
            }

            var label = new Label
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name
            };

            try
            {
                var createdLabel = await _labelRepository.CreateAsync(
                    label, cancellationToken);

                return StatusCode(
                    StatusCodes.Status201Created, createdLabel);
            }
            catch (PostgresException ex)
                when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Conflict(new
                {
                    Message = "You already have a label with this name."
                });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Rename(
    Guid id,
    [FromBody] CreateLabelRequest request,
    CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The JWT does not contain a valid GUID user ID."
                });
            }

            var name = request.Name.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new
                {
                    Message = "Label name cannot be empty."
                });
            }

            try
            {
                var label = await _labelRepository.RenameAsync(
                    id, userId, name, cancellationToken);

                if (label is null)
                {
                    return NotFound(new
                    {
                        Message = "Label not found."
                    });
                }

                return Ok(label);
            }
            catch (PostgresException ex)
                when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Conflict(new
                {
                    Message = "You already have a label with this name."
                });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(
    Guid id,
    CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The JWT does not contain a valid GUID user ID."
                });
            }

            var deleted = await _labelRepository.DeleteAsync(
                id, userId, cancellationToken);

            if (!deleted)
            {
                return NotFound(new
                {
                    Message = "Label not found."
                });
            }

            return NoContent();
        }
    }
}
