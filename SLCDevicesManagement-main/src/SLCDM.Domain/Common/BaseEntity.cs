using System.ComponentModel.DataAnnotations;

namespace SLCDM.Domain.Common;

/// <summary>
/// Raiz comun de toda entidad del dominio. Solo la clave primaria: sin
/// logica de infraestructura, EF Core la mapea por convencion.
/// </summary>
public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }
}
