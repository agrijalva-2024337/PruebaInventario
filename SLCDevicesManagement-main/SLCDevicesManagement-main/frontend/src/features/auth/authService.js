import { DEMO_PASSWORD, usuariosSesion } from '@/features/auth/mocks/usuariosSesion';
import { apiPaths } from '@/shared/api/paths';
import { env } from '@/shared/config/env';
import httpClient from '@/shared/services/httpClient';
import {
  clearAccessToken,
  getAccessToken,
  getSessionUser,
  setAccessToken,
  setSessionUser,
} from '@/shared/services/tokenStorage';

const MOCK_DELAY_MS = 400;

function wait(ms) {
  return new Promise((resolve) => {
    setTimeout(resolve, ms);
  });
}

function mapUsuario(dto) {
  if (!dto) {
    return null;
  }

  return {
    id: dto.id,
    username: dto.username,
    nombre: dto.nombre,
    nombres: dto.nombre,
    apellidos: '',
    correo: dto.email ?? dto.correo,
    email: dto.email ?? dto.correo,
    rol: dto.rol,
    role: dto.role,
    idEmpresa: dto.idEmpresa ?? null,
    habilitado: true,
  };
}

function persistSession({ accessToken, usuario }) {
  setAccessToken(accessToken);
  setSessionUser(usuario);
  return { accessToken, usuario };
}

export async function login({ correo, clave, emailOrUsername, password }) {
  const identity = String(emailOrUsername ?? correo ?? '').trim();
  const secret = String(password ?? clave ?? '').trim();

  if (env.useApiMock) {
    await wait(MOCK_DELAY_MS);
    const usuario = usuariosSesion.find(
      (item) =>
        item.correo.toLowerCase() === identity.toLowerCase() ||
        item.username?.toLowerCase() === identity.toLowerCase(),
    );

    if (!usuario || secret !== DEMO_PASSWORD || !usuario.habilitado) {
      const error = new Error('Correo o contraseña incorrectos.');
      error.status = 401;
      throw error;
    }

    return persistSession({
      accessToken: `mock-jwt-${usuario.id}`,
      usuario,
    });
  }

  const response = await httpClient.post(apiPaths.auth.login, {
    emailOrUsername: identity,
    password: secret,
  });
  const data = response.data;

  return persistSession({
    accessToken: data.token ?? data.accessToken,
    usuario: mapUsuario(data.userDetails ?? data.user),
  });
}

export async function getMe() {
  if (env.useApiMock) {
    await wait(MOCK_DELAY_MS);
    const usuario = getSessionUser();

    if (!usuario || !getAccessToken()) {
      const error = new Error('No hay una sesión activa.');
      error.status = 401;
      throw error;
    }

    return usuario;
  }

  const response = await httpClient.get(apiPaths.auth.profile);
  const usuario = mapUsuario(response.data);
  setSessionUser(usuario);
  return usuario;
}

export function logout() {
  clearAccessToken();
}
