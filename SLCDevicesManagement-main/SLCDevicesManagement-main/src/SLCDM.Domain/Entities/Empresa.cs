using System.ComponentModel.DataAnnotations;

namespace SLCDM.Domain.Entities;

/// <summary>
/// Tabla `Empresa` del ERD DERCAS. Solo lleva `habilitado` — sin fechas de
/// auditoria (no estan en el diagrama).
/// </summary>
public class Empresa : SLCDM.Domain.Common.BaseHabilitadoEntity
{
    [Required(ErrorMessage = "El campo nombre es obligatorio")]
    [MaxLength(150, ErrorMessage = "El campo nombre no debe superar los 150 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo nit codigo es obligatorio")]
    [MaxLength(50, ErrorMessage = "El campo nit codigo no debe superar los 50 caracteres")]
    public string NitCodigo { get; set; } = string.Empty;

    [MaxLength(150, ErrorMessage = "El campo direccion no debe superar los 150 caracteres")]
    public string? Direccion { get; set; }

    [MaxLength(30, ErrorMessage = "El campo telefono no debe superar los 30 caracteres")]
    public string? Telefono { get; set; }
}
