const VARIANT_CLASS = {
  primary: 'bg-slate-900 text-white hover:bg-slate-800',
  secondary: 'border border-slate-200 bg-white text-slate-800 hover:bg-slate-50',
  danger: 'bg-red-600 text-white hover:bg-red-700',
  ghost: 'text-slate-700 hover:bg-slate-100',
};

export function Button({
  variant = 'primary',
  type = 'button',
  disabled = false,
  className = '',
  children,
  ...props
}) {
  return (
    <button
      type={type}
      disabled={disabled}
      className={[
        'inline-flex items-center justify-center rounded-md px-3 py-2 text-sm font-medium transition-colors',
        'disabled:cursor-not-allowed disabled:opacity-50',
        VARIANT_CLASS[variant] ?? VARIANT_CLASS.primary,
        className,
      ].join(' ')}
      {...props}
    >
      {children}
    </button>
  );
}
