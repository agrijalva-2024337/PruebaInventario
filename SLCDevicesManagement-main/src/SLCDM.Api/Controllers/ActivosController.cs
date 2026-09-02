using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.Activos;
using SLCDM.Application.Features.Activos.Commands;
using SLCDM.Application.Features.Activos.Queries;
using SLCDM.Application.Features.Asignaciones;
using SLCDM.Application.Features.Asignaciones.Queries;

namespace SLCDM.Api.Controllers;

public sealed class ActivosController : ApiControllerBase
{
    private readonly IQueryHandler<GetActivosQuery, IReadOnlyList<ActivoDto>> _getAll;
    private readonly IQueryHandler<GetActivoByIdQuery, ActivoDto> _getById;
    private readonly ICommandHandler<CreateActivoCommand, int> _create;
    private readonly ICommandHandler<UpdateActivoCommand> _update;
    private readonly ICommandHandler<DeleteActivoCommand> _delete;

    public ActivosController(
        IQueryHandler<GetActivosQuery, IReadOnlyList<ActivoDto>> getAll,
        IQueryHandler<GetActivoByIdQuery, ActivoDto> getById,
        ICommandHandler<CreateActivoCommand, int> create,
        ICommandHandler<UpdateActivoCommand> update,
        ICommandHandler<DeleteActivoCommand> delete)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _delete = delete;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<ActivoDto>>> GetAll(
        [FromQuery] int? idCategoriaActivo = null,
        [FromQuery] int? idProveedor = null,
        [FromQuery] int? idUbicacion = null,
        CancellationToken cancellationToken = default) =>
        Ok(await _getAll.HandleAsync(
            new GetActivosQuery(idCategoriaActivo, idProveedor, idUbicacion),
            cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<ActivoDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetActivoByIdQuery(id), cancellationToken));

    [HttpGet("{id:int}/asignaciones")]
    [Authorize(Roles = Roles.Lectura)]
    [ProducesResponseType(typeof(IReadOnlyList<AsignacionHistorialDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AsignacionHistorialDto>>> GetHistorialAsignaciones(
        int id,
        [FromServices] IQueryHandler<GetHistorialAsignacionesPorActivoQuery, IReadOnlyList<AsignacionHistorialDto>> historial,
        CancellationToken cancellationToken) =>
        Ok(await historial.HandleAsync(new GetHistorialAsignacionesPorActivoQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Create([FromBody] CreateActivoCommand command, CancellationToken cancellationToken)
    {
        var id = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateActivoCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return IdMismatch();
        await _update.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _delete.HandleAsync(new DeleteActivoCommand(id), cancellationToken);
        return NoContent();
    }
}
