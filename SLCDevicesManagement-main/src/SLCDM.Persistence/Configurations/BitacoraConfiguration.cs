using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class BitacoraConfiguration : IEntityTypeConfiguration<Bitacora>
{
    public void Configure(EntityTypeBuilder<Bitacora> builder)
    {
        builder.ToTable("bitacora");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("id_bitacora");

        builder.Property(b => b.IdUsuario).HasColumnName("id_usuario").IsRequired();

        builder.Property(b => b.FechaHora).HasColumnName("fecha_hora").IsRequired();

        builder.Property(b => b.TipoOperacion)
            .HasColumnName("tipo_operacion")
            .HasConversion<string>()
            .HasColumnType("varchar(30)")
            .IsRequired();

        builder.Property(b => b.EntidadAfectada)
            .HasColumnName("entidad_afectada")
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(b => b.Descripcion)
            .HasColumnName("descripcion")
            .HasColumnType("varchar(300)");

        builder.Property(b => b.InformacionAnterior)
            .HasColumnName("informacion_anterior")
            .HasColumnType("nvarchar(max)");

        builder.Property(b => b.InformacionNueva)
            .HasColumnName("informacion_nueva")
            .HasColumnType("nvarchar(max)");
            
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(b => b.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.IdUsuario);
        builder.HasIndex(b => b.FechaHora);
    }
}
