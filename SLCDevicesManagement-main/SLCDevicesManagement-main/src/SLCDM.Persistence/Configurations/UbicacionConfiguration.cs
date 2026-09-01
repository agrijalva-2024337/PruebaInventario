using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class UbicacionConfiguration : IEntityTypeConfiguration<Ubicacion>
{
    public void Configure(EntityTypeBuilder<Ubicacion> builder)
    {
        builder.ToTable("ubicacion");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id_ubicacion");

        builder.Property(u => u.IdSede).HasColumnName("id_sede").IsRequired();

        builder.Property(u => u.Habilitado)
            .HasColumnName("habilitado")
            .HasDefaultValue(true);

        builder.Property(u => u.Nombre)
            .HasColumnName("nombre")
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(u => u.Descripcion)
            .HasColumnName("descripcion")
            .HasColumnType("varchar(200)");

        builder.Property(u => u.Latitud)
            .HasColumnName("latitud")
            .HasColumnType("decimal(9,6)")
            .IsRequired();

        builder.Property(u => u.Longitud)
            .HasColumnName("longitud")
            .HasColumnType("decimal(9,6)")
            .IsRequired();

        builder.HasOne<Sede>()
            .WithMany()
            .HasForeignKey(u => u.IdSede)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.IdSede);
        builder.HasIndex(u => u.Nombre);
    }
}
