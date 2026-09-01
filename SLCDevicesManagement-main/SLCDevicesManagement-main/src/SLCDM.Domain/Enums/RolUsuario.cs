namespace SLCDM.Domain.Enums;

/// <summary>
/// Los 4 perfiles de usuario del documento de requerimientos (ver BE-09:
/// Administrador general, Administrador de empresa, Operador de
/// inventario, Consulta). <c>Consulta</c> queda como valor 0 a propósito
/// — es el rol de menos privilegio, así que si algún día se crea un
/// <see cref="SLCDM.Domain.Entities.Usuario"/> sin asignar `Rol`
/// explícitamente, el default de C# (0) no otorga acceso administrativo
/// por accidente.
/// </summary>
public enum RolUsuario
{
    Consulta = 0,
    OperadorInventario = 1,
    AdministradorEmpresa = 2,
    AdministradorGeneral = 3,
}
