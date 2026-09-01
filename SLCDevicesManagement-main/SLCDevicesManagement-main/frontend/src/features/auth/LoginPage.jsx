import { useState } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '@/features/auth/AuthContext';
import { Button } from '@/shared/components/Button';
import { TextField } from '@/shared/components/TextField';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';

export function LoginPage() {
  const { login, isAuthenticated, isLoading } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [emailOrUsername, setEmailOrUsername] = useState('');
  const [password, setPassword] = useState('');
  const [errorMessage, setErrorMessage] = useState(null);

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setErrorMessage(null);

    try {
      await login({ emailOrUsername, password });
      const next = location.state?.from?.pathname ?? '/';
      navigate(next, { replace: true });
    } catch (error) {
      setErrorMessage(getErrorMessage(error) || 'Correo o contraseña incorrectos.');
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-50 px-4">
      <div className="w-full max-w-md rounded-lg border border-slate-200 bg-white p-8 shadow-sm">
        <p className="text-xs font-semibold uppercase tracking-widest text-slate-400">DERCAS</p>
        <h1 className="mt-2 text-2xl font-semibold text-slate-900">Iniciar sesión</h1>
        <p className="mt-2 text-sm text-slate-500">
          Administrador general:{' '}
          <span className="font-mono">admin@localhost</span>
          {' / '}
          <span className="font-mono">CambiarYa_Dercas1!</span>
        </p>
        <p className="mt-1 text-sm text-slate-500">
          Administrador de empresa (SLCTrade):{' '}
          <span className="font-mono">admin.empresa@slctrade.com</span>
          {' / '}
          <span className="font-mono">Empresa_Dercas1!</span>
        </p>
        <p className="mt-1 text-sm text-slate-500">
          Operador de inventario (SLCTrade):{' '}
          <span className="font-mono">operador@slctrade.com</span>
          {' / '}
          <span className="font-mono">Operador_Dercas1!</span>
        </p>

        <form className="mt-6 space-y-4" onSubmit={handleSubmit}>
          <TextField
            label="Correo o usuario"
            name="emailOrUsername"
            value={emailOrUsername}
            onChange={(event) => setEmailOrUsername(event.target.value)}
            required
            autoComplete="username"
          />
          <TextField
            label="Contraseña"
            name="password"
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
            autoComplete="current-password"
          />

          {errorMessage ? <p className="text-sm text-red-600">{errorMessage}</p> : null}

          <Button type="submit" className="w-full" disabled={isLoading}>
            {isLoading ? 'Entrando...' : 'Entrar'}
          </Button>
        </form>
      </div>
    </div>
  );
}
