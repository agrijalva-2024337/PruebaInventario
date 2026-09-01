using Mapster;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Bitacoras;

public sealed class BitacoraMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Bitacora, BitacoraDto>();

        config.NewConfig<Commands.CreateBitacoraCommand, Bitacora>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.FechaHora);
    }
}
