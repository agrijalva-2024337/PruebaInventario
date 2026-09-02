using Mapster;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Activos;

public sealed class ActivoMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Activo, ActivoDto>()
            .Ignore(dest => dest.EstadoOperativo)
            .Ignore(dest => dest.EstadoNombre)
            .Ignore(dest => dest.CodigoInterno)
            .Ignore(dest => dest.IdEmpresa)
            .Ignore(dest => dest.NombreEmpresa)
            .Ignore(dest => dest.IdSede)
            .Ignore(dest => dest.NombreSede)
            .Ignore(dest => dest.NombreUbicacion)
            .Ignore(dest => dest.NombreArea)
            .Ignore(dest => dest.IdResponsable)
            .Ignore(dest => dest.NombreResponsable);

        config.NewConfig<Commands.CreateActivoCommand, Activo>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CategoriaActivo!)
            .Ignore(dest => dest.Proveedor!)
            .Ignore(dest => dest.Ubicacion!);

        config.NewConfig<Commands.UpdateActivoCommand, Activo>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CategoriaActivo!)
            .Ignore(dest => dest.Proveedor!)
            .Ignore(dest => dest.Ubicacion!);
    }
}
