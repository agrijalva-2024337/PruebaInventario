using System.ComponentModel.DataAnnotations;
using SLCDM.Domain.Common;

namespace SLCDM.Domain.Entities;

public class Pais : BaseEntity
{
    [Required(ErrorMessage = "El campo nombre es obligatorio")]
    [MaxLength(100, ErrorMessage = "El campo nombre no debe superar los 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    [Required(ErrorMessage = "El campo codigo iso2 es obligatorio")]
    [MaxLength(2, ErrorMessage = "El campo codigo iso 2 no debe superar los 2 caracteres")]
    public string CodigoIso2 { get; set; } = string.Empty;
    [Required(ErrorMessage = "El campo codigo iso3 es obligatorio")]
    [MaxLength(3, ErrorMessage = "El campo codigo iso 3 no debe superar los 3 caracteres")]
    public string CodigoIso3 {get; set;} = string.Empty;
    [MaxLength(5, ErrorMessage = "El campo codigo telefonico no debe superar los 5 caracteres")]
    public string? CodigoTelefonico {get; set;}
}