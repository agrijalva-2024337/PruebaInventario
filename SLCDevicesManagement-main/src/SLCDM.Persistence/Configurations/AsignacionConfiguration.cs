using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class AsignacionConfiguration : IEntityTypeConfiguration<Asignacion>
{
    public void Configure(EntityTypeBuilder<Asignacion> builder)
    {
        builder.ToTable("asignacion");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id_asignacion");

        builder.Property(a => a.IdActivo).HasColumnName("id_activo").IsRequired();
        builder.Property(a => a.IdUsuario).HasColumnName("id_usuario").IsRequired();
        builder.Property(a => a.IdResponsable).HasColumnName("id_responsable").IsRequired();
        builder.Property(a => a.IdUbicacion).HasColumnName("id_ubicacion").IsRequired();
        builder.Property(a => a.IdEstado).HasColumnName("id_estado").IsRequired();
        builder.Property(a => a.IdTipoAsignacion).HasColumnName("id_tipo_asignacion").IsRequired();

        builder.Property(a => a.FechaAsignacion)
            .HasColumnName("fecha_asignacion")
            .IsRequired();

        builder.Property(a => a.FechaDevolucion)
            .HasColumnName("fecha_devolucion");

        builder.Property(a => a.Activa)
            .HasColumnName("activa")
            .HasDefaultValue(true);

        builder.Property(a => a.Observaciones)
            .HasColumnName("observaciones")
            .HasColumnType("varchar(300)");

        builder.Property(a => a.FirmaEntrega)
            .HasColumnName("firma_entrega")
            .HasColumnType("varbinary(max)");

        builder.Property(a => a.FechaFirmaEntrega)
            .HasColumnName("fecha_firma_entrega");

        builder.Property(a => a.FirmaRecibe)
            .HasColumnName("firma_recibe")
            .HasColumnType("varbinary(max)");

        builder.Property(a => a.DocumentoPdfUrl)
            .HasColumnName("documento_pdf_url")
            .HasColumnType("varchar(300)");

        builder.Property(a => a.DocumentoPdfGenerardoEn)
            .HasColumnName("documento_pdf_generado_en");

        builder.HasOne(a => a.Activo)
            .WithMany()
            .HasForeignKey(a => a.IdActivo)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Usuario)
            .WithMany()
            .HasForeignKey(a => a.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Responsable)
            .WithMany()
            .HasForeignKey(a => a.IdResponsable)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Ubicacion)
            .WithMany()
            .HasForeignKey(a => a.IdUbicacion)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Estado)
            .WithMany()
            .HasForeignKey(a => a.IdEstado)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.TipoAsignacion)
            .WithMany()
            .HasForeignKey(a => a.IdTipoAsignacion)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.IdActivo);
        builder.HasIndex(a => a.IdUsuario);
        builder.HasIndex(a => a.IdResponsable);
        builder.HasIndex(a => a.IdUbicacion);
        builder.HasIndex(a => a.IdEstado);
        builder.HasIndex(a => a.IdTipoAsignacion);
        builder.HasIndex(a => a.Activa);
    }
}
