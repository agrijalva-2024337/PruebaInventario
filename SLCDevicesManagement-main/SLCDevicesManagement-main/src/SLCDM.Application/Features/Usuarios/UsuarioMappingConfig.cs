using Mapster;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Usuarios;

public sealed class UsuarioMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Usuario, UsuarioDto>();

        config.NewConfig<Commands.CreateUsuarioCommand, Usuario>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Habilitado)
            .Ignore(dest => dest.FechaCreacion)
            .Ignore(dest => dest.PasswordHash);

        config.NewConfig<Commands.UpdateUsuarioCommand, Usuario>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.FechaCreacion)
            .Ignore(dest => dest.PasswordHash);
    }
}
