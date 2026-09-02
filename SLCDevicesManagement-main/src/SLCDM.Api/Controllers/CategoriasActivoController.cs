using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.CategoriasActivo;
using SLCDM.Application.Features.CategoriasActivo.Commands;
using SLCDM.Application.Features.CategoriasActivo.Queries;

namespace SLCDM.Api.Controllers;

public sealed class CategoriasActivoController : ApiControllerBase
{
    private readonly IQueryHandler<GetCategoriasActivoQuery, IReadOnlyList<CategoriaActivoDto>> _getAll;
    private readonly IQueryHandler<GetCategoriaActivoByIdQuery, CategoriaActivoDto> _getById;
    private readonly ICommandHandler<CreateCategoriaActivoCommand, int> _create;
    private readonly ICommandHandler<UpdateCategoriaActivoCommand> _update;
    private readonly ICommandHandler<DisableCategoriaActivoCommand> _disable;

    public CategoriasActivoController(
        IQueryHandler<GetCategoriasActivoQuery, IReadOnlyList<CategoriaActivoDto>> getAll,
        IQueryHandler<GetCategoriaActivoByIdQuery, CategoriaActivoDto> getById,
        ICommandHandler<CreateCategoriaActivoCommand, int> create,
        ICommandHandler<UpdateCategoriaActivoCommand> update,
        ICommandHandler<DisableCategoriaActivoCommand> disable)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _disable = disable;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<CategoriaActivoDto>>> GetAll(
        [FromQuery] bool incluirInhabilitados = false,
        CancellationToken cancellationToken = default) =>
        Ok(await _getAll.HandleAsync(new GetCategoriasActivoQuery(incluirInhabilitados), cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<CategoriaActivoDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetCategoriaActivoByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<IActionResult> Create([FromBody] CreateCategoriaActivoCommand command, CancellationToken cancellationToken)
    {
        var id = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoriaActivoCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return IdMismatch();
        await _update.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/disable")]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<IActionResult> Disable(int id, CancellationToken cancellationToken)
    {
        await _disable.HandleAsync(new DisableCategoriaActivoCommand(id), cancellationToken);
        return NoContent();
    }
}
