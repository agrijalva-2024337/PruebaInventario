namespace SLCDM.Domain.Common;

/// <summary>
/// Reemplaza a BaseAuditableEntity. Segun el ERD real, 7 entidades
/// (Empresa, Sede, Area, Responsable, Categoria_Activo, Proveedor,
/// Ubicacion) SOLO llevan la columna `habilitado` (soft-delete) — ninguna
/// lleva ademas fecha_creacion/fecha_modificacion. Por eso esta clase
/// tiene solo esa propiedad, en vez de las tres pegadas.
///
/// `Usuario` es un caso distinto (habilitado + fecha_creacion, SIN
/// fecha_modificacion) — tambien hereda de aca, pero declara su propio
/// `FechaCreacion` directo en Usuario.cs, porque es la unica entidad con
/// esa combinacion y no vale la pena una tercera clase para un solo caso
/// (ver charla del canal: mejor poner el campo suelto que crear
/// abstraccion para un unico uso).
/// </summary>
public abstract class BaseHabilitadoEntity : BaseEntity
{
    public bool Habilitado { get; set; } = true;
}