import { useEffect, useState } from 'react';
import { checkApiHealth } from '@/shared/services/healthService';

export function useApiHealth() {
  const [status, setStatus] = useState('idle');
  const [data, setData] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    let cancelled = false;

    async function run() {
      setStatus('loading');
      setError(null);

      try {
        const result = await checkApiHealth();

        if (!cancelled) {
          setData(result);
          setStatus('success');
        }
      } catch (err) {
        if (!cancelled) {
          setError(err);
          setStatus('error');
        }
      }
    }

    run();

    return () => {
      cancelled = true;
    };
  }, []);

  return { status, data, error };
}
