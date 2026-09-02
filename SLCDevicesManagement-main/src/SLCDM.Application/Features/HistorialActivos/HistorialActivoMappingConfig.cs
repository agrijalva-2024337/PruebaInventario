using Mapster;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.HistorialActivos;

public sealed class HistorialActivoMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<HistorialActivo, HistorialActivoDto>();

        config.NewConfig<Commands.CreateHistorialActivoCommand, HistorialActivo>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Asignacion!)
            .Ignore(dest => dest.DetalleActivo!);
    }
}
