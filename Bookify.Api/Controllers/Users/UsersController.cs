using Asp.Versioning;
using Bookify.Api.Shared;
using Bookify.Application.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers.Users;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/users")]
public class UsersController(ISender sender) : ControllerBase
{
    private readonly ISender sender = sender;

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LogIn(
        LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsFailure ? Unauthorized(result.Error) : Ok(result.Value);
    }
}
