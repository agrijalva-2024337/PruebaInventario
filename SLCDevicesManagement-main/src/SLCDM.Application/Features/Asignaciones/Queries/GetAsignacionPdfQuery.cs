using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Application.Features.Asignaciones;

namespace SLCDM.Application.Features.Asignaciones.Queries;

public sealed record GetAsignacionPdfQuery(int Id, string? PublicAppUrl = null);

public sealed class GetAsignacionPdfQueryValidator : AbstractValidator<GetAsignacionPdfQuery>
{
    public GetAsignacionPdfQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id asignacion");
    }
}

public sealed class GetAsignacionPdfQueryHandler : IQueryHandler<GetAsignacionPdfQuery, AsignacionPdfFileDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IAsignacionPdfService _pdf;
    private readonly IValidator<GetAsignacionPdfQuery> _validator;

    public GetAsignacionPdfQueryHandler(
        IApplicationDbContext db,
        IAsignacionPdfService pdf,
        IValidator<GetAsignacionPdfQuery> validator)
    {
        _db = db;
        _pdf = pdf;
        _validator = validator;
    }

    public async Task<AsignacionPdfFileDto> HandleAsync(
        GetAsignacionPdfQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);
        var file = await _pdf.GenerarAsync(query.Id, query.PublicAppUrl, cancellationToken);

        var tracked = await _db.Asignaciones.FirstOrDefaultAsync(a => a.Id == query.Id, cancellationToken);
        if (tracked is not null && tracked.DocumentoPdfGenerardoEn is null)
        {
            tracked.DocumentoPdfUrl ??= AsignacionDocumento.PdfRelativeUrl(tracked.Id);
            tracked.DocumentoPdfGenerardoEn = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return file;
    }
}
