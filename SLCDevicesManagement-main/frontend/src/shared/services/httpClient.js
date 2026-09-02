import axios from 'axios';
import { env } from '@/shared/config/env';
import { getEmpresaActiva } from '@/shared/empresaActiva';
import { clearAccessToken, getAccessToken } from '@/shared/services/tokenStorage';

const httpClient = axios.create({
  baseURL: env.apiUrl,
  timeout: 15000,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
});

httpClient.interceptors.request.use((config) => {
  const url = String(config.url ?? '');
  const skipBearer =
    url.includes('/api/auth/login') || url.includes('/api/health') || url.includes('/api/consulta');

  if (skipBearer) {
    delete config.headers.Authorization;
    return config;
  }

  const token = getAccessToken();

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  const empresaId = getEmpresaActiva();
  if (empresaId) {
    config.headers['X-Empresa-Id'] = empresaId;
  } else {
    delete config.headers['X-Empresa-Id'];
  }

  return config;
});

httpClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status;
    const url = String(error.config?.url ?? '');
    const isPublic =
      url.includes('/api/auth/login') || url.includes('/api/health') || url.includes('/api/consulta');

    if (status === 401 && !isPublic) {
      clearAccessToken();

      if (typeof window !== 'undefined' && !window.location.pathname.startsWith('/login')) {
        window.location.assign('/login');
      }
    }

    return Promise.reject(error);
  },
);

export default httpClient;
