using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class ActivoConfiguration : IEntityTypeConfiguration<Activo>
{
    public void Configure(EntityTypeBuilder<Activo> builder)
    {
        builder.ToTable("activo");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id_activo");

        builder.Property(a => a.IdCategoriaActivo).HasColumnName("id_categoria").IsRequired();
        builder.Property(a => a.IdProveedor).HasColumnName("id_proveedor").IsRequired();
        builder.Property(a => a.IdUbicacion).HasColumnName("id_ubicacion").IsRequired();

        builder.Property(a => a.Nombre)
            .HasColumnName("nombre")
            .HasColumnType("varchar(150)")
            .IsRequired();

        builder.Property(a => a.Descripcion)
            .HasColumnName("descripcion")
            .HasColumnType("varchar(300)");

        builder.Property(a => a.Marca)
            .HasColumnName("marca")
            .HasColumnType("varchar(100)");

        builder.Property(a => a.Modelo)
            .HasColumnName("modelo")
            .HasColumnType("varchar(100)");

        builder.Property(a => a.NumeroSerie)
            .HasColumnName("numero_serie")
            .HasColumnType("varchar(100)");

        builder.Property(a => a.FechaCompra)
            .HasColumnName("fecha_compra")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(a => a.CostoAdquisicion)
            .HasColumnName("costo_adquisicion")
            .HasColumnType("decimal(12,2)")
            .IsRequired();

        builder.Property(a => a.Moneda)
            .HasColumnName("moneda")
            .HasColumnType("varchar(10)");

        builder.Property(a => a.NumeroFactura)
            .HasColumnName("numero_factura")
            .HasColumnType("varchar(50)");

        builder.Property(a => a.FechaVencimientoGarantia)
            .HasColumnName("fecha_vencimiento_garantia")
            .HasColumnType("date");

        builder.Property(a => a.Observaciones)
            .HasColumnName("observaciones")
            .HasColumnType("varchar(500)");

        builder.HasOne(a => a.CategoriaActivo)
            .WithMany()
            .HasForeignKey(a => a.IdCategoriaActivo)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Proveedor)
            .WithMany()
            .HasForeignKey(a => a.IdProveedor)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Ubicacion)
            .WithMany()
            .HasForeignKey(a => a.IdUbicacion)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.NumeroSerie);
        builder.HasIndex(a => a.IdCategoriaActivo);
        builder.HasIndex(a => a.IdProveedor);
        builder.HasIndex(a => a.IdUbicacion);
    }
}
