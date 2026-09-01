import { useCallback, useEffect, useState } from 'react';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';

export function useResource(loadFn) {
  const [data, setData] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState(null);

  const reload = useCallback(async () => {
    setIsLoading(true);

    try {
      const result = await loadFn();
      setData(result);
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
        const result = await loadFn();

        if (!cancelled) {
          setData(result);
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

  return { data, isLoading, errorMessage, reload };
}
