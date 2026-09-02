using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.Areas;
using SLCDM.Application.Features.Areas.Commands;
using SLCDM.Application.Features.Areas.Queries;

namespace SLCDM.Api.Controllers;

public sealed class AreasController : ApiControllerBase
{
    private readonly IQueryHandler<GetAreasQuery, IReadOnlyList<AreaDto>> _getAll;
    private readonly IQueryHandler<GetAreaByIdQuery, AreaDto> _getById;
    private readonly ICommandHandler<CreateAreaCommand, int> _create;
    private readonly ICommandHandler<UpdateAreaCommand> _update;
    private readonly ICommandHandler<DisableAreaCommand> _disable;

    public AreasController(
        IQueryHandler<GetAreasQuery, IReadOnlyList<AreaDto>> getAll,
        IQueryHandler<GetAreaByIdQuery, AreaDto> getById,
        ICommandHandler<CreateAreaCommand, int> create,
        ICommandHandler<UpdateAreaCommand> update,
        ICommandHandler<DisableAreaCommand> disable)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _disable = disable;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<AreaDto>>> GetAll(
        [FromQuery] bool incluirInhabilitados = false,
        [FromQuery] int? idSede = null,
        CancellationToken cancellationToken = default) =>
        Ok(await _getAll.HandleAsync(new GetAreasQuery(incluirInhabilitados, idSede), cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<AreaDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetAreaByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<IActionResult> Create([FromBody] CreateAreaCommand command, CancellationToken cancellationToken)
    {
        var id = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAreaCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return IdMismatch();
        await _update.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/disable")]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<IActionResult> Disable(int id, CancellationToken cancellationToken)
    {
        await _disable.HandleAsync(new DisableAreaCommand(id), cancellationToken);
        return NoContent();
    }
}
