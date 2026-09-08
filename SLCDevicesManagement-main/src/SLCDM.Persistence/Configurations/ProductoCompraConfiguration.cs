using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class ProductoCompraConfiguration : IEntityTypeConfiguration<ProductoCompra>
{
    public void Configure(EntityTypeBuilder<ProductoCompra> builder)
    {
        builder.ToTable("producto_compra");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id_producto_compra");

        builder.Property(p => p.IdProveedor).HasColumnName("id_proveedor").IsRequired();
        builder.Property(p => p.IdCategoriaActivo).HasColumnName("id_categoria");

        builder.Property(p => p.Habilitado)
            .HasColumnName("habilitado")
            .HasDefaultValue(true);

        builder.Property(p => p.Nombre)
            .HasColumnName("nombre")
            .HasColumnType("varchar(150)")
            .IsRequired();

        builder.Property(p => p.Marca)
            .HasColumnName("marca")
            .HasColumnType("varchar(100)");

        builder.Property(p => p.Modelo)
            .HasColumnName("modelo")
            .HasColumnType("varchar(100)");

        builder.Property(p => p.Descripcion)
            .HasColumnName("descripcion")
            .HasColumnType("varchar(500)");

        builder.Property(p => p.CostoUnitario)
            .HasColumnName("costo_unitario")
            .HasColumnType("decimal(12,2)")
            .IsRequired();

        builder.Property(p => p.Moneda)
            .HasColumnName("moneda")
            .HasColumnType("varchar(10)");

        builder.Property(p => p.NumeroFactura)
            .HasColumnName("numero_factura")
            .HasColumnType("varchar(50)");

        builder.Property(p => p.FechaCompra)
            .HasColumnName("fecha_compra")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(p => p.FechaVencimientoGarantia)
            .HasColumnName("fecha_vencimiento_garantia")
            .HasColumnType("date")
            .IsRequired();

        builder.HasOne(p => p.Proveedor)
            .WithMany()
            .HasForeignKey(p => p.IdProveedor)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.CategoriaActivo)
            .WithMany()
            .HasForeignKey(p => p.IdCategoriaActivo)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.IdProveedor);
        builder.HasIndex(p => p.IdCategoriaActivo);
    }
}
