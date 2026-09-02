using System.ComponentModel.DataAnnotations;

namespace SLCDM.Domain.Entities;

/// <summary>
/// Tabla `Area` del ERD DERCAS. Solo lleva `habilitado` — sin fechas de
/// auditoria (no estan en el diagrama).
/// </summary>
public class Area : SLCDM.Domain.Common.BaseHabilitadoEntity
{
    public int IdSede { get; set; }

    [Required(ErrorMessage = "El campo nombre es obligatorio")]
    [MaxLength(100, ErrorMessage = "El campo nombre no debe superar los 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "El campo descripcion no debe superar los 200 caracteres")]
    public string? Descripcion { get; set; }
}
