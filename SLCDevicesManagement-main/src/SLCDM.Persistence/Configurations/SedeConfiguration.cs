using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class SedeConfiguration : IEntityTypeConfiguration<Sede>
{
    public void Configure(EntityTypeBuilder<Sede> builder)
    {
        builder.ToTable("sede");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id_sede");

        builder.Property(s => s.IdEmpresa).HasColumnName("id_empresa").IsRequired();
        builder.Property(s => s.IdPais).HasColumnName("id_pais").IsRequired();

        builder.Property(s => s.Habilitado)
            .HasColumnName("habilitado")
            .HasDefaultValue(true);

        builder.Property(s => s.Nombre)
            .HasColumnName("nombre")
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(s => s.Direccion)
            .HasColumnName("direccion")
            .HasColumnType("varchar(100)");

        builder.Property(s => s.Ciudad)
            .HasColumnName("ciudad")
            .HasColumnType("varchar(100)");

        builder.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(s => s.IdEmpresa)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Pais>()
            .WithMany()
            .HasForeignKey(s => s.IdPais)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.IdEmpresa);
        builder.HasIndex(s => s.IdPais);
    }
}
