const VARIANT_CLASS = {
  primary: 'bg-slate-900 text-white',
  secondary: 'bg-slate-100 text-slate-700',
  danger: 'bg-red-100 text-red-800',
  ghost: 'bg-slate-50 text-slate-500 ring-1 ring-inset ring-slate-200',
  success: 'bg-emerald-100 text-emerald-800',
};

export function Badge({ variant = 'secondary', children, className = '' }) {
  return (
    <span
      className={[
        'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium',
        VARIANT_CLASS[variant] ?? VARIANT_CLASS.secondary,
        className,
      ].join(' ')}
    >
      {children}
    </span>
  );
}
