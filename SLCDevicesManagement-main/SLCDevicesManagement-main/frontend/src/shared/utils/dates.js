export function todayInputValue() {
  return new Date().toISOString().slice(0, 10);
}

export function toDateInput(value) {
  if (!value) {
    return '';
  }

  return String(value).slice(0, 10);
}

export function toIsoDate(value) {
  if (!value) {
    return null;
  }

  return `${value}T12:00:00.000Z`;
}

export function formatDate(value) {
  if (!value) {
    return '—';
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return String(value);
  }

  return date.toLocaleDateString('es-GT');
}
