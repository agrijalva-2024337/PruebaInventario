    using FluentValidation;
    using Mapster;
    using Microsoft.EntityFrameworkCore;
    using SLCDM.Application.Common.Interfaces;
    using SLCDM.Application.Common.Validation;
    using SLCDM.Domain.Entities;

    namespace SLCDM.Application.Features.Paises.Commands;

    public sealed record CreatePaisCommand(
        string Nombre,
        string CodigoIso2,
        string CodigoIso3,
        string? CodigoTelefonico);

    public sealed class CreatePaisCommandValidator : AbstractValidator<CreatePaisCommand>
    {
        public CreatePaisCommandValidator(IApplicationDbContext db)
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El campo nombre es obligatorio.")
                .MaximumLength(100).WithMessage("El campo nombre no debe superar los 100 caracteres.");

            RuleFor(x => x.CodigoIso2)
                .NotEmpty().WithMessage("El campo codigo iso2 es obligatorio.")
                .Length(2).WithMessage("El campo codigo iso2 debe tener 2 caracteres.")
                .MustAsync(async (iso2, ct) =>
                    !await db.Paises.AnyAsync(p => p.CodigoIso2 == iso2.ToUpper(), ct))
                .WithMessage("Ya existe un pais con el mismo codigo iso2.");

            RuleFor(x => x.CodigoIso3)
                .NotEmpty().WithMessage("El campo codigo iso3 es obligatorio.")
                .Length(3).WithMessage("El campo codigo iso3 debe tener 3 caracteres.")
                .MustAsync(async (iso3, ct) =>
                    !await db.Paises.AnyAsync(p => p.CodigoIso3 == iso3.ToUpper(), ct))
                .WithMessage("Ya existe un pais con el mismo codigo iso3.");

            RuleFor(x => x.CodigoTelefonico)
                .MaximumLength(5).WithMessage("El campo codigo telefonico no debe superar los 5 caracteres.")
                .When(x => !string.IsNullOrWhiteSpace(x.CodigoTelefonico));
        }
    }

    public sealed class CreatePaisCommandHandler : ICommandHandler<CreatePaisCommand, int>
    {
        private readonly IApplicationDbContext _db;
        private readonly IValidator<CreatePaisCommand> _validator;

        public CreatePaisCommandHandler(IApplicationDbContext db, IValidator<CreatePaisCommand> validator)
        {
            _db = db;
            _validator = validator;
        }

        public async Task<int> HandleAsync(CreatePaisCommand command, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateAndThrowAsync(command, cancellationToken);

            var entity = command.Adapt<Pais>();
            entity.CodigoIso2 = command.CodigoIso2.ToUpperInvariant();
            entity.CodigoIso3 = command.CodigoIso3.ToUpperInvariant();

            _db.Paises.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
    }
