using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.Sedes;
using SLCDM.Application.Features.Sedes.Commands;
using SLCDM.Application.Features.Sedes.Queries;

namespace SLCDM.Api.Controllers;

public sealed class SedesController : ApiControllerBase
{
    private readonly IQueryHandler<GetSedesQuery, IReadOnlyList<SedeDto>> _getAll;
    private readonly IQueryHandler<GetSedeByIdQuery, SedeDto> _getById;
    private readonly ICommandHandler<CreateSedeCommand, int> _create;
    private readonly ICommandHandler<UpdateSedeCommand> _update;
    private readonly ICommandHandler<DisableSedeCommand> _disable;

    public SedesController(
        IQueryHandler<GetSedesQuery, IReadOnlyList<SedeDto>> getAll,
        IQueryHandler<GetSedeByIdQuery, SedeDto> getById,
        ICommandHandler<CreateSedeCommand, int> create,
        ICommandHandler<UpdateSedeCommand> update,
        ICommandHandler<DisableSedeCommand> disable)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _disable = disable;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<SedeDto>>> GetAll(
        [FromQuery] bool incluirInhabilitados = false,
        [FromQuery] int? idEmpresa = null,
        CancellationToken cancellationToken = default) =>
        Ok(await _getAll.HandleAsync(new GetSedesQuery(incluirInhabilitados, idEmpresa), cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<SedeDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetSedeByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<IActionResult> Create([FromBody] CreateSedeCommand command, CancellationToken cancellationToken)
    {
        var id = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSedeCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return IdMismatch();
        await _update.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/disable")]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<IActionResult> Disable(int id, CancellationToken cancellationToken)
    {
        await _disable.HandleAsync(new DisableSedeCommand(id), cancellationToken);
        return NoContent();
    }
}
