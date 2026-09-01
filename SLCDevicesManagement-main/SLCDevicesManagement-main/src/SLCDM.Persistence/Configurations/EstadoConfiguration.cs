using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class EstadoConfiguration : IEntityTypeConfiguration<Estado>
{
    public void Configure(EntityTypeBuilder<Estado> builder)
    {
        builder.ToTable("estado");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id_estado");

        builder.Property(e => e.Nombre)
            .HasColumnName("nombre")
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(e => e.Descripcion)
            .HasColumnName("descripcion")
            .HasColumnType("varchar(150)");

        // Estado NO hereda de BaseAuditableEntity (es catalogo simple, sin
        // habilitado/fechas) — coincide con el ERD.
        builder.HasIndex(e => e.Nombre).IsUnique();
    }
}
