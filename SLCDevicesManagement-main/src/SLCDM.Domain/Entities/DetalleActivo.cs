using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SLCDM.Domain.Entities;

public class DetalleActivo: SLCDM.Domain.Common.BaseEntity {
    [Required(ErrorMessage = "El campo activo es obligatorio")]
    public int IdActivo { get; set; }

    [ForeignKey("IdActivo")]
    public Activo? Activo { get; set; }

    [Required(ErrorMessage = "El campo historico inventario es obligatorio")]
    public int IdHistoricoInventario { get; set; }

    [ForeignKey("IdHistoricoInventario")]
    public HistoricoInventario? HistoricoInventario { get; set; }

    public bool Encontrado { get; set; } 

    public bool BuenEstado { get; set; }

    [MaxLength(300, ErrorMessage = "El campo observaciones debe tener como maximo 300 caracteres")]
    public string? Observaciones { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime FechaVerificacion { get; set; }


}