namespace SLCDM.Application.Features.CategoriasActivo;

public sealed record CategoriaActivoDto(
    int Id,
    bool Habilitado,
    string Nombre,
    string? Descripcion);
