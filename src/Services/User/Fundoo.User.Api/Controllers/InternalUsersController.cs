using Fundoo.User.Api.Contract;
using Fundoo.User.Api.Services;
using Fundoo.User.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fundoo.User.Api.Controllers;

[ApiController]
[Route("internal/users")]
public sealed class InternalUsersController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly InternalServiceKeyValidator _serviceKeyValidator;

    public InternalUsersController(
        IAuthService authService,
        InternalServiceKeyValidator serviceKeyValidator)
    {
        _authService = authService;
        _serviceKeyValidator = serviceKeyValidator;
    }

    [HttpPost("lookup")]
    public async Task<IActionResult> FindByEmail(
        [FromBody] UserLookupRequest request,
        [FromHeader(Name = "X-Fundoo-Service-Key")] string? serviceKey,
        CancellationToken cancellationToken)
    {
        if (!_serviceKeyValidator.IsValid(serviceKey))
        {
            return Unauthorized();
        }

        var userId = await _authService.FindUserIdByEmailAsync(
            request.Email,
            cancellationToken);

        if (userId is null)
        {
            return NotFound();
        }

        return Ok(new { UserId = userId.Value });
    }
}