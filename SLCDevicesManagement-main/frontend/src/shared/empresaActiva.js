export const EMPRESA_ACTIVA_KEY = 'dercas.empresaId';
export const EMPRESA_ACTIVA_EVENT = 'dercas-empresa-activa';

export function getEmpresaActiva() {
  try {
    return sessionStorage.getItem(EMPRESA_ACTIVA_KEY) || '';
  } catch {
    return '';
  }
}

export function setEmpresaActiva(id) {
  const value = id == null ? '' : String(id);
  try {
    if (value) {
      sessionStorage.setItem(EMPRESA_ACTIVA_KEY, value);
    } else {
      sessionStorage.removeItem(EMPRESA_ACTIVA_KEY);
    }
  } catch {
    /* ignore */
  }
  window.dispatchEvent(new CustomEvent(EMPRESA_ACTIVA_EVENT, { detail: value }));
}

export function subscribeEmpresaActiva(onChange) {
  function handler(event) {
    onChange(event.detail ?? getEmpresaActiva());
  }

  window.addEventListener(EMPRESA_ACTIVA_EVENT, handler);
  return () => window.removeEventListener(EMPRESA_ACTIVA_EVENT, handler);
}
