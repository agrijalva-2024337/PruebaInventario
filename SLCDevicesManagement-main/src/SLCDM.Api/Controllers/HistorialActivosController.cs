using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.HistorialActivos;
using SLCDM.Application.Features.HistorialActivos.Commands;
using SLCDM.Application.Features.HistorialActivos.Queries;

namespace SLCDM.Api.Controllers;

public sealed class HistorialActivosController : ApiControllerBase
{
    private readonly IQueryHandler<GetHistorialActivosQuery, IReadOnlyList<HistorialActivoDto>> _getAll;
    private readonly IQueryHandler<GetHistorialActivoByIdQuery, HistorialActivoDto> _getById;
    private readonly ICommandHandler<CreateHistorialActivoCommand, int> _create;

    public HistorialActivosController(
        IQueryHandler<GetHistorialActivosQuery, IReadOnlyList<HistorialActivoDto>> getAll,
        IQueryHandler<GetHistorialActivoByIdQuery, HistorialActivoDto> getById,
        ICommandHandler<CreateHistorialActivoCommand, int> create)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<HistorialActivoDto>>> GetAll(
        [FromQuery] int? idAsignacion = null,
        [FromQuery] int? idDetalleActivo = null,
        CancellationToken cancellationToken = default) =>
        Ok(await _getAll.HandleAsync(new GetHistorialActivosQuery(idAsignacion, idDetalleActivo), cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<HistorialActivoDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetHistorialActivoByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Create([FromBody] CreateHistorialActivoCommand command, CancellationToken cancellationToken)
    {
        var id = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }
}
