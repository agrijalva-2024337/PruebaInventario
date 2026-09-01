using Mapster;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Proveedores;

public sealed class ProveedorMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Proveedor, ProveedorDto>();

        config.NewConfig<Commands.CreateProveedorCommand, Proveedor>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Empresa!);

        config.NewConfig<Commands.UpdateProveedorCommand, Proveedor>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Empresa!);
    }
}
