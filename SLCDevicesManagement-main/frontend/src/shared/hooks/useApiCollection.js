import { useCallback, useEffect, useState } from 'react';
import { subscribeEmpresaActiva } from '@/shared/empresaActiva';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';

export function useApiCollection(loadFn) {
  const [rows, setRows] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState(null);
  const [banner, setBanner] = useState(null);

  const reload = useCallback(async () => {
    setIsLoading(true);

    try {
      const data = await loadFn();
      setRows(Array.isArray(data) ? data : []);
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
          setRows(Array.isArray(data) ? data : []);
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
    return subscribeEmpresaActiva(() => {
      reload();
    });
  }, [reload]);

  useEffect(() => {
    if (!banner) {
      return undefined;
    }

    const timeoutId = window.setTimeout(() => setBanner(null), 4000);
    return () => window.clearTimeout(timeoutId);
  }, [banner]);

  return { rows, isLoading, errorMessage, banner, setBanner, reload };
}
