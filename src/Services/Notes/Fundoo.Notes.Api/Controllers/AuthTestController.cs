
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fundoo.Notes.Api.Controllers;

[ApiController]
[Route("api/auth-test")]
public class AuthTestController : ControllerBase
{
    [Authorize]
    [HttpGet]
    public IActionResult Get()
    {
        var userId =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new
            {
                Message = "User ID claim is missing from the JWT."
            });
        }

        return Ok(new
        {
            Message = "JWT validation succeeded.",
            UserId = userId
        });
    }
}
