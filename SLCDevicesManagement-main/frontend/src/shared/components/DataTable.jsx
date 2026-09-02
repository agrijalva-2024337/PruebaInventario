import { useId, useMemo, useState } from 'react';
import { FeedbackState } from '@/shared/components/FeedbackState';

function isSortable(column) {
  if (column.sortable === false || column.key === 'acciones') {
    return false;
  }

  return column.sortable !== false;
}

function matchesSearch(row, columns, query) {
  const normalized = query.trim().toLowerCase();
  if (!normalized) {
    return true;
  }

  return columns.some((column) => {
    if (column.key === 'acciones') {
      return false;
    }

    return String(row[column.key] ?? '')
      .toLowerCase()
      .includes(normalized);
  });
}

function compareRows(left, right, sortKey, direction) {
  const a = left[sortKey];
  const b = right[sortKey];

  if (a == null && b == null) {
    return 0;
  }
  if (a == null) {
    return 1;
  }
  if (b == null) {
    return -1;
  }

  if (typeof a === 'number' && typeof b === 'number') {
    return direction === 'asc' ? a - b : b - a;
  }

  if (typeof a === 'boolean' && typeof b === 'boolean') {
    return direction === 'asc' ? Number(a) - Number(b) : Number(b) - Number(a);
  }

  const result = String(a).localeCompare(String(b), 'es', { numeric: true, sensitivity: 'base' });
  return direction === 'asc' ? result : -result;
}

const ALIGN_CLASS = {
  left: 'text-left',
  center: 'text-center',
  right: 'text-right',
};

export function DataTable({
  columns,
  rows,
  keyField = 'id',
  isLoading = false,
  errorMessage = null,
  emptyMessage = 'No hay datos para mostrar.',
  onRowClick,
  rowClassName,
  searchPlaceholder = 'Buscar...',
  pageSize = 10,
}) {
  const searchId = useId();
  const [query, setQuery] = useState('');
  const [sort, setSort] = useState({ key: null, direction: 'asc' });
  const [page, setPage] = useState(1);

  const processedRows = useMemo(() => {
    const filtered = rows.filter((row) => matchesSearch(row, columns, query));

    if (!sort.key) {
      return filtered;
    }

    return [...filtered].sort((left, right) => compareRows(left, right, sort.key, sort.direction));
  }, [columns, query, rows, sort]);

  const pageCount = Math.max(1, Math.ceil(processedRows.length / pageSize));
  const currentPage = Math.min(page, pageCount);
  const pageRows = processedRows.slice((currentPage - 1) * pageSize, currentPage * pageSize);

  function handleSort(column) {
    if (!isSortable(column)) {
      return;
    }

    setSort((current) => {
      if (current.key !== column.key) {
        return { key: column.key, direction: 'asc' };
      }

      return {
        key: column.key,
        direction: current.direction === 'asc' ? 'desc' : 'asc',
      };
    });
  }

  let status = 'success';
  if (isLoading) {
    status = 'loading';
  } else if (errorMessage) {
    status = 'error';
  } else if (processedRows.length === 0) {
    status = 'empty';
  }

  return (
    <div className="space-y-4">
      <div>
        <label htmlFor={searchId} className="sr-only">
          {searchPlaceholder}
        </label>
        <input
          id={searchId}
          type="search"
          value={query}
          onChange={(event) => {
            setQuery(event.target.value);
            setPage(1);
          }}
          placeholder={searchPlaceholder}
          className="w-full max-w-sm rounded-md border border-slate-200 bg-white px-3 py-2 text-sm"
        />
      </div>

      <FeedbackState status={status} errorMessage={errorMessage} emptyMessage={emptyMessage}>
        <div className="overflow-x-auto rounded-lg border border-slate-200 bg-white">
          <table className="min-w-full divide-y divide-slate-200 text-sm">
            <thead className="bg-slate-50">
              <tr>
                {columns.map((column) => {
                  const align = ALIGN_CLASS[column.align] ?? ALIGN_CLASS.left;
                  const sortable = isSortable(column);
                  const isActive = sort.key === column.key;

                  return (
                    <th
                      key={column.key}
                      scope="col"
                      aria-sort={
                        isActive ? (sort.direction === 'asc' ? 'ascending' : 'descending') : 'none'
                      }
                      className={`px-4 py-3 font-semibold text-slate-600 ${align}`}
                    >
                      {sortable ? (
                        <button
                          type="button"
                          className="inline-flex items-center gap-1 hover:text-slate-900"
                          onClick={() => handleSort(column)}
                        >
                          {column.header}
                          {isActive ? (
                            <span aria-hidden="true">{sort.direction === 'asc' ? '↑' : '↓'}</span>
                          ) : null}
                        </button>
                      ) : (
                        column.header
                      )}
                    </th>
                  );
                })}
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {pageRows.map((row) => (
                <tr
                  key={row[keyField]}
                  className={[
                    onRowClick ? 'cursor-pointer hover:bg-slate-50' : '',
                    typeof rowClassName === 'function' ? rowClassName(row) : (rowClassName ?? ''),
                  ]
                    .filter(Boolean)
                    .join(' ')}
                  onClick={onRowClick ? () => onRowClick(row) : undefined}
                >
                  {columns.map((column) => {
                    const align = ALIGN_CLASS[column.align] ?? ALIGN_CLASS.left;
                    const content = column.render ? column.render(row) : (row[column.key] ?? '');

                    return (
                      <td key={column.key} className={`px-4 py-3 text-slate-800 ${align}`}>
                        {content}
                      </td>
                    );
                  })}
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {pageCount > 1 ? (
          <div className="flex items-center justify-between gap-3 text-sm text-slate-600">
            <p>
              Página {currentPage} de {pageCount} · {processedRows.length} registros
            </p>
            <div className="flex gap-2">
              <button
                type="button"
                className="rounded-md border border-slate-200 bg-white px-3 py-1 disabled:opacity-50"
                disabled={currentPage <= 1}
                onClick={() => setPage((value) => Math.max(1, value - 1))}
              >
                Anterior
              </button>
              <button
                type="button"
                className="rounded-md border border-slate-200 bg-white px-3 py-1 disabled:opacity-50"
                disabled={currentPage >= pageCount}
                onClick={() => setPage((value) => Math.min(pageCount, value + 1))}
              >
                Siguiente
              </button>
            </div>
          </div>
        ) : null}
      </FeedbackState>
    </div>
  );
}
