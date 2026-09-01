using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SLCDM.Api.Extensions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.Auth;
using SLCDM.Application.Features.Auth.Commands;
using SLCDM.Application.Features.Auth.Queries;

namespace SLCDM.Api.Controllers;

[Route("api/auth")]
public sealed class AuthController : ApiControllerBase
{
    private readonly ICommandHandler<LoginCommand, LoginResponseDto> _login;
    private readonly IQueryHandler<GetMeQuery, AuthenticatedUserDto> _getMe;

    public AuthController(
        ICommandHandler<LoginCommand, LoginResponseDto> login,
        IQueryHandler<GetMeQuery, AuthenticatedUserDto> getMe)
    {
        _login = login;
        _getMe = getMe;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResponseDto>> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await _login.HandleAsync(command, cancellationToken));
    }

    [HttpGet("profile")]
    [Authorize(Roles = Roles.Lectura)]
    [ProducesResponseType(typeof(AuthenticatedUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticatedUserDto>> GetProfile(CancellationToken cancellationToken)
    {
        return Ok(await _getMe.HandleAsync(new GetMeQuery(), cancellationToken));
    }
}
