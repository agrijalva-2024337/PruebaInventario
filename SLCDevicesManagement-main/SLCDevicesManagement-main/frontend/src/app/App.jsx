import { Route, Routes } from 'react-router-dom';
import { HomePage } from '@/app/pages/HomePage';
import { NotFoundPage } from '@/app/pages/NotFoundPage';
import { ActivosPage } from '@/features/activos/ActivosPage';
import {
  AsignacionesPage,
  BajasPage,
  MantenimientosPage,
  TrasladosPage,
} from '@/features/asignaciones/MovimientosPage';
import { LoginPage } from '@/features/auth/LoginPage';
import { RequireAuth } from '@/features/auth/RequireAuth';
import { CategoriasPage } from '@/features/catalogos/categorias/CategoriasPage';
import { ProveedoresPage } from '@/features/catalogos/proveedores/ProveedoresPage';
import { UbicacionesPage } from '@/features/catalogos/ubicaciones/UbicacionesPage';
import { InventarioPage } from '@/features/inventario/InventarioPage';
import { AreasPage } from '@/features/organizacion/areas/AreasPage';
import { EmpresasPage } from '@/features/organizacion/empresas/EmpresasPage';
import { EstadosPage, TiposAsignacionPage } from '@/features/organizacion/estados/EstadosPage';
import { ResponsablesPage } from '@/features/organizacion/responsables/ResponsablesPage';
import { SedesPage } from '@/features/organizacion/sedes/SedesPage';
import { UsuariosPage } from '@/features/organizacion/usuarios/UsuariosPage';
import { ReportesPage } from '@/features/reportes/ReportesPage';
import { AppLayout } from '@/shared/layout/AppLayout';

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<RequireAuth />}>
        <Route element={<AppLayout />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/catalogos/empresas" element={<EmpresasPage />} />
          <Route path="/catalogos/sedes" element={<SedesPage />} />
          <Route path="/catalogos/areas" element={<AreasPage />} />
          <Route path="/catalogos/responsables" element={<ResponsablesPage />} />
          <Route path="/catalogos/categorias" element={<CategoriasPage />} />
          <Route path="/catalogos/proveedores" element={<ProveedoresPage />} />
          <Route path="/catalogos/ubicaciones" element={<UbicacionesPage />} />
          <Route path="/catalogos/usuarios" element={<UsuariosPage />} />
          <Route path="/catalogos/estados" element={<EstadosPage />} />
          <Route path="/catalogos/tipos-asignacion" element={<TiposAsignacionPage />} />
          <Route path="/activos" element={<ActivosPage />} />
          <Route path="/operaciones/asignaciones" element={<AsignacionesPage />} />
          <Route path="/operaciones/traslados" element={<TrasladosPage />} />
          <Route path="/operaciones/mantenimientos" element={<MantenimientosPage />} />
          <Route path="/operaciones/bajas" element={<BajasPage />} />
          <Route path="/inventario" element={<InventarioPage />} />
          <Route path="/reportes" element={<ReportesPage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Route>
    </Routes>
  );
}

export default App;
