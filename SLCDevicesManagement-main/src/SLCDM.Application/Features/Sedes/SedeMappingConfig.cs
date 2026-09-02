using Mapster;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Sedes;

public sealed class SedeMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Sede, SedeDto>();

        config.NewConfig<Commands.CreateSedeCommand, Sede>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Habilitado);

        config.NewConfig<Commands.UpdateSedeCommand, Sede>()
            .Ignore(dest => dest.Id);
    }
}
