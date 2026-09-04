using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class RedConocidaConfiguration : IEntityTypeConfiguration<RedConocida>
{
    public void Configure(EntityTypeBuilder<RedConocida> builder)
    {
        builder.ToTable("red_conocida");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id_red_conocida");

        builder.Property(r => r.Bssid)
            .HasColumnName("bssid")
            .HasColumnType("varchar(17)")
            .IsRequired();

        builder.Property(r => r.IdUbicacion).HasColumnName("id_ubicacion").IsRequired();

        builder.HasOne(r => r.Ubicacion)
            .WithMany()
            .HasForeignKey(r => r.IdUbicacion)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.Bssid).IsUnique();
    }
}
