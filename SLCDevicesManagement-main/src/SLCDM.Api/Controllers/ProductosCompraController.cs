using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.ProductosCompra;
using SLCDM.Application.Features.ProductosCompra.Commands;
using SLCDM.Application.Features.ProductosCompra.Queries;

namespace SLCDM.Api.Controllers;

public sealed class ProductosCompraController : ApiControllerBase
{
    private readonly IQueryHandler<GetProductosCompraQuery, IReadOnlyList<ProductoCompraDto>> _getAll;
    private readonly IQueryHandler<GetProductoCompraByIdQuery, ProductoCompraDto> _getById;
    private readonly ICommandHandler<CreateProductoCompraCommand, int> _create;
    private readonly ICommandHandler<UpdateProductoCompraCommand> _update;
    private readonly ICommandHandler<DisableProductoCompraCommand> _disable;

    public ProductosCompraController(
        IQueryHandler<GetProductosCompraQuery, IReadOnlyList<ProductoCompraDto>> getAll,
        IQueryHandler<GetProductoCompraByIdQuery, ProductoCompraDto> getById,
        ICommandHandler<CreateProductoCompraCommand, int> create,
        ICommandHandler<UpdateProductoCompraCommand> update,
        ICommandHandler<DisableProductoCompraCommand> disable)
    {
        _getAll = getAll;
        _getById = getById;
        _create = create;
        _update = update;
        _disable = disable;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<ProductoCompraDto>>> GetAll(
        [FromQuery] bool incluirInhabilitados = false,
        [FromQuery] int? idProveedor = null,
        CancellationToken cancellationToken = default) =>
        Ok(await _getAll.HandleAsync(new GetProductosCompraQuery(incluirInhabilitados, idProveedor), cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<ProductoCompraDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _getById.HandleAsync(new GetProductoCompraByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductoCompraCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _create.HandleAsync(command, cancellationToken);
        return CreatedId(nameof(GetById), id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateProductoCompraCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return IdMismatch();
        }

        await _update.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/disable")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Disable(int id, CancellationToken cancellationToken)
    {
        await _disable.HandleAsync(new DisableProductoCompraCommand(id), cancellationToken);
        return NoContent();
    }
}
