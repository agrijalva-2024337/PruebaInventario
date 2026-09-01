import { useEffect, useId, useState } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '@/features/auth/AuthContext';
import { isAdministradorGeneral, canWriteEmpresa, rolLabel } from '@/features/auth/permissions';
import * as empresaService from '@/features/organizacion/empresas/empresaService';
import { NAV_SECTIONS } from '@/shared/layout/navConfig';

function linkClassName({ isActive }) {
  return [
    'block rounded-md px-3 py-1.5 text-sm',
    isActive
      ? 'bg-slate-800 font-medium text-white'
      : 'text-slate-300 hover:bg-slate-800 hover:text-white',
  ].join(' ');
}

export function AppLayout() {
  const { usuario, logout } = useAuth();
  const navigate = useNavigate();
  const empresaSelectId = useId();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [empresas, setEmpresas] = useState([]);
  const [empresaId, setEmpresaId] = useState(
    usuario?.idEmpresa != null ? String(usuario.idEmpresa) : '',
  );
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const perfil = rolLabel(usuario);
  const puedeCambiarEmpresa = isAdministradorGeneral(usuario);
  const puedeAdministrarUsuarios = canWriteEmpresa(usuario);
  const navSections = NAV_SECTIONS.map((section) => ({
    ...section,
    items: section.items.filter((item) => {
      if (item.requiresAdminGeneral && !isAdministradorGeneral(usuario)) {
        return false;
      }
      if (item.requiresEscrituraEmpresa && !puedeAdministrarUsuarios) {
        return false;
      }
      return true;
    }),
  })).filter((section) => section.items.length > 0);

  useEffect(() => {
    let cancelled = false;

    async function loadEmpresas() {
      try {
        const rows = await empresaService.getAll();
        if (cancelled) {
          return;
        }

        const activas = rows.filter((item) => item.habilitado !== false);
        setEmpresas(activas);

        setEmpresaId((current) => {
          if (current && activas.some((item) => String(item.id) === current)) {
            return current;
          }

          if (usuario?.idEmpresa != null) {
            return String(usuario.idEmpresa);
          }

          return activas[0] ? String(activas[0].id) : '';
        });
      } catch {
        if (!cancelled) {
          setEmpresas([]);
        }
      }
    }

    loadEmpresas();
    return () => {
      cancelled = true;
    };
  }, [usuario?.idEmpresa]);

  useEffect(() => {
    if (!sidebarOpen) {
      return undefined;
    }

    function onKeyDown(event) {
      if (event.key === 'Escape') {
        setSidebarOpen(false);
      }
    }

    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, [sidebarOpen]);

  return (
    <div className="min-h-screen bg-slate-50 text-slate-900">
      {sidebarOpen ? (
        <button
          type="button"
          className="fixed inset-0 z-30 bg-slate-900/50 lg:hidden"
          aria-label="Cerrar menú de navegación"
          onClick={() => setSidebarOpen(false)}
        />
      ) : null}

      <aside
        className={[
          'fixed inset-y-0 left-0 z-40 flex w-64 flex-col bg-slate-900 text-slate-100 transition-transform lg:translate-x-0',
          sidebarOpen ? 'translate-x-0' : '-translate-x-full',
        ].join(' ')}
      >
        <div className="border-b border-slate-800 px-5 py-4">
          <p className="text-xs font-semibold uppercase tracking-widest text-slate-400">DERCAS</p>
          <p className="text-sm font-semibold">Inventario de activos</p>
        </div>

        <nav className="flex-1 overflow-y-auto px-3 py-4" aria-label="Navegación principal">
          {navSections.map((section) => (
            <div key={section.id} className="mb-4">
              <p className="px-3 pb-1 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
                {section.label}
              </p>
              <ul className="space-y-0.5">
                {section.items.map((item) => (
                  <li key={item.label}>
                    {item.disabled ? (
                      <span
                        className="block cursor-not-allowed rounded-md px-3 py-1.5 text-sm text-slate-500"
                        aria-disabled="true"
                        title="Disponible en un sprint siguiente"
                      >
                        {item.label}
                      </span>
                    ) : (
                      <NavLink
                        to={item.to}
                        end={item.end}
                        className={linkClassName}
                        onClick={() => setSidebarOpen(false)}
                      >
                        {item.label}
                      </NavLink>
                    )}
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </nav>
      </aside>

      <div className="flex min-h-screen flex-col lg:pl-64">
        <header className="sticky top-0 z-20 border-b border-slate-200 bg-white">
          <div className="flex items-center gap-4 px-4 py-3 sm:px-6">
            <button
              type="button"
              className="rounded-md border border-slate-200 p-2 text-slate-700 lg:hidden"
              aria-label="Abrir menú de navegación"
              onClick={() => setSidebarOpen(true)}
            >
              <span className="block h-0.5 w-4 bg-current" />
              <span className="mt-1 block h-0.5 w-4 bg-current" />
              <span className="mt-1 block h-0.5 w-4 bg-current" />
            </button>

            <div className="min-w-0 flex-1">
              <p className="truncate text-sm font-semibold sm:text-base">SLCDevicesManagement</p>
              <p className="hidden text-xs text-slate-500 sm:block">
                {empresas.find((item) => String(item.id) === String(empresaId))?.nombre
                  ?? (usuario?.idEmpresa == null && puedeCambiarEmpresa
                    ? 'Todas las empresas'
                    : 'Sin empresa asignada')}
              </p>
            </div>

            <div className="min-w-0">
              <label htmlFor={empresaSelectId} className="sr-only">
                Empresa activa
              </label>
              <select
                id={empresaSelectId}
                className="max-w-36 truncate rounded-md border border-slate-200 bg-white px-2 py-1.5 text-sm sm:max-w-64 sm:px-3 disabled:bg-slate-50"
                value={empresaId}
                disabled={!puedeCambiarEmpresa || empresas.length <= 1}
                onChange={(event) => setEmpresaId(event.target.value)}
              >
                {empresas.length === 0 ? (
                  <option value="">Sin empresa</option>
                ) : (
                  empresas.map((empresa) => (
                    <option key={empresa.id} value={empresa.id}>
                      {empresa.nombre}
                    </option>
                  ))
                )}
              </select>
            </div>

            <div className="relative">
              <button
                type="button"
                className="rounded-md border border-slate-200 px-3 py-1.5 text-sm font-medium"
                aria-expanded={userMenuOpen}
                onClick={() => setUserMenuOpen((open) => !open)}
              >
                <span className="block max-w-40 truncate">
                  {usuario?.nombre || usuario?.username || 'Usuario'}
                </span>
                {perfil ? (
                  <span className="hidden text-[11px] font-normal text-slate-500 sm:block">
                    {perfil}
                  </span>
                ) : null}
              </button>
              {userMenuOpen ? (
                <div className="absolute right-0 mt-2 w-48 rounded-md border border-slate-200 bg-white py-1 shadow-md">
                  <p className="px-3 py-2 text-xs text-slate-500">
                    {usuario?.correo || usuario?.username || 'Sesión activa'}
                  </p>
                  {perfil ? (
                    <p className="px-3 pb-2 text-xs font-medium text-slate-700">{perfil}</p>
                  ) : null}
                  <button
                    type="button"
                    className="block w-full px-3 py-2 text-left text-sm text-slate-700 hover:bg-slate-50"
                    onClick={() => {
                      setUserMenuOpen(false);
                      logout();
                      navigate('/login', { replace: true });
                    }}
                  >
                    Cerrar sesión
                  </button>
                </div>
              ) : null}
            </div>
          </div>
        </header>

        <main className="mx-auto w-full max-w-6xl flex-1 px-4 py-8 sm:px-6">
          <Outlet />
        </main>

        <footer className="border-t border-slate-200 bg-white py-4 text-center text-xs text-slate-500">
          Sistemas Logísticos y Corporativos, S.A.
        </footer>
      </div>
    </div>
  );
}
