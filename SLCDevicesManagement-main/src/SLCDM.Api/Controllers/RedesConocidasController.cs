using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.RedesConocidas;
using SLCDM.Application.Features.RedesConocidas.Commands;
using SLCDM.Application.Features.RedesConocidas.Queries;

namespace SLCDM.Api.Controllers;

public sealed class RedesConocidasController : ApiControllerBase
{
    private readonly IQueryHandler<GetRedesConocidasQuery, IReadOnlyList<RedConocidaDto>> _getAll;
    private readonly IQueryHandler<GetRedConocidaByIdQuery, RedConocidaDto> _getById;
    private readonly ICommandHandler<CreateRedConocidaCommand, int> _create;
    private readonly ICommandHandler<UpdateRedConocidaCommand> _update;
    private readonly ICommandHandler<DeleteRedConocidaCommand> _delete;

    public RedesConocidasController(
        IQueryHandler<GetRedesConocidasQuery, IReadOnlyList<RedConocidaDto>> getAll,
        IQueryHandler<GetRedConocidaByIdQuery, RedConocidaDto> getById,
        ICommandHandler<CreateRedConocidaCommand, int> create,
        ICommandHandler<UpdateRedConocidaCommand> update,
        ICommandHandler<DeleteRedConocidaCommand> delete)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _delete = delete;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<RedConocidaDto>>> GetAll(
        [FromQuery] int? idUbicacion = null,
        CancellationToken cancellationToken = default) =>
        Ok(await _getAll.HandleAsync(new GetRedesConocidasQuery(idUbicacion), cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<RedConocidaDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetRedConocidaByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Create([FromBody] CreateRedConocidaCommand command, CancellationToken cancellationToken)
    {
        var id = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRedConocidaCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return IdMismatch();
        await _update.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _delete.HandleAsync(new DeleteRedConocidaCommand(id), cancellationToken);
        return NoContent();
    }
}
