using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class ResponsableConfiguration : IEntityTypeConfiguration<Responsable>
{
    public void Configure(EntityTypeBuilder<Responsable> builder)
    {
        builder.ToTable("responsable");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id_responsable");

        builder.Property(r => r.IdArea).HasColumnName("id_area").IsRequired();

        builder.Property(r => r.Habilitado)
            .HasColumnName("habilitado")
            .HasDefaultValue(true);

        builder.Property(r => r.NombreCompleto)
            .HasColumnName("nombre_completo")
            .HasColumnType("varchar(150)")
            .IsRequired();

        builder.Property(r => r.Cargo)
            .HasColumnName("cargo")
            .HasColumnType("varchar(100)");

        builder.Property(r => r.Correo)
            .HasColumnName("correo")
            .HasColumnType("varchar(150)");

        builder.Property(r => r.Telefono)
            .HasColumnName("telefono")
            .HasColumnType("varchar(30)");

        builder.HasOne<Area>()
            .WithMany()
            .HasForeignKey(r => r.IdArea)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.IdArea);
    }
}
