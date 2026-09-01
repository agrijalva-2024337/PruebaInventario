using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.Empresas;
using SLCDM.Application.Features.Empresas.Commands;
using SLCDM.Application.Features.Empresas.Queries;

namespace SLCDM.Api.Controllers;

public sealed class EmpresasController : ApiControllerBase
{
    private readonly IQueryHandler<GetEmpresasQuery, IReadOnlyList<EmpresaDto>> _getAll;
    private readonly IQueryHandler<GetEmpresaByIdQuery, EmpresaDto> _getById;
    private readonly ICommandHandler<CreateEmpresaCommand, int> _create;
    private readonly ICommandHandler<UpdateEmpresaCommand> _update;
    private readonly ICommandHandler<DisableEmpresaCommand> _disable;

    public EmpresasController(
        IQueryHandler<GetEmpresasQuery, IReadOnlyList<EmpresaDto>> getAll,
        IQueryHandler<GetEmpresaByIdQuery, EmpresaDto> getById,
        ICommandHandler<CreateEmpresaCommand, int> create,
        ICommandHandler<UpdateEmpresaCommand> update,
        ICommandHandler<DisableEmpresaCommand> disable)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _disable = disable;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<EmpresaDto>>> GetAll(
        [FromQuery] bool incluirInhabilitados = false,
        CancellationToken cancellationToken = default) =>
        Ok(await _getAll.HandleAsync(new GetEmpresasQuery(incluirInhabilitados), cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<EmpresaDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetEmpresaByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.AdministradorGeneral)]
    public async Task<IActionResult> Create([FromBody] CreateEmpresaCommand command, CancellationToken cancellationToken)
    {
        var id = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmpresaCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return IdMismatch();
        await _update.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/disable")]
    [Authorize(Roles = Roles.EscrituraEmpresa)]
    public async Task<IActionResult> Disable(int id, CancellationToken cancellationToken)
    {
        await _disable.HandleAsync(new DisableEmpresaCommand(id), cancellationToken);
        return NoContent();
    }
}
