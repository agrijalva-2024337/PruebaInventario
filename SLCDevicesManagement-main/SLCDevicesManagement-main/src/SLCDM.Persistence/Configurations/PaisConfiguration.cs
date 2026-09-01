using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class PaisConfiguration : IEntityTypeConfiguration<Pais>
{
    public void Configure(EntityTypeBuilder<Pais> builder)
    {
        builder.ToTable("pais");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id_pais");

        builder.Property(p => p.Nombre)
            .HasColumnName("nombre")
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(p => p.CodigoIso2)
            .HasColumnName("codigo_iso2")
            .HasColumnType("varchar(2)")
            .IsRequired();

        builder.Property(p => p.CodigoIso3)
            .HasColumnName("codigo_iso3")
            .HasColumnType("varchar(3)")
            .IsRequired();

        builder.Property(p => p.CodigoTelefonico)
            .HasColumnName("codigo_telefonico")
            .HasColumnType("varchar(5)");

        // Sin Habilitado/FechaCreacion/FechaModificacion: Pais ahora hereda
        // de BaseEntity a secas, no lleva auditoria segun el ERD.

        builder.HasIndex(p => p.CodigoIso2).IsUnique();
        builder.HasIndex(p => p.CodigoIso3).IsUnique();
    }
}
