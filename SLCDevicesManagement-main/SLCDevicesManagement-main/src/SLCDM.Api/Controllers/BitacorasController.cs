using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.Bitacoras;
using SLCDM.Application.Features.Bitacoras.Commands;
using SLCDM.Application.Features.Bitacoras.Queries;

namespace SLCDM.Api.Controllers;

public sealed class BitacorasController : ApiControllerBase
{
    private readonly IQueryHandler<GetBitacorasQuery, IReadOnlyList<BitacoraDto>> _getAll;
    private readonly IQueryHandler<GetBitacoraByIdQuery, BitacoraDto> _getById;
    private readonly ICommandHandler<CreateBitacoraCommand, int> _create;

    public BitacorasController(
        IQueryHandler<GetBitacorasQuery, IReadOnlyList<BitacoraDto>> getAll,
        IQueryHandler<GetBitacoraByIdQuery, BitacoraDto> getById,
        ICommandHandler<CreateBitacoraCommand, int> create)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
    }

    [HttpGet]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<ActionResult<IReadOnlyList<BitacoraDto>>> GetAll(
        [FromQuery] int? idUsuario = null,
        [FromQuery] string? entidadAfectada = null,
        CancellationToken cancellationToken = default) =>
        Ok(await _getAll.HandleAsync(new GetBitacorasQuery(idUsuario, entidadAfectada), cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<ActionResult<BitacoraDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetBitacoraByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<IActionResult> Create([FromBody] CreateBitacoraCommand command, CancellationToken cancellationToken)
    {
        var id = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }
}
