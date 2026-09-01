using Mapster;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Empresas;

public sealed class EmpresaMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Empresa, EmpresaDto>();

        config.NewConfig<Commands.CreateEmpresaCommand, Empresa>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Habilitado);

        config.NewConfig<Commands.UpdateEmpresaCommand, Empresa>()
            .Ignore(dest => dest.Id);
    }
}
