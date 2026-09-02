using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id_usuario");

        builder.Property(u => u.IdEmpresa).HasColumnName("id_empresa");

        builder.Property(u => u.Habilitado)
            .HasColumnName("habilitado")
            .HasDefaultValue(true);

        builder.Property(u => u.Nombres)
            .HasColumnName("nombres")
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(u => u.Apellidos)
            .HasColumnName("apellidos")
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(u => u.Correo)
            .HasColumnName("correo")
            .HasColumnType("varchar(150)")
            .IsRequired();

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder.Property(u => u.Rol)
            .HasColumnName("rol")
            .HasConversion<string>()
            .HasColumnType("varchar(50)")
            .IsRequired();

        // Unica entidad con fecha_creacion PERO SIN fecha_modificacion —
        // se declara directo en Usuario.cs, no viene de una clase comun.
        builder.Property(u => u.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .HasDefaultValueSql("GETDATE()");

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(u => u.IdEmpresa)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Correo).IsUnique();
        builder.HasIndex(u => u.IdEmpresa);
    }
}
