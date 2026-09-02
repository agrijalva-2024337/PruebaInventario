const ACCESS_TOKEN_KEY = 'slcdm_access_token';
const SESSION_USER_KEY = 'slcdm_session_user';

export function getAccessToken() {
  return window.localStorage.getItem(ACCESS_TOKEN_KEY);
}

export function setAccessToken(token) {
  if (!token) {
    clearAccessToken();
    return;
  }

  window.localStorage.setItem(ACCESS_TOKEN_KEY, token);
}

export function getSessionUser() {
  const raw = window.localStorage.getItem(SESSION_USER_KEY);

  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw);
  } catch {
    clearSessionUser();
    return null;
  }
}

export function setSessionUser(user) {
  if (!user) {
    clearSessionUser();
    return;
  }

  window.localStorage.setItem(SESSION_USER_KEY, JSON.stringify(user));
}

export function clearSessionUser() {
  window.localStorage.removeItem(SESSION_USER_KEY);
}

export function clearAccessToken() {
  window.localStorage.removeItem(ACCESS_TOKEN_KEY);
  clearSessionUser();
}
