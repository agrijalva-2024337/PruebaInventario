namespace SLCDM.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"No se encontro {entityName} con id '{key}'.")
    {
    }
}
