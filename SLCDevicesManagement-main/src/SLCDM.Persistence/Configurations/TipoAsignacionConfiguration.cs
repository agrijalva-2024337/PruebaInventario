using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class TipoAsignacionConfiguration : IEntityTypeConfiguration<TipoAsignacion>
{
    public void Configure(EntityTypeBuilder<TipoAsignacion> builder)
    {
        builder.ToTable("tipo_asignacion");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id_tipo_asignacion");

        builder.Property(t => t.Nombre)
            .HasColumnName("nombre")
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(t => t.Descripcion)
            .HasColumnName("descripcion")
            .HasColumnType("varchar(150)");

        // Igual que Estado: catalogo simple, sin auditoria, coincide con el ERD.
        builder.HasIndex(t => t.Nombre).IsUnique();
    }
}
