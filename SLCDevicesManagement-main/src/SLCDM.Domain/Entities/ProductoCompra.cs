using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SLCDM.Domain.Entities;

/// <summary>
/// Producto de una compra/stock de un proveedor. Sirve de plantilla al
/// dar de alta varios activos iguales (misma factura, marca, modelo y costo).
/// </summary>
public class ProductoCompra : SLCDM.Domain.Common.BaseHabilitadoEntity
{
    [Required]
    public int IdProveedor { get; set; }

    [ForeignKey("IdProveedor")]
    public Proveedor? Proveedor { get; set; }

    public int? IdCategoriaActivo { get; set; }

    [ForeignKey("IdCategoriaActivo")]
    public CategoriaActivo? CategoriaActivo { get; set; }

    [Required(ErrorMessage = "El campo nombre es obligatorio")]
    [MaxLength(150, ErrorMessage = "El campo nombre no debe superar los 150 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "El campo marca no debe superar los 100 caracteres")]
    public string? Marca { get; set; }

    [MaxLength(100, ErrorMessage = "El campo modelo no debe superar los 100 caracteres")]
    public string? Modelo { get; set; }

    [MaxLength(500, ErrorMessage = "El campo descripcion no debe superar los 500 caracteres")]
    public string? Descripcion { get; set; }

    public decimal CostoUnitario { get; set; }

    [MaxLength(10, ErrorMessage = "El campo moneda no debe superar los 10 caracteres")]
    public string? Moneda { get; set; }

    [MaxLength(50, ErrorMessage = "El campo numero factura no debe superar los 50 caracteres")]
    public string? NumeroFactura { get; set; }

    public DateTime FechaCompra { get; set; }

    public DateTime FechaVencimientoGarantia { get; set; }
}
