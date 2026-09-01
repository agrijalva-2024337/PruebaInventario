const VARIANT_CLASS = {
  success: 'border-emerald-200 bg-emerald-50 text-emerald-800',
  error: 'border-red-200 bg-red-50 text-red-800',
  info: 'border-slate-200 bg-slate-50 text-slate-700',
};

export function AlertBanner({ variant = 'success', message, onDismiss }) {
  if (!message) {
    return null;
  }

  return (
    <div
      role={variant === 'error' ? 'alert' : 'status'}
      className={`flex items-start justify-between gap-3 rounded-md border px-4 py-3 text-sm ${VARIANT_CLASS[variant] ?? VARIANT_CLASS.info}`}
    >
      <p>{message}</p>
      {onDismiss ? (
        <button
          type="button"
          className="shrink-0 text-xs font-medium uppercase tracking-wide opacity-70 hover:opacity-100"
          onClick={onDismiss}
        >
          Cerrar
        </button>
      ) : null}
    </div>
  );
}
