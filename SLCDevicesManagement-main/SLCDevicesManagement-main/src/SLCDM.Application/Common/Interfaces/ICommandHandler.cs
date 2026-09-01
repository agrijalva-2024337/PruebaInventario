namespace SLCDM.Application.Common.Interfaces;

/// <summary>
/// Handler de comando sin valor de retorno (update / disable / delete).
/// </summary>
public interface ICommandHandler<in TCommand>
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handler de comando con valor de retorno (create devuelve el id).
/// </summary>
public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
