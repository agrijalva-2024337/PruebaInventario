using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.Usuarios;
using SLCDM.Application.Features.Usuarios.Commands;
using SLCDM.Application.Features.Usuarios.Queries;

namespace SLCDM.Api.Controllers;

public sealed class UsuariosController : ApiControllerBase
{
    private readonly IQueryHandler<GetUsuariosQuery, IReadOnlyList<UsuarioDto>> _getAll;
    private readonly IQueryHandler<GetUsuarioByIdQuery, UsuarioDto> _getById;
    private readonly ICommandHandler<CreateUsuarioCommand, CreateUsuarioResult> _create;
    private readonly ICommandHandler<UpdateUsuarioCommand> _update;
    private readonly ICommandHandler<DisableUsuarioCommand> _disable;

    public UsuariosController(
        IQueryHandler<GetUsuariosQuery, IReadOnlyList<UsuarioDto>> getAll,
        IQueryHandler<GetUsuarioByIdQuery, UsuarioDto> getById,
        ICommandHandler<CreateUsuarioCommand, CreateUsuarioResult> create,
        ICommandHandler<UpdateUsuarioCommand> update,
        ICommandHandler<DisableUsuarioCommand> disable)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _disable = disable;
    }

    [HttpGet]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> GetAll(
        [FromQuery] bool incluirInhabilitados = false,
        [FromQuery] int? idEmpresa = null,
        CancellationToken cancellationToken = default) =>
        Ok(await _getAll.HandleAsync(new GetUsuariosQuery(incluirInhabilitados, idEmpresa), cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<ActionResult<UsuarioDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetUsuarioByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    [ProducesResponseType(typeof(CreateUsuarioResult), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioCommand command, CancellationToken cancellationToken)
    {
        var result = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), result.Id, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return IdMismatch();
        await _update.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/disable")]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<IActionResult> Disable(int id, CancellationToken cancellationToken)
    {
        await _disable.HandleAsync(new DisableUsuarioCommand(id), cancellationToken);
        return NoContent();
    }
}
