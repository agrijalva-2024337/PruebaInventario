using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.Paises;
using SLCDM.Application.Features.Paises.Commands;
using SLCDM.Application.Features.Paises.Queries;

namespace SLCDM.Api.Controllers;

public sealed class PaisesController : ApiControllerBase
{
    private readonly IQueryHandler<GetPaisesQuery, IReadOnlyList<PaisDto>> _getAll;
    private readonly IQueryHandler<GetPaisByIdQuery, PaisDto> _getById;
    private readonly ICommandHandler<CreatePaisCommand, int> _create;
    private readonly ICommandHandler<UpdatePaisCommand> _update;
    private readonly ICommandHandler<DeletePaisCommand> _delete;

    public PaisesController(
        IQueryHandler<GetPaisesQuery, IReadOnlyList<PaisDto>> getAll,
        IQueryHandler<GetPaisByIdQuery, PaisDto> getById,
        ICommandHandler<CreatePaisCommand, int> create,
        ICommandHandler<UpdatePaisCommand> update,
        ICommandHandler<DeletePaisCommand> delete)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _delete = delete;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<PaisDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _getAll.HandleAsync(new GetPaisesQuery(), cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<PaisDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetPaisByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.AdministradorGeneral)]
    public async Task<IActionResult> Create([FromBody] CreatePaisCommand command, CancellationToken cancellationToken)
    {
        var id = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdministradorGeneral)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePaisCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return IdMismatch();
        await _update.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdministradorGeneral)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _delete.HandleAsync(new DeletePaisCommand(id), cancellationToken);
        return NoContent();
    }
}
