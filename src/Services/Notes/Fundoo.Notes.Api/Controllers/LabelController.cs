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
        private readonly ILabelService _labelService;

        public LabelsController(ILabelService labelService)
        {
            _labelService = labelService;
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

            var labels = await _labelService.GetAllByUserIdAsync(
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
                var createdLabel = await _labelService.CreateAsync(
                 userId, request.Name, cancellationToken);

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
                var label = await _labelService.RenameAsync(
                id, userId, request.Name, cancellationToken);

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

            var deleted = await _labelService.DeleteAsync(
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
