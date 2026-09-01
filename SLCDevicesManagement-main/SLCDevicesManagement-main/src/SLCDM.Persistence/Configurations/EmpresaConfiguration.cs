using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("empresa");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id_empresa");

        builder.Property(e => e.Habilitado)
            .HasColumnName("habilitado")
            .HasDefaultValue(true);

        builder.Property(e => e.Nombre)
            .HasColumnName("nombre")
            .HasColumnType("varchar(150)")
            .IsRequired();

        builder.Property(e => e.NitCodigo)
            .HasColumnName("nit_codigo")
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(e => e.Direccion)
            .HasColumnName("direccion")
            .HasColumnType("varchar(150)");

        builder.Property(e => e.Telefono)
            .HasColumnName("telefono")
            .HasColumnType("varchar(30)");

        builder.HasIndex(e => e.NitCodigo).IsUnique();
    }
}
    