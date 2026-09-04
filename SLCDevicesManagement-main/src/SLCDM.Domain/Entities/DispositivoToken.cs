using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SLCDM.Domain.Entities;

/// <summary>
/// Credencial de un agente de rastreo instalado en un equipo. Un token por
/// activo. El valor crudo del token solo existe en el momento en que se
/// genera (RegistrarDispositivoCommand / AutoRegistrarDispositivoCommand);
/// aqui solo se guarda su hash.
/// </summary>
public class DispositivoToken : SLCDM.Domain.Common.BaseEntity
{
    [Required(ErrorMessage = "El campo id activo es obligatorio")]
    public int IdActivo { get; set; }

    [ForeignKey("IdActivo")]
    public Activo? Activo { get; set; }

    [Required]
    [MaxLength(200, ErrorMessage = "El campo token hash no debe superar los 200 caracteres")]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreadoEn { get; set; }

    public DateTime? ExpiraEn { get; set; }

    public DateTime? UltimoUsoEn { get; set; }

    public bool Revocado { get; set; }

    public int? UltimaUbicacionDetectadaId { get; set; }

    [ForeignKey("UltimaUbicacionDetectadaId")]
    public Ubicacion? UltimaUbicacionDetectada { get; set; }

    public bool FueraDeRango { get; set; }

    [MaxLength(17)]
    public string? UltimoBssid { get; set; }

    public decimal? UltimaLatitud { get; set; }

    public decimal? UltimaLongitud { get; set; }

    [MaxLength(10)]
    public string? OrigenCoordenada { get; set; }
}
