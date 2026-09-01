using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.DetallesActivos;
using SLCDM.Application.Features.DetallesActivos.Commands;
using SLCDM.Application.Features.DetallesActivos.Queries;

namespace SLCDM.Api.Controllers;

public sealed class DetallesActivosController : ApiControllerBase
{
    private readonly IQueryHandler<GetDetallesActivosQuery, IReadOnlyList<DetalleActivoDto>> _getAll;
    private readonly IQueryHandler<GetDetalleActivoByIdQuery, DetalleActivoDto> _getById;
    private readonly ICommandHandler<CreateDetalleActivoCommand, int> _create;
    private readonly ICommandHandler<UpdateDetalleActivoCommand> _update;
    private readonly ICommandHandler<DeleteDetalleActivoCommand> _delete;

    public DetallesActivosController(
        IQueryHandler<GetDetallesActivosQuery, IReadOnlyList<DetalleActivoDto>> getAll,
        IQueryHandler<GetDetalleActivoByIdQuery, DetalleActivoDto> getById,
        ICommandHandler<CreateDetalleActivoCommand, int> create,
        ICommandHandler<UpdateDetalleActivoCommand> update,
        ICommandHandler<DeleteDetalleActivoCommand> delete)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _delete = delete;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<DetalleActivoDto>>> GetAll(
        [FromQuery] int? idHistoricoInventario = null,
        [FromQuery] int? idActivo = null,
        CancellationToken cancellationToken = default) =>
        Ok(await _getAll.HandleAsync(new GetDetallesActivosQuery(idHistoricoInventario, idActivo), cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<DetalleActivoDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetDetalleActivoByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Create([FromBody] CreateDetalleActivoCommand command, CancellationToken cancellationToken)
    {
        var id = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDetalleActivoCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return IdMismatch();
        await _update.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _delete.HandleAsync(new DeleteDetalleActivoCommand(id), cancellationToken);
        return NoContent();
    }
}
