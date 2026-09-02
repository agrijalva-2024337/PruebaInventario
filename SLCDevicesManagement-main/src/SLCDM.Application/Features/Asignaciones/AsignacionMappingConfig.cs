using Mapster;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Asignaciones;

public sealed class AsignacionMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Asignacion, AsignacionDto>()
            .Map(dest => dest.TieneFirmaEntrega, src => src.FirmaEntrega != null && src.FirmaEntrega.Length > 0)
            .Map(dest => dest.TieneFirmaRecibe, src => src.FirmaRecibe != null && src.FirmaRecibe.Length > 0);

        config.NewConfig<Commands.CreateAsignacionCommand, Asignacion>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.IdEstado)
            .Ignore(dest => dest.Activo!)
            .Ignore(dest => dest.Usuario!)
            .Ignore(dest => dest.Responsable!)
            .Ignore(dest => dest.Ubicacion!)
            .Ignore(dest => dest.Estado!)
            .Ignore(dest => dest.TipoAsignacion!);

        config.NewConfig<Commands.UpdateAsignacionCommand, Asignacion>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Activo!)
            .Ignore(dest => dest.Usuario!)
            .Ignore(dest => dest.Responsable!)
            .Ignore(dest => dest.Ubicacion!)
            .Ignore(dest => dest.Estado!)
            .Ignore(dest => dest.TipoAsignacion!);
    }
}
