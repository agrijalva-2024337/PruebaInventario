using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.Responsables;
using SLCDM.Application.Features.Responsables.Commands;
using SLCDM.Application.Features.Responsables.Queries;

namespace SLCDM.Api.Controllers;

public sealed class ResponsablesController : ApiControllerBase
{
    private readonly IQueryHandler<GetResponsablesQuery, IReadOnlyList<ResponsableDto>> _getAll;
    private readonly IQueryHandler<GetResponsableByIdQuery, ResponsableDto> _getById;
    private readonly ICommandHandler<CreateResponsableCommand, int> _create;
    private readonly ICommandHandler<UpdateResponsableCommand> _update;
    private readonly ICommandHandler<DisableResponsableCommand> _disable;

    public ResponsablesController(
        IQueryHandler<GetResponsablesQuery, IReadOnlyList<ResponsableDto>> getAll,
        IQueryHandler<GetResponsableByIdQuery, ResponsableDto> getById,
        ICommandHandler<CreateResponsableCommand, int> create,
        ICommandHandler<UpdateResponsableCommand> update,
        ICommandHandler<DisableResponsableCommand> disable)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _disable = disable;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<ResponsableDto>>> GetAll(
        [FromQuery] bool incluirInhabilitados = false,
        [FromQuery] int? idArea = null,
        CancellationToken cancellationToken = default) =>
        Ok(await _getAll.HandleAsync(new GetResponsablesQuery(incluirInhabilitados, idArea), cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<ResponsableDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetResponsableByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Create([FromBody] CreateResponsableCommand command, CancellationToken cancellationToken)
    {
        var id = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateResponsableCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return IdMismatch();
        await _update.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/disable")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Disable(int id, CancellationToken cancellationToken)
    {
        await _disable.HandleAsync(new DisableResponsableCommand(id), cancellationToken);
        return NoContent();
    }
}
