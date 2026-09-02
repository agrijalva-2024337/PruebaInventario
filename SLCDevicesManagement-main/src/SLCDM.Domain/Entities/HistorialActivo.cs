using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SLCDM.Domain.Entities;

public class HistorialActivo : SLCDM.Domain.Common.BaseEntity
{
    public int? IdAsignacion { get; set; }
    [ForeignKey("IdAsignacion")]
    public Asignacion? Asignacion { get; set; }

    public int? IdDetalleActivo { get; set; }
    [ForeignKey("IdDetalleActivo")]
    public DetalleActivo? DetalleActivo { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime FechaHora { get; set; }

    [MaxLength(30, ErrorMessage = "El campo tipo operacion debe tener como maximo 30 caracteres")]
    public string? TipoOperacion { get; set; } = string.Empty;

    [MaxLength(300, ErrorMessage = "El campo descripcion debe tener como maximo 300 caracteres")]
    public string? Descripcion { get; set; } = string.Empty;

    // Sin [MaxLength]: el ERD las define NVARCHAR(MAX), no VARCHAR(300)
    // como el resto de campos de texto de esta entidad.
    public string? InformacionAnterior { get; set; } = string.Empty;

    public string? InformacionNueva { get; set; } = string.Empty;
}
