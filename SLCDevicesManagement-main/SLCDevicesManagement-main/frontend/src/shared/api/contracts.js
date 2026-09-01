/**
 * Contratos del cliente HTTP (FE-04).
 *
 * Application aún no expone DTOs (BE-07 / BE-08 / BE-09). Estos typedefs
 * copian el Domain en camelCase, que es lo que serializa ASP.NET por defecto.
 * No se importan en runtime: sirven de referencia para servicios y hooks.
 *
 * @typedef {0 | 1 | 2 | 3} RolUsuario
 * Consulta = 0, OperadorInventario = 1, AdministradorEmpresa = 2, AdministradorGeneral = 3
 *
 * @typedef {0 | 1 | 2} TipoOperacionBitacora
 * Creacion = 0, Modificacion = 1, Eliminacion = 2
 *
 * @typedef {object} LoginRequest
 * @property {string} [emailOrUsername]
 * @property {string} [password]
 * @property {string} [correo]
 * @property {string} [clave]
 *
 * @typedef {object} UsuarioSesion
 * @property {number} id
 * @property {number | null} idEmpresa
 * @property {string} [nombre]
 * @property {string} [nombres]
 * @property {string} [apellidos]
 * @property {string} correo
 * @property {string} username
 * @property {RolUsuario} rol
 * @property {string} [role]
 *
 * @typedef {object} LoginResponse
 * @property {string} accessToken
 * @property {UsuarioSesion} usuario
 *
 * @typedef {object} PaisDto
 * @property {number} id
 * @property {string} nombre
 * @property {string} codigoIso2
 * @property {string} codigoIso3
 * @property {string | null} codigoTelefonico
 *
 * @typedef {object} EmpresaDto
 * @property {number} id
 * @property {string} nombre
 * @property {string} nitCodigo
 * @property {string | null} direccion
 * @property {string | null} telefono
 * @property {boolean} habilitado
 *
 * @typedef {object} SedeDto
 * @property {number} id
 * @property {number} idEmpresa
 * @property {number} idPais
 * @property {string} nombre
 * @property {string | null} direccion
 * @property {string | null} ciudad
 * @property {boolean} habilitado
 *
 * @typedef {object} AreaDto
 * @property {number} id
 * @property {number} idSede
 * @property {string} nombre
 * @property {string | null} descripcion
 * @property {boolean} habilitado
 *
 * @typedef {object} UsuarioDto
 * @property {number} id
 * @property {number | null} idEmpresa
 * @property {string} nombres
 * @property {string} apellidos
 * @property {string} correo
 * @property {string} username
 * @property {RolUsuario} rol
 * @property {boolean} habilitado
 * @property {string} fechaCreacion
 *
 * @typedef {object} CreateUsuarioResult
 * @property {number} id
 * @property {string | null} [passwordGenerada]
 *
 * @typedef {object} ResponsableDto
 * @property {number} id
 * @property {number} idArea
 * @property {string} nombreCompleto
 * @property {string | null} cargo
 * @property {string | null} correo
 * @property {string | null} telefono
 * @property {boolean} habilitado
 *
 * @typedef {object} BitacoraDto
 * @property {number} id
 * @property {number} idUsuario
 * @property {string} fechaHora
 * @property {TipoOperacionBitacora} tipoOperacion
 * @property {string} entidadAfectada
 * @property {string | null} descripcion
 * @property {string | null} informacionAnterior
 * @property {string | null} informacionNueva
 *
 * @typedef {object} EstadoDto
 * @property {number} id
 * @property {string} nombre
 * @property {string | null} descripcion
 *
 * @typedef {object} TipoAsignacionDto
 * @property {number} id
 * @property {string} nombre
 * @property {string | null} descripcion
 *
 * @typedef {object} CategoriaActivoDto
 * @property {number} id
 * @property {string} nombre
 * @property {string | null} descripcion
 * @property {boolean} habilitado
 *
 * @typedef {object} ProveedorDto
 * @property {number} id
 * @property {number} idEmpresa
 * @property {string} nombre
 * @property {string} nit
 * @property {string | null} nombreContacto
 * @property {string | null} telefono
 * @property {string | null} correo
 * @property {boolean} habilitado
 *
 * @typedef {object} UbicacionDto
 * @property {number} id
 * @property {number} idSede
 * @property {string} nombre
 * @property {string | null} descripcion
 * @property {number} latitud
 * @property {number} longitud
 * @property {boolean} habilitado
 *
 * @typedef {object} ActivoDto
 * @property {number} id
 * @property {number} idCategoriaActivo
 * @property {number} idProveedor
 * @property {number | null} idUbicacion
 * @property {string} nombre
 * @property {string | null} descripcion
 * @property {string | null} marca
 * @property {string | null} modelo
 * @property {string | null} numeroSerie
 * @property {string} fechaCompra
 * @property {number} costoAdquisicion
 * @property {string | null} moneda
 * @property {string | null} numeroFactura
 * @property {string} fechaVencimientoGarantia
 * @property {string | null} observaciones
 *
 * @typedef {object} AsignacionDto
 * @property {number} id
 * @property {number} idActivo
 * @property {number} idUsuario
 * @property {number} idResponsable
 * @property {number} idEstado
 * @property {number} idTipoAsignacion
 * @property {string} fechaAsignacion
 * @property {string | null} fechaDevolucion
 * @property {boolean} activa
 * @property {string | null} observaciones
 * @property {string | null} documentoPdfUrl
 *
 * @typedef {object} HistoricoInventarioDto
 * @property {number} id
 * @property {number} idSede
 * @property {boolean} cerrado
 * @property {string | null} responsable
 * @property {string} fechaInicio
 * @property {string | null} fechaCierre
 * @property {string | null} observaciones
 *
 * @typedef {object} DetalleActivoDto
 * @property {number} id
 * @property {number} idActivo
 * @property {number} idHistoricoInventario
 * @property {boolean} encontrado
 * @property {boolean} buenEstado
 * @property {string | null} observaciones
 * @property {string} fechaVerificacion
 *
 * @typedef {object} HistorialActivoDto
 * @property {number} id
 * @property {number | null} idAsignacion
 * @property {number | null} idDetalleActivo
 * @property {string} fechaHora
 * @property {string | null} tipoOperacion
 * @property {string | null} descripcion
 * @property {string | null} informacionAnterior
 * @property {string | null} informacionNueva
 */

export const RolUsuario = {
  Consulta: 0,
  OperadorInventario: 1,
  AdministradorEmpresa: 2,
  AdministradorGeneral: 3,
};

export const TipoOperacionBitacora = {
  Creacion: 0,
  Modificacion: 1,
  Eliminacion: 2,
};

export const rolUsuarioLabel = {
  [RolUsuario.Consulta]: 'Consulta',
  [RolUsuario.OperadorInventario]: 'Operador de inventario',
  [RolUsuario.AdministradorEmpresa]: 'Administrador de empresa',
  [RolUsuario.AdministradorGeneral]: 'Administrador general',
};
