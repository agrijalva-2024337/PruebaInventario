using System.ComponentModel.DataAnnotations;
using SLCDM.Domain.Enums;

namespace SLCDM.Domain.Entities;

/// <summary>
/// Tabla `Bitacora` del ERD DERCAS (17 entidades, grupo empresarial y organizacion — BE-02).
/// </summary>
public class Bitacora : SLCDM.Domain.Common.BaseEntity
{
    public int IdUsuario { get; set; }

    public DateTime FechaHora { get; set; }

    public TipoOperacionBitacora TipoOperacion { get; set; }

    [Required(ErrorMessage = "El campo entidad afectada es obligatorio")]
    [MaxLength(100, ErrorMessage = "El campo entidad afectada no debe superar los 100 caracteres")]
    public string EntidadAfectada { get; set; } = string.Empty;

    [MaxLength(300, ErrorMessage = "El campo descripcion no debe superar los 300 caracteres")]
    public string? Descripcion { get; set; }

    public string? InformacionAnterior { get; set; }

    public string? InformacionNueva { get; set; }
}