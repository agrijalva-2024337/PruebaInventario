import { Route, Routes } from 'react-router-dom';
import { HomePage } from '@/app/pages/HomePage';
import { NotFoundPage } from '@/app/pages/NotFoundPage';
import { ActivoDetallePage } from '@/features/activos/ActivoDetallePage';
import { ActivosPage } from '@/features/activos/ActivosPage';
import { EscanearQrPage } from '@/features/activos/EscanearQrPage';
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
import { BitacorasPage } from '@/features/organizacion/bitacoras/BitacorasPage';
import { ReportesPage } from '@/features/reportes/ReportesPage';
import { ConsultaActivoPage } from '@/features/consulta/ConsultaActivoPage';
import { RedesConocidasPage } from '@/features/catalogos/redesConocidas/RedesConocidasPage';
import { FueraDeRangoPage } from '@/features/rastreo/FueraDeRangoPage';
import { MapaRastreoPage } from '@/features/rastreo/MapaRastreoPage';
import { AppLayout } from '@/shared/layout/AppLayout';
import { PublicLayout } from '@/shared/layout/PublicLayout';

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<PublicLayout />}>
        <Route path="/escanear" element={<EscanearQrPage publicMode />} />
        <Route path="/consulta/:id" element={<ConsultaActivoPage />} />
      </Route>
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
          <Route path="/catalogos/bitacora" element={<BitacorasPage />} />
          <Route path="/catalogos/estados" element={<EstadosPage />} />
          <Route path="/catalogos/tipos-asignacion" element={<TiposAsignacionPage />} />
          <Route path="/catalogos/redes-conocidas" element={<RedesConocidasPage />} />
          <Route path="/activos" element={<ActivosPage />} />
          <Route path="/activos/escanear" element={<EscanearQrPage />} />
          <Route path="/activos/:id" element={<ActivoDetallePage />} />
          <Route path="/operaciones/asignaciones" element={<AsignacionesPage />} />
          <Route path="/operaciones/traslados" element={<TrasladosPage />} />
          <Route path="/operaciones/mantenimientos" element={<MantenimientosPage />} />
          <Route path="/operaciones/bajas" element={<BajasPage />} />
          <Route path="/inventario" element={<InventarioPage />} />
          <Route path="/reportes" element={<ReportesPage />} />
          <Route path="/rastreo/fuera-de-rango" element={<FueraDeRangoPage />} />
          <Route path="/rastreo/mapa" element={<MapaRastreoPage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Route>
    </Routes>
  );
}

export default App;
