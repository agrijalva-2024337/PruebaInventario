using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.Ubicaciones;
using SLCDM.Application.Features.Ubicaciones.Commands;
using SLCDM.Application.Features.Ubicaciones.Queries;

namespace SLCDM.Api.Controllers;

public sealed class UbicacionesController : ApiControllerBase
{
    private readonly IQueryHandler<GetUbicacionesQuery, IReadOnlyList<UbicacionDto>> _getAll;
    private readonly IQueryHandler<GetUbicacionByIdQuery, UbicacionDto> _getById;
    private readonly ICommandHandler<CreateUbicacionCommand, int> _create;
    private readonly ICommandHandler<UpdateUbicacionCommand> _update;
    private readonly ICommandHandler<DisableUbicacionCommand> _disable;

    public UbicacionesController(
        IQueryHandler<GetUbicacionesQuery, IReadOnlyList<UbicacionDto>> getAll,
        IQueryHandler<GetUbicacionByIdQuery, UbicacionDto> getById,
        ICommandHandler<CreateUbicacionCommand, int> create,
        ICommandHandler<UpdateUbicacionCommand> update,
        ICommandHandler<DisableUbicacionCommand> disable)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _disable = disable;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<UbicacionDto>>> GetAll(
        [FromQuery] bool incluirInhabilitados = false,
        [FromQuery] int? idSede = null,
        CancellationToken cancellationToken = default) =>
        Ok(await _getAll.HandleAsync(new GetUbicacionesQuery(incluirInhabilitados, idSede), cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<UbicacionDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetUbicacionByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Create([FromBody] CreateUbicacionCommand command, CancellationToken cancellationToken)
    {
        var id = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUbicacionCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return IdMismatch();
        await _update.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/disable")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Disable(int id, CancellationToken cancellationToken)
    {
        await _disable.HandleAsync(new DisableUbicacionCommand(id), cancellationToken);
        return NoContent();
    }
}
