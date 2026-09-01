using Mapster;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Areas;

public sealed class AreaMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Area, AreaDto>();

        config.NewConfig<Commands.CreateAreaCommand, Area>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Habilitado);

        config.NewConfig<Commands.UpdateAreaCommand, Area>()
            .Ignore(dest => dest.Id);
    }
}
