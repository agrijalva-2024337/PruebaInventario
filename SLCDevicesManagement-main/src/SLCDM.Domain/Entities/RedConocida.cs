using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SLCDM.Domain.Entities;

/// <summary>
/// Un punto de acceso Wi-Fi fisico (BSSID) conocido, mapeado a la Ubicacion
/// que cubre. Una Ubicacion puede tener varios BSSID (varios APs dando
/// cobertura a la misma sede), por eso esto es una tabla y no una columna
/// en Ubicacion.
/// </summary>
public class RedConocida : SLCDM.Domain.Common.BaseEntity
{
    [Required]
    [MaxLength(17, ErrorMessage = "El campo bssid no debe superar los 17 caracteres")]
    public string Bssid { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo id ubicacion es obligatorio")]
    public int IdUbicacion { get; set; }

    [ForeignKey("IdUbicacion")]
    public Ubicacion? Ubicacion { get; set; }
}
