export function dataUrlToBase64(dataUrl) {
  if (!dataUrl) {
    return null;
  }

  const comma = String(dataUrl).indexOf(',');
  return comma >= 0 ? dataUrl.slice(comma + 1) : dataUrl;
}

export function buildActivoQrUrl(id) {
  return `${window.location.origin}/consulta/${id}`;
}

export function parseActivoIdFromQr(text) {
  const raw = String(text ?? '').trim();
  const fromPath = raw.match(/\/(?:consulta|activos)\/(\d+)(?:[/?#]|$)/i);
  if (fromPath) {
    return Number(fromPath[1]);
  }

  if (/^\d+$/.test(raw)) {
    return Number(raw);
  }

  return null;
}
