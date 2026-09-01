using System.ComponentModel.DataAnnotations;

namespace SLCDM.Domain.Entities;

/// <summary>
/// Tabla `Sede` del ERD DERCAS. Solo lleva `habilitado` — sin fechas de
/// auditoria (no estan en el diagrama).
/// </summary>
public class Sede : SLCDM.Domain.Common.BaseHabilitadoEntity
{
    public int IdEmpresa { get; set; }

    public int IdPais { get; set; }

    [Required(ErrorMessage = "El campo nombre es obligatorio")]
    [MaxLength(100, ErrorMessage = "El campo nombre no debe superar los 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "El campo direccion no debe superar los 100 caracteres")]
    public string? Direccion { get; set; }

    [MaxLength(100, ErrorMessage = "El campo ciudad no debe superar los 100 caracteres")]
    public string? Ciudad { get; set; }
}
