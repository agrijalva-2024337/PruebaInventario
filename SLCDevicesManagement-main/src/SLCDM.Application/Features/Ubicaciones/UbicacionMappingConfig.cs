using Mapster;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Ubicaciones;

public sealed class UbicacionMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Ubicacion, UbicacionDto>();

        config.NewConfig<Commands.CreateUbicacionCommand, Ubicacion>()
            .Ignore(dest => dest.Id);

        config.NewConfig<Commands.UpdateUbicacionCommand, Ubicacion>()
            .Ignore(dest => dest.Id);
    }
}
