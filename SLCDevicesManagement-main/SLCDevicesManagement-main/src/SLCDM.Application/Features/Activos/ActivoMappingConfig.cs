using Mapster;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Activos;

public sealed class ActivoMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Activo, ActivoDto>();

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
