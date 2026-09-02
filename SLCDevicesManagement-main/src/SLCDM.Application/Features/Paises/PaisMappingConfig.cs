using Mapster;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Paises;

public sealed class PaisMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Pais, PaisDto>();

        config.NewConfig<Commands.CreatePaisCommand, Pais>()
            .Ignore(dest => dest.Id);

        config.NewConfig<Commands.UpdatePaisCommand, Pais>()
            .Ignore(dest => dest.Id);
    }
}
