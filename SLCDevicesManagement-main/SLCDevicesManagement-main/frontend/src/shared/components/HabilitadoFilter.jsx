export function HabilitadoFilter({ value, onChange }) {
  const options = [
    { id: 'activos', label: 'Activos' },
    { id: 'inactivos', label: 'Inactivos' },
    { id: 'todos', label: 'Todos' },
  ];

  return (
    <div className="inline-flex rounded-md border border-slate-200 bg-white p-1" role="group">
      {options.map((option) => {
        const isActive = value === option.id;

        return (
          <button
            key={option.id}
            type="button"
            className={[
              'rounded px-3 py-1 text-sm',
              isActive ? 'bg-slate-900 text-white' : 'text-slate-600 hover:bg-slate-50',
            ].join(' ')}
            aria-pressed={isActive}
            onClick={() => onChange(option.id)}
          >
            {option.label}
          </button>
        );
      })}
    </div>
  );
}
