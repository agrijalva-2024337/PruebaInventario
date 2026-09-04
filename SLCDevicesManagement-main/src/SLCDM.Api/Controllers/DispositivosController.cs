using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SLCDM.Api.Authentication;
using SLCDM.Api.Extensions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.Dispositivos;
using SLCDM.Application.Features.Dispositivos.Commands;
using SLCDM.Application.Features.Dispositivos.Queries;

namespace SLCDM.Api.Controllers;

public sealed class DispositivosController : ApiControllerBase
{
    private readonly ICommandHandler<RegistrarDispositivoCommand, DispositivoTokenDto> _registrar;
    private readonly ICommandHandler<AutoRegistrarDispositivoCommand, DispositivoTokenDto> _autoRegistrar;
    private readonly ICommandHandler<RevocarDispositivoCommand> _revocar;
    private readonly ICommandHandler<RegistrarUbicacionCommand> _ping;
    private readonly IQueryHandler<GetDispositivosFueraDeRangoQuery, IReadOnlyList<DispositivoFueraDeRangoDto>> _fueraDeRango;
    private readonly IQueryHandler<GetDispositivosRastreoQuery, IReadOnlyList<DispositivoRastreoDto>> _rastreo;
    private readonly IQueryHandler<GetRastreoByActivoQuery, DispositivoRastreoDto> _rastreoByActivo;

    public DispositivosController(
        ICommandHandler<RegistrarDispositivoCommand, DispositivoTokenDto> registrar,
        ICommandHandler<AutoRegistrarDispositivoCommand, DispositivoTokenDto> autoRegistrar,
        ICommandHandler<RevocarDispositivoCommand> revocar,
        ICommandHandler<RegistrarUbicacionCommand> ping,
        IQueryHandler<GetDispositivosFueraDeRangoQuery, IReadOnlyList<DispositivoFueraDeRangoDto>> fueraDeRango,
        IQueryHandler<GetDispositivosRastreoQuery, IReadOnlyList<DispositivoRastreoDto>> rastreo,
        IQueryHandler<GetRastreoByActivoQuery, DispositivoRastreoDto> rastreoByActivo)
    {
        _registrar = registrar;
        _autoRegistrar = autoRegistrar;
        _revocar = revocar;
        _ping = ping;
        _fueraDeRango = fueraDeRango;
        _rastreo = rastreo;
        _rastreoByActivo = rastreoByActivo;
    }

    [HttpGet("fuera-de-rango")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<DispositivoFueraDeRangoDto>>> FueraDeRango(
        CancellationToken cancellationToken) =>
        Ok(await _fueraDeRango.HandleAsync(new GetDispositivosFueraDeRangoQuery(), cancellationToken));

    [HttpGet("mapa")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<DispositivoRastreoDto>>> Mapa(
        CancellationToken cancellationToken) =>
        Ok(await _rastreo.HandleAsync(new GetDispositivosRastreoQuery(), cancellationToken));

    [HttpGet("activos/{idActivo:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<DispositivoRastreoDto>> PorActivo(
        int idActivo,
        CancellationToken cancellationToken) =>
        Ok(await _rastreoByActivo.HandleAsync(new GetRastreoByActivoQuery(idActivo), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarDispositivoCommand command,
        CancellationToken cancellationToken)
    {
        var dto = await _registrar.HandleAsync(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    [HttpPost("auto-registro")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.DeviceAutoRegistroPolicy)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AutoRegistrar(
        [FromBody] AutoRegistrarDispositivoCommand command,
        CancellationToken cancellationToken)
    {
        var dto = await _autoRegistrar.HandleAsync(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    [HttpPost("{id:int}/revocar")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Revocar(int id, CancellationToken cancellationToken)
    {
        await _revocar.HandleAsync(new RevocarDispositivoCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("ping")]
    [Authorize(AuthenticationSchemes = DeviceTokenDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Ping(
        [FromBody] DevicePingRequest body,
        CancellationToken cancellationToken)
    {
        var idActivoClaim = User.FindFirstValue(DeviceClaimTypes.IdActivo);
        if (!int.TryParse(idActivoClaim, out var idActivo))
        {
            return Unauthorized();
        }

        await _ping.HandleAsync(
            new RegistrarUbicacionCommand(idActivo, body.Bssid, body.Latitud, body.Longitud),
            cancellationToken);
        return NoContent();
    }
}

public sealed record DevicePingRequest(string? Bssid, decimal? Latitud, decimal? Longitud);
