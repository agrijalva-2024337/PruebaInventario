import { createContext, useCallback, useContext, useMemo, useState } from 'react';
import * as authService from '@/features/auth/authService';
import { getSessionUser } from '@/shared/services/tokenStorage';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [usuario, setUsuario] = useState(() => getSessionUser());
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const login = useCallback(async (credentials) => {
    setIsLoading(true);
    setError(null);

    try {
      const session = await authService.login(credentials);
      setUsuario(session.usuario);
      return session;
    } catch (err) {
      setError(err);
      setUsuario(null);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const logout = useCallback(() => {
    authService.logout();
    setUsuario(null);
    setError(null);
  }, []);

  const value = useMemo(
    () => ({
      usuario,
      isAuthenticated: Boolean(usuario),
      isLoading,
      error,
      login,
      logout,
    }),
    [usuario, isLoading, error, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth debe usarse dentro de AuthProvider.');
  }

  return context;
}
