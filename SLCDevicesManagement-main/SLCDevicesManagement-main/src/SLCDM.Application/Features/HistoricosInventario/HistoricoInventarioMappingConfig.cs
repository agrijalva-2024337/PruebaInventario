using Mapster;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.HistoricosInventario;

public sealed class HistoricoInventarioMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<HistoricoInventario, HistoricoInventarioDto>();

        config.NewConfig<Commands.CreateHistoricoInventarioCommand, HistoricoInventario>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Sede!);
    }
}
