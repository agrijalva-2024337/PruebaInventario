using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SLCDM.Domain.Entities;

public class Activo: SLCDM.Domain.Common.BaseEntity {

    [Required(ErrorMessage = "El campo categoria es obligatorio")]
    public int IdCategoriaActivo { get; set; }

    [ForeignKey("IdCategoriaActivo")]
    public CategoriaActivo? CategoriaActivo { get; set; }
    
    [Required(ErrorMessage = "El campo proveedor es obligatorio")]
    public int IdProveedor { get; set; }

    [ForeignKey("IdProveedor")]
    public Proveedor? Proveedor { get; set; }

    public int IdUbicacion { get; set; }

    [ForeignKey("IdUbicacion")]
    public Ubicacion? Ubicacion { get; set; }

    [Required(ErrorMessage = "El campo nombre es obligatorio")]
    [MaxLength(150, ErrorMessage = "El campo nombre no debe superar los 150 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(300, ErrorMessage = "El campo descripcion no debe superar los 300 caracteres")]
    public string? Descripcion { get; set; }

    [MaxLength(100, ErrorMessage = "El campo marca no debe superar los 100 caracteres")]
    public string? Marca { get; set; }

    [MaxLength(100, ErrorMessage = "El campo modelo no debe superar los 100 caracteres")]
    public string? Modelo { get; set; }

    [MaxLength(100, ErrorMessage = "El campo numero de serie no debe superar los 100 caracteres")]
    public string? NumeroSerie { get; set; }
    
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime FechaCompra { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El campo costo adquisicion debe ser mayor a 0")]
    public decimal CostoAdquisicion { get; set; }
    
    [MaxLength(10, ErrorMessage = "El campo moneda no debe superar los 10 caracteres")]
    public string? Moneda { get; set; }

    [MaxLength(50, ErrorMessage = "El campo numero factura no debe superar los 50 caracteres")]
    public string? NumeroFactura { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime FechaVencimientoGarantia { get; set; }

    [MaxLength(500, ErrorMessage = "El campo observaciones no debe superar los 500 caracteres")]
    public string? Observaciones { get; set; }
}