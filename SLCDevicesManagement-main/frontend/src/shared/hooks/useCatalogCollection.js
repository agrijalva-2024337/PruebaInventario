import { useCallback, useEffect, useMemo, useState } from 'react';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';

export function useCatalogCollection(loadFn) {
  const [rows, setRows] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState(null);
  const [filter, setFilter] = useState('activos');
  const [banner, setBanner] = useState(null);

  const reload = useCallback(async () => {
    setIsLoading(true);

    try {
      const data = await loadFn();
      setRows(data);
      setErrorMessage(null);
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsLoading(false);
    }
  }, [loadFn]);

  useEffect(() => {
    let cancelled = false;

    async function run() {
      try {
        const data = await loadFn();

        if (!cancelled) {
          setRows(data);
          setErrorMessage(null);
          setIsLoading(false);
        }
      } catch (error) {
        if (!cancelled) {
          setErrorMessage(getErrorMessage(error));
          setIsLoading(false);
        }
      }
    }

    run();

    return () => {
      cancelled = true;
    };
  }, [loadFn]);

  useEffect(() => {
    if (!banner) {
      return undefined;
    }

    const timeoutId = window.setTimeout(() => setBanner(null), 4000);
    return () => window.clearTimeout(timeoutId);
  }, [banner]);

  const visibleRows = useMemo(() => {
    if (filter === 'todos') {
      return rows;
    }

    if (filter === 'inactivos') {
      return rows.filter((row) => !row.habilitado);
    }

    return rows.filter((row) => row.habilitado);
  }, [filter, rows]);

  return {
    rows,
    visibleRows,
    isLoading,
    errorMessage,
    filter,
    setFilter,
    banner,
    setBanner,
    reload,
  };
}
