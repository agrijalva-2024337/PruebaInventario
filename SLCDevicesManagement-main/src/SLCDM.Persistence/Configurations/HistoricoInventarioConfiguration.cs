using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class HistoricoInventarioConfiguration : IEntityTypeConfiguration<HistoricoInventario>
{
    public void Configure(EntityTypeBuilder<HistoricoInventario> builder)
    {
        builder.ToTable("historico_inventario");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasColumnName("id_inventario");

        builder.Property(h => h.IdSede).HasColumnName("id_sede").IsRequired();

        builder.Property(h => h.Cerrado)
            .HasColumnName("cerrado")
            .HasDefaultValue(false);

        builder.Property(h => h.Responsable)
            .HasColumnName("responsable")
            .HasColumnType("varchar(150)");

        builder.Property(h => h.FechaInicio)
            .HasColumnName("fecha_inicio")
            .IsRequired();

        builder.Property(h => h.FechaCierre)
            .HasColumnName("fecha_cierre");

        builder.Property(h => h.Observaciones)
            .HasColumnName("observaciones")
            .HasColumnType("varchar(300)");

        builder.HasOne(h => h.Sede)
            .WithMany()
            .HasForeignKey(h => h.IdSede)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(h => h.IdSede);
        builder.HasIndex(h => h.FechaInicio);
    }
}
