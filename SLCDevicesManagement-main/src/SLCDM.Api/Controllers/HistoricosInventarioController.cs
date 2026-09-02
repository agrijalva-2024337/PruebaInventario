using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.DetallesActivos.Commands;
using SLCDM.Application.Features.HistoricosInventario;
using SLCDM.Application.Features.HistoricosInventario.Commands;
using SLCDM.Application.Features.HistoricosInventario.Queries;

namespace SLCDM.Api.Controllers;

public sealed class HistoricosInventarioController : ApiControllerBase
{
    private readonly IQueryHandler<GetHistoricosInventarioQuery, IReadOnlyList<HistoricoInventarioDto>> _getAll;
    private readonly IQueryHandler<GetHistoricoInventarioByIdQuery, HistoricoInventarioDto> _getById;
    private readonly IQueryHandler<GetDiferenciasInventarioQuery, DiferenciasInventarioDto> _diferencias;
    private readonly ICommandHandler<CreateHistoricoInventarioCommand, int> _create;
    private readonly ICommandHandler<CerrarHistoricoInventarioCommand> _cerrar;
    private readonly ICommandHandler<CreateDetalleActivoCommand, int> _crearDetalle;

    public HistoricosInventarioController(
        IQueryHandler<GetHistoricosInventarioQuery, IReadOnlyList<HistoricoInventarioDto>> getAll,
        IQueryHandler<GetHistoricoInventarioByIdQuery, HistoricoInventarioDto> getById,
        IQueryHandler<GetDiferenciasInventarioQuery, DiferenciasInventarioDto> diferencias,
        ICommandHandler<CreateHistoricoInventarioCommand, int> create,
        ICommandHandler<CerrarHistoricoInventarioCommand> cerrar,
        ICommandHandler<CreateDetalleActivoCommand, int> crearDetalle)
    {
        _getAll = getAll;
        _getById = getById;
        _diferencias = diferencias;
        _create = create;
        _cerrar = cerrar;
        _crearDetalle = crearDetalle;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<HistoricoInventarioDto>>> GetAll(
        [FromQuery] int? idSede = null,
        [FromQuery] int? idEmpresa = null,
        [FromQuery] bool? soloAbiertos = null,
        CancellationToken cancellationToken = default) =>
        Ok(await _getAll.HandleAsync(
            new GetHistoricosInventarioQuery(idSede, idEmpresa, soloAbiertos),
            cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<HistoricoInventarioDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetHistoricoInventarioByIdQuery(id), cancellationToken));

    [HttpGet("{id:int}/diferencias")]
    [Authorize(Roles = Roles.Lectura)]
    [ProducesResponseType(typeof(DiferenciasInventarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DiferenciasInventarioDto>> GetDiferencias(
        int id,
        [FromQuery] bool incluirAbierta = false,
        CancellationToken cancellationToken = default) =>
        Ok(await _diferencias.HandleAsync(new GetDiferenciasInventarioQuery(id, incluirAbierta), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Create(
        [FromBody] CreateHistoricoInventarioCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPost("{id:int}/detalles")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistrarDetalle(
        int id,
        [FromBody] CreateDetalleActivoCommand command,
        CancellationToken cancellationToken)
    {
        var detalleId = await _crearDetalle.HandleAsync(
            command with { IdHistoricoInventario = id },
            cancellationToken);
        return Created($"/api/detallesActivos/{detalleId}", new { id = detalleId });
    }

    [HttpPost("{id:int}/cerrar")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Cerrar(
        int id,
        [FromBody] CerrarHistoricoInventarioCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id) return IdMismatch();
        await _cerrar.HandleAsync(command, cancellationToken);
        return NoContent();
    }
}
