import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '@/features/auth/AuthContext';

export function RequireAuth() {
  const { isAuthenticated } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    const activoMatch = location.pathname.match(/^\/activos\/(\d+)/);
    if (activoMatch) {
      return <Navigate to={`/consulta/${activoMatch[1]}`} replace />;
    }

    if (location.pathname === '/activos/escanear') {
      return <Navigate to="/escanear" replace />;
    }

    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  return <Outlet />;
}
