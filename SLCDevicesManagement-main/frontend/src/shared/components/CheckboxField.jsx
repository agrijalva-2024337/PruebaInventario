import { useId } from 'react';

export function CheckboxField({
  label,
  name,
  value,
  onChange,
  onBlur,
  error,
  required = false,
  disabled = false,
}) {
  const id = useId();

  return (
    <div>
      <div className="flex items-center gap-2">
        <input
          id={id}
          name={name}
          type="checkbox"
          checked={Boolean(value)}
          onChange={onChange}
          onBlur={onBlur}
          disabled={disabled}
          className="h-4 w-4 rounded border-slate-300"
          aria-invalid={Boolean(error)}
        />
        <label htmlFor={id} className="text-sm font-medium text-slate-700">
          {label}
          {required ? <span className="text-red-600"> *</span> : null}
        </label>
      </div>
      {error ? <p className="mt-1 text-xs text-red-600">{error}</p> : null}
    </div>
  );
}
