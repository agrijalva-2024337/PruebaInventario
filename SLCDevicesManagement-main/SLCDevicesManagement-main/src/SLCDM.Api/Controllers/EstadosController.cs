using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.Estados;
using SLCDM.Application.Features.Estados.Commands;
using SLCDM.Application.Features.Estados.Queries;

namespace SLCDM.Api.Controllers;

public sealed class EstadosController : ApiControllerBase
{
    private readonly IQueryHandler<GetEstadosQuery, IReadOnlyList<EstadoDto>> _getAll;
    private readonly IQueryHandler<GetEstadoByIdQuery, EstadoDto> _getById;
    private readonly ICommandHandler<CreateEstadoCommand, int> _create;
    private readonly ICommandHandler<UpdateEstadoCommand> _update;
    private readonly ICommandHandler<DeleteEstadoCommand> _delete;

    public EstadosController(
        IQueryHandler<GetEstadosQuery, IReadOnlyList<EstadoDto>> getAll,
        IQueryHandler<GetEstadoByIdQuery, EstadoDto> getById,
        ICommandHandler<CreateEstadoCommand, int> create,
        ICommandHandler<UpdateEstadoCommand> update,
        ICommandHandler<DeleteEstadoCommand> delete)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _delete = delete;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<EstadoDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _getAll.HandleAsync(new GetEstadosQuery(), cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<EstadoDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetEstadoByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.AdministradorGeneral)]
    public async Task<IActionResult> Create([FromBody] CreateEstadoCommand command, CancellationToken cancellationToken)
    {
        var id = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdministradorGeneral)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEstadoCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return IdMismatch();
        await _update.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdministradorGeneral)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _delete.HandleAsync(new DeleteEstadoCommand(id), cancellationToken);
        return NoContent();
    }
}
