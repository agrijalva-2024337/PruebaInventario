using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("proveedor");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id_proveedor");

        builder.Property(p => p.IdEmpresa).HasColumnName("id_empresa").IsRequired();

        builder.Property(p => p.Habilitado)
            .HasColumnName("habilitado")
            .HasDefaultValue(true);

        builder.Property(p => p.Nombre)
            .HasColumnName("nombre")
            .HasColumnType("varchar(150)")
            .IsRequired();

        builder.Property(p => p.Nit)
            .HasColumnName("nit")
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(p => p.NombreContacto)
            .HasColumnName("nombre_contacto")
            .HasColumnType("varchar(100)");

        builder.Property(p => p.Telefono)
            .HasColumnName("telefono")
            .HasColumnType("varchar(30)");

        builder.Property(p => p.Correo)
            .HasColumnName("correo")
            .HasColumnType("varchar(150)");

        builder.HasOne(p => p.Empresa)
            .WithMany()
            .HasForeignKey(p => p.IdEmpresa)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.IdEmpresa);
        builder.HasIndex(p => p.Nit);
    }
}
