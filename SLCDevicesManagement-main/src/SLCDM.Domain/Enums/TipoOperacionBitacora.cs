namespace SLCDM.Domain.Enums;

/// <summary>
/// Tipo de operación que <see cref="SLCDM.Domain.Entities.Bitacora"/>
/// registra sobre `entidad_afectada` (columna `tipo_operacion` del ERD).
/// </summary>
public enum TipoOperacionBitacora
{
    Creacion = 0,
    Modificacion = 1,
    Eliminacion = 2,
}
