import { useId } from 'react';

export function SelectField({
  label,
  name,
  value,
  onChange,
  onBlur,
  error,
  required = false,
  disabled = false,
  options = [],
  placeholder = 'Seleccionar...',
}) {
  const id = useId();

  function handleChange(event) {
    const raw = event.target.value;
    const match = options.find((option) => String(option.value) === raw);
    const nextValue = match ? match.value : raw;

    onChange({ name, value: nextValue });
  }

  return (
    <div>
      <label htmlFor={id} className="block text-sm font-medium text-slate-700">
        {label}
        {required ? <span className="text-red-600"> *</span> : null}
      </label>
      <select
        id={id}
        name={name}
        value={value === null || value === undefined ? '' : String(value)}
        onChange={handleChange}
        onBlur={onBlur}
        disabled={disabled}
        className="mt-1 w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm disabled:bg-slate-50"
        aria-invalid={Boolean(error)}
      >
        <option value="">{placeholder}</option>
        {options.map((option) => (
          <option key={String(option.value)} value={String(option.value)}>
            {option.label}
          </option>
        ))}
      </select>
      {error ? <p className="mt-1 text-xs text-red-600">{error}</p> : null}
    </div>
  );
}
