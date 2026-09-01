using Mapster;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Responsables;

public sealed class ResponsableMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Responsable, ResponsableDto>();

        config.NewConfig<Commands.CreateResponsableCommand, Responsable>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Habilitado);

        config.NewConfig<Commands.UpdateResponsableCommand, Responsable>()
            .Ignore(dest => dest.Id);
    }
}
