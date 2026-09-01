using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Areas.Commands;

public sealed record UpdateAreaCommand(
    int Id,
    int IdSede,
    string Nombre,
    string? Descripcion,
    bool Habilitado);

public sealed class UpdateAreaCommandValidator : AbstractValidator<UpdateAreaCommand>
{
    public UpdateAreaCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Id).RequiredId("id area");

        RuleFor(x => x.IdSede)
            .RequiredId("id sede")
            .MustAsync(async (id, ct) => await db.Sedes.AnyAsync(s => s.Id == id, ct))
            .WithMessage("El campo id sede no corresponde a un registro existente.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El campo nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El campo nombre no debe superar los 100 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(200).WithMessage("El campo descripcion no debe superar los 200 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));
    }
}

public sealed class UpdateAreaCommandHandler : ICommandHandler<UpdateAreaCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<UpdateAreaCommand> _validator;

    public UpdateAreaCommandHandler(IApplicationDbContext db, IValidator<UpdateAreaCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(UpdateAreaCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Areas.FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Area", command.Id);

        command.Adapt(entity);

        await _db.SaveChangesAsync(cancellationToken);
    }
}
