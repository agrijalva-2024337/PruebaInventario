using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("area");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id_area");

        builder.Property(a => a.IdSede).HasColumnName("id_sede").IsRequired();

        builder.Property(a => a.Habilitado)
            .HasColumnName("habilitado")
            .HasDefaultValue(true);

        builder.Property(a => a.Nombre)
            .HasColumnName("nombre")
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(a => a.Descripcion)
            .HasColumnName("descripcion")
            .HasColumnType("varchar(200)");

        builder.HasOne<Sede>()
            .WithMany()
            .HasForeignKey(a => a.IdSede)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.IdSede);
    }
}
