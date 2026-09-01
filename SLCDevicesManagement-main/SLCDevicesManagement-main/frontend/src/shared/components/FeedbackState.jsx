const STATUS_STYLES = {
  loading: 'border-slate-200 bg-slate-50 text-slate-600',
  error: 'border-red-200 bg-red-50 text-red-800',
  empty: 'border-slate-200 bg-slate-50 text-slate-600',
};

export function FeedbackState({
  status,
  loadingMessage = 'Cargando...',
  errorMessage = 'Ocurrió un error al procesar la solicitud.',
  emptyMessage = 'No hay datos para mostrar.',
  children,
}) {
  if (status === 'idle' || status === 'loading') {
    return (
      <div role="status" className={`rounded-md border px-4 py-3 text-sm ${STATUS_STYLES.loading}`}>
        {loadingMessage}
      </div>
    );
  }

  if (status === 'error') {
    return (
      <div role="alert" className={`rounded-md border px-4 py-3 text-sm ${STATUS_STYLES.error}`}>
        {errorMessage}
      </div>
    );
  }

  if (status === 'empty') {
    return (
      <div className={`rounded-md border px-4 py-3 text-sm ${STATUS_STYLES.empty}`}>
        {emptyMessage}
      </div>
    );
  }

  return children ?? null;
}
