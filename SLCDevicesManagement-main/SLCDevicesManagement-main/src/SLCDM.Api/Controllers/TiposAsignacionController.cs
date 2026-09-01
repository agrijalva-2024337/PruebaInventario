using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.TiposAsignacion;
using SLCDM.Application.Features.TiposAsignacion.Commands;
using SLCDM.Application.Features.TiposAsignacion.Queries;

namespace SLCDM.Api.Controllers;

public sealed class TiposAsignacionController : ApiControllerBase
{
    private readonly IQueryHandler<GetTiposAsignacionQuery, IReadOnlyList<TipoAsignacionDto>> _getAll;
    private readonly IQueryHandler<GetTipoAsignacionByIdQuery, TipoAsignacionDto> _getById;
    private readonly ICommandHandler<CreateTipoAsignacionCommand, int> _create;
    private readonly ICommandHandler<UpdateTipoAsignacionCommand> _update;
    private readonly ICommandHandler<DeleteTipoAsignacionCommand> _delete;

    public TiposAsignacionController(
        IQueryHandler<GetTiposAsignacionQuery, IReadOnlyList<TipoAsignacionDto>> getAll,
        IQueryHandler<GetTipoAsignacionByIdQuery, TipoAsignacionDto> getById,
        ICommandHandler<CreateTipoAsignacionCommand, int> create,
        ICommandHandler<UpdateTipoAsignacionCommand> update,
        ICommandHandler<DeleteTipoAsignacionCommand> delete)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _delete = delete;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<TipoAsignacionDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _getAll.HandleAsync(new GetTiposAsignacionQuery(), cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<TipoAsignacionDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetTipoAsignacionByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.AdministradorGeneral)]
    public async Task<IActionResult> Create([FromBody] CreateTipoAsignacionCommand command, CancellationToken cancellationToken)
    {
        var id = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdministradorGeneral)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTipoAsignacionCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return IdMismatch();
        await _update.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdministradorGeneral)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _delete.HandleAsync(new DeleteTipoAsignacionCommand(id), cancellationToken);
        return NoContent();
    }
}
