using Mapster;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.DetallesActivos;

public sealed class DetalleActivoMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DetalleActivo, DetalleActivoDto>();

        config.NewConfig<Commands.CreateDetalleActivoCommand, DetalleActivo>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Activo!)
            .Ignore(dest => dest.HistoricoInventario!);

        config.NewConfig<Commands.UpdateDetalleActivoCommand, DetalleActivo>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Activo!)
            .Ignore(dest => dest.HistoricoInventario!);
    }
}
