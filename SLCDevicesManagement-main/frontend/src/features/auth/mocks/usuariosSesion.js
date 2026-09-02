import { RolUsuario } from '@/shared/api/contracts';

export const DEMO_PASSWORD = 'Practica2026';

export const usuariosSesion = [
  {
    id: 1,
    idEmpresa: null,
    nombres: 'María Isabel',
    apellidos: 'Barrios Soto',
    correo: 'admin.general@slc.com.gt',
    username: 'mbarrios',
    rol: RolUsuario.AdministradorGeneral,
    habilitado: true,
    fechaCreacion: '2026-01-15T08:00:00',
  },
  {
    id: 2,
    idEmpresa: 1,
    nombres: 'Carlos Eduardo',
    apellidos: 'Morales Paz',
    correo: 'admin.empresa@slc.com.gt',
    username: 'cmorales',
    rol: RolUsuario.AdministradorEmpresa,
    habilitado: true,
    fechaCreacion: '2026-02-03T09:30:00',
  },
  {
    id: 3,
    idEmpresa: 1,
    nombres: 'Ana Lucía',
    apellidos: 'Hernández Cú',
    correo: 'operador@slc.com.gt',
    username: 'ahernandez',
    rol: RolUsuario.OperadorInventario,
    habilitado: true,
    fechaCreacion: '2026-03-12T10:00:00',
  },
  {
    id: 4,
    idEmpresa: 1,
    nombres: 'José Andrés',
    apellidos: 'López Choc',
    correo: 'consulta@slc.com.gt',
    username: 'jlopez',
    rol: RolUsuario.Consulta,
    habilitado: true,
    fechaCreacion: '2026-04-01T11:15:00',
  },
];
