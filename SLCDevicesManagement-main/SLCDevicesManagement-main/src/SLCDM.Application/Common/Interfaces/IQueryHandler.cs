namespace SLCDM.Application.Common.Interfaces;

/// <summary>
/// Handler de consulta (CQRS). Sustituye a MediatR <c>IRequestHandler</c>:
/// cada query tiene un handler explicito registrado en DI.
/// </summary>
public interface IQueryHandler<in TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
