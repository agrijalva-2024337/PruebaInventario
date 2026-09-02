using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SLCDM.Domain.Entities;

public class HistoricoInventario: SLCDM.Domain.Common.BaseEntity {
    [Required(ErrorMessage = "El campo sede es obligatorio")]
    public int IdSede { get; set; }

    [ForeignKey("IdSede")]
    public Sede? Sede { get; set; }

    public bool Cerrado { get; set; } = false;

    [MaxLength(150, ErrorMessage = "El campo responsable debe tener como maximo 150 caracteres")]
    public string? Responsable { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo fecha inicio es obligatorio")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime FechaInicio { get; set; }

    public DateTime? FechaCierre { get; set; }

    [MaxLength(300, ErrorMessage = "El campo observaciones debe tener como maximo 300 caracteres")]
    public string? Observaciones { get; set; } = string.Empty;


}