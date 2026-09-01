using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SLCDM.Domain.Entities;

public class Asignacion : SLCDM.Domain.Common.BaseEntity
{
    [Required(ErrorMessage = "El campo activo es obligatorio")]
    public int IdActivo { get; set; }

    [ForeignKey("IdActivo")]
    public Activo? Activo { get; set; }

    /// <summary>Usuario que entrega el activo.</summary>
    [Required(ErrorMessage = "El campo usuario es obligatorio")]
    public int IdUsuario { get; set; }

    [ForeignKey("IdUsuario")]
    public Usuario? Usuario { get; set; }

    /// <summary>Responsable que recibe el activo.</summary>
    [Required(ErrorMessage = "El campo responsable es obligatorio")]
    public int IdResponsable { get; set; }

    [ForeignKey("IdResponsable")]
    public Responsable? Responsable { get; set; }

    /// <summary>Ubicacion de uso del activo durante la asignacion.</summary>
    [Required(ErrorMessage = "El campo ubicacion es obligatorio")]
    public int IdUbicacion { get; set; }

    [ForeignKey("IdUbicacion")]
    public Ubicacion? Ubicacion { get; set; }

    [Required(ErrorMessage = "El campo estado es obligatorio")]
    public int IdEstado { get; set; }

    [ForeignKey("IdEstado")]
    public Estado? Estado { get; set; }

    [Required(ErrorMessage = "El campo tipo asignacion es obligatorio")]
    public int IdTipoAsignacion { get; set; }

    [ForeignKey("IdTipoAsignacion")]
    public TipoAsignacion? TipoAsignacion { get; set; }

    [Required(ErrorMessage = "El campo fecha asignacion es obligatorio")]
    [DataType(DataType.Date)]
    public DateTime FechaAsignacion { get; set; }

    public DateTime? FechaDevolucion { get; set; }

    public bool Activa { get; set; } = true;

    [MaxLength(300, ErrorMessage = "El campo observaciones no debe superar los 300 caracteres")]
    public string? Observaciones { get; set; } = string.Empty;

    public byte[]? FirmaEntrega { get; set; }

    public DateTime? FechaFirmaEntrega { get; set; }

    public byte[]? FirmaRecibe { get; set; }

    [MaxLength(300, ErrorMessage = "El campo documento pdf url no debe superar los 300 caracteres")]
    public string? DocumentoPdfUrl { get; set; } = string.Empty;

    public DateTime? DocumentoPdfGenerardoEn { get; set; }
}
