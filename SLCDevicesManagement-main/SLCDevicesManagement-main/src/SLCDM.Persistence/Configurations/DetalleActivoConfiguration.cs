using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class DetalleActivoConfiguration : IEntityTypeConfiguration<DetalleActivo>
{
    public void Configure(EntityTypeBuilder<DetalleActivo> builder)
    {
        builder.ToTable("detalle_activo");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id_detalle");

        builder.Property(d => d.IdActivo).HasColumnName("id_activo").IsRequired();
        builder.Property(d => d.IdHistoricoInventario).HasColumnName("id_inventario").IsRequired();

        builder.Property(d => d.Encontrado)
            .HasColumnName("encontrado");

        builder.Property(d => d.BuenEstado)
            .HasColumnName("buen_estado");

        builder.Property(d => d.Observaciones)
            .HasColumnName("observaciones")
            .HasColumnType("varchar(300)");

        builder.Property(d => d.FechaVerificacion)
            .HasColumnName("fecha_verificacion");

        builder.HasOne(d => d.Activo)
            .WithMany()
            .HasForeignKey(d => d.IdActivo)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.HistoricoInventario)
            .WithMany()
            .HasForeignKey(d => d.IdHistoricoInventario)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.IdActivo);
        builder.HasIndex(d => d.IdHistoricoInventario);
    }
}
