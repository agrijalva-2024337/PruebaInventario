using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.Proveedores;
using SLCDM.Application.Features.Proveedores.Commands;
using SLCDM.Application.Features.Proveedores.Queries;

namespace SLCDM.Api.Controllers;

public sealed class ProveedoresController : ApiControllerBase
{
    private readonly IQueryHandler<GetProveedoresQuery, IReadOnlyList<ProveedorDto>> _getAll;
    private readonly IQueryHandler<GetProveedorByIdQuery, ProveedorDto> _getById;
    private readonly ICommandHandler<CreateProveedorCommand, int> _create;
    private readonly ICommandHandler<UpdateProveedorCommand> _update;
    private readonly ICommandHandler<DisableProveedorCommand> _disable;

    public ProveedoresController(
        IQueryHandler<GetProveedoresQuery, IReadOnlyList<ProveedorDto>> getAll,
        IQueryHandler<GetProveedorByIdQuery, ProveedorDto> getById,
        ICommandHandler<CreateProveedorCommand, int> create,
        ICommandHandler<UpdateProveedorCommand> update,
        ICommandHandler<DisableProveedorCommand> disable)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _disable = disable;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<ProveedorDto>>> GetAll(
        [FromQuery] bool incluirInhabilitados = false,
        [FromQuery] int? idEmpresa = null,
        CancellationToken cancellationToken = default) =>
        Ok(await _getAll.HandleAsync(new GetProveedoresQuery(incluirInhabilitados, idEmpresa), cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<ProveedorDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetProveedorByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<IActionResult> Create([FromBody] CreateProveedorCommand command, CancellationToken cancellationToken)
    {
        var id = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProveedorCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return IdMismatch();
        await _update.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/disable")]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<IActionResult> Disable(int id, CancellationToken cancellationToken)
    {
        await _disable.HandleAsync(new DisableProveedorCommand(id), cancellationToken);
        return NoContent();
    }
}
