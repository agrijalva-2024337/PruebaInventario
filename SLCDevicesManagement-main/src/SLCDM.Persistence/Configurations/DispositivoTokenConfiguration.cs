using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class DispositivoTokenConfiguration : IEntityTypeConfiguration<DispositivoToken>
{
    public void Configure(EntityTypeBuilder<DispositivoToken> builder)
    {
        builder.ToTable("dispositivo_token");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id_dispositivo_token");

        builder.Property(d => d.IdActivo).HasColumnName("id_activo").IsRequired();

        builder.HasOne(d => d.Activo)
            .WithMany()
            .HasForeignKey(d => d.IdActivo)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(d => d.TokenHash)
            .HasColumnName("token_hash")
            .HasColumnType("varchar(200)")
            .IsRequired();

        builder.Property(d => d.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.Property(d => d.ExpiraEn).HasColumnName("expira_en");
        builder.Property(d => d.UltimoUsoEn).HasColumnName("ultimo_uso_en");

        builder.Property(d => d.Revocado)
            .HasColumnName("revocado")
            .HasDefaultValue(false);

        builder.Property(d => d.UltimaUbicacionDetectadaId).HasColumnName("ultima_ubicacion_detectada_id");

        builder.HasOne(d => d.UltimaUbicacionDetectada)
            .WithMany()
            .HasForeignKey(d => d.UltimaUbicacionDetectadaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(d => d.FueraDeRango)
            .HasColumnName("fuera_de_rango")
            .HasDefaultValue(false);

        builder.Property(d => d.UltimoBssid)
            .HasColumnName("ultimo_bssid")
            .HasColumnType("varchar(17)");

        builder.Property(d => d.UltimaLatitud)
            .HasColumnName("ultima_latitud")
            .HasColumnType("decimal(9,6)");

        builder.Property(d => d.UltimaLongitud)
            .HasColumnName("ultima_longitud")
            .HasColumnType("decimal(9,6)");

        builder.Property(d => d.OrigenCoordenada)
            .HasColumnName("origen_coordenada")
            .HasColumnType("varchar(10)");

        builder.HasIndex(d => d.TokenHash).IsUnique();
        builder.HasIndex(d => d.IdActivo).IsUnique().HasFilter("[revocado] = 0");
    }
}
