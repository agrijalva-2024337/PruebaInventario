using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.Asignaciones;
using SLCDM.Application.Features.Asignaciones.Commands;
using SLCDM.Application.Features.Asignaciones.Queries;

namespace SLCDM.Api.Controllers;

public sealed class AsignacionesController : ApiControllerBase
{
    private readonly IQueryHandler<GetAsignacionesQuery, IReadOnlyList<AsignacionDto>> _getAll;
    private readonly IQueryHandler<GetAsignacionByIdQuery, AsignacionDto> _getById;
    private readonly IQueryHandler<GetHistorialAsignacionesPorActivoQuery, IReadOnlyList<AsignacionHistorialDto>> _historial;
    private readonly ICommandHandler<CreateAsignacionCommand, int> _entregar;
    private readonly ICommandHandler<CreateTrasladoCommand, int> _trasladar;
    private readonly ICommandHandler<CreateMantenimientoCommand, int> _iniciarMantenimiento;
    private readonly ICommandHandler<CreateBajaCommand, int> _darDeBaja;
    private readonly ICommandHandler<FinalizarMantenimientoCommand> _finalizarMantenimiento;
    private readonly ICommandHandler<UpdateAsignacionCommand> _update;
    private readonly ICommandHandler<DevolverAsignacionCommand> _devolver;

    public AsignacionesController(
        IQueryHandler<GetAsignacionesQuery, IReadOnlyList<AsignacionDto>> getAll,
        IQueryHandler<GetAsignacionByIdQuery, AsignacionDto> getById,
        IQueryHandler<GetHistorialAsignacionesPorActivoQuery, IReadOnlyList<AsignacionHistorialDto>> historial,
        ICommandHandler<CreateAsignacionCommand, int> entregar,
        ICommandHandler<CreateTrasladoCommand, int> trasladar,
        ICommandHandler<CreateMantenimientoCommand, int> iniciarMantenimiento,
        ICommandHandler<CreateBajaCommand, int> darDeBaja,
        ICommandHandler<FinalizarMantenimientoCommand> finalizarMantenimiento,
        ICommandHandler<UpdateAsignacionCommand> update,
        ICommandHandler<DevolverAsignacionCommand> devolver)
    {
        _getAll = getAll;
        _getById = getById;
        _historial = historial;
        _entregar = entregar;
        _trasladar = trasladar;
        _iniciarMantenimiento = iniciarMantenimiento;
        _darDeBaja = darDeBaja;
        _finalizarMantenimiento = finalizarMantenimiento;
        _update = update;
        _devolver = devolver;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<AsignacionDto>>> GetAll(
        [FromQuery] int? idActivo = null,
        [FromQuery] int? idUsuario = null,
        [FromQuery] bool? soloActivas = null,
        CancellationToken cancellationToken = default) =>
        Ok(await _getAll.HandleAsync(new GetAsignacionesQuery(idActivo, idUsuario, soloActivas), cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<AsignacionDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetAsignacionByIdQuery(id), cancellationToken));

    [HttpGet("activo/{idActivo:int}/historial")]
    [Authorize(Roles = Roles.Lectura)]
    [ProducesResponseType(typeof(IReadOnlyList<AsignacionHistorialDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AsignacionHistorialDto>>> GetHistorialPorActivo(
        int idActivo,
        CancellationToken cancellationToken) =>
        Ok(await _historial.HandleAsync(new GetHistorialAsignacionesPorActivoQuery(idActivo), cancellationToken));

    [HttpPost]
    [HttpPost("entrega")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Entregar(
        [FromBody] CreateAsignacionCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _entregar.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateAsignacionCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id) return IdMismatch();
        await _update.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("traslado")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Trasladar(
        [FromBody] CreateTrasladoCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _trasladar.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPost("mantenimiento")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> IniciarMantenimiento(
        [FromBody] CreateMantenimientoCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _iniciarMantenimiento.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPost("baja")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DarDeBaja(
        [FromBody] CreateBajaCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _darDeBaja.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPost("{id:int}/finalizar-mantenimiento")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> FinalizarMantenimiento(
        int id,
        [FromBody] FinalizarMantenimientoCommand command,
        CancellationToken cancellationToken)
    {
        await _finalizarMantenimiento.HandleAsync(command with { Id = id }, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/devolver")]
    [HttpPost("{id:int}/devolucion")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Devolver(
        int id,
        [FromBody] DevolverAsignacionCommand command,
        CancellationToken cancellationToken)
    {
        await _devolver.HandleAsync(command with { Id = id }, cancellationToken);
        return NoContent();
    }
}
