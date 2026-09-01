using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class CategoriaActivoConfiguration : IEntityTypeConfiguration<CategoriaActivo>
{
    public void Configure(EntityTypeBuilder<CategoriaActivo> builder)
    {
        builder.ToTable("categoria_activo");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id_categoria");

        builder.Property(c => c.Habilitado)
            .HasColumnName("habilitado")
            .HasDefaultValue(true);

        builder.Property(c => c.Nombre)
            .HasColumnName("nombre")
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(c => c.Descripcion)
            .HasColumnName("descripcion")
            .HasColumnType("varchar(200)");

        builder.HasIndex(c => c.Nombre).IsUnique();
    }
}
