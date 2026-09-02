using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SLCDM.Domain.Entities;

/// <summary>
/// Tabla "Proveedor" del ERD: a quien se le compra un activo o quien da
/// mantenimiento externo. Solo lleva `habilitado` — sin fechas de
/// auditoria (no estan en el diagrama).
/// </summary>
public class Proveedor : SLCDM.Domain.Common.BaseHabilitadoEntity
{
    [Required(ErrorMessage = "El campo id empresa es obligatorio")]
    public int IdEmpresa { get; set; }

    [ForeignKey("IdEmpresa")]
    public Empresa? Empresa { get; set; }

    [Required(ErrorMessage = "El campo nombre es obligatorio")]
    [MaxLength(150, ErrorMessage = "El campo nombre no debe superar los 150 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo nit es obligatorio")]
    [MaxLength(50, ErrorMessage = "El campo nit no debe superar los 50 caracteres")]
    public string Nit { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "El campo nombre contacto no debe superar los 100 caracteres")]
    public string? NombreContacto { get; set; }

    [MaxLength(30, ErrorMessage = "El campo telefono no debe superar los 30 caracteres")]
    public string? Telefono { get; set; }

    [MaxLength(150, ErrorMessage = "El campo correo no debe superar los 150 caracteres")]
    public string? Correo { get; set; }
}
