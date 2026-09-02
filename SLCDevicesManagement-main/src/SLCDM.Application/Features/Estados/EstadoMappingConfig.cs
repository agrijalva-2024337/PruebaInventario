using Mapster;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Estados;

public sealed class EstadoMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Estado, EstadoDto>();

        config.NewConfig<Commands.CreateEstadoCommand, Estado>()
            .Ignore(dest => dest.Id);

        config.NewConfig<Commands.UpdateEstadoCommand, Estado>()
            .Ignore(dest => dest.Id);
    }
}
