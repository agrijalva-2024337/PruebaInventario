import { FeedbackState } from '@/shared/components/FeedbackState';
import { env } from '@/shared/config/env';
import { useApiHealth } from '@/shared/hooks/useApiHealth';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';

export function HomePage() {
  const { status, data, error } = useApiHealth();

  return (
    <section className="space-y-8">
      <div>
        <p className="text-sm font-medium uppercase tracking-widest text-slate-500">
          Sistemas Logísticos y Corporativos, S.A.
        </p>
        <h2 className="mt-2 text-3xl font-semibold tracking-tight text-slate-900">
          SLCDevicesManagement
        </h2>
        <p className="mt-3 max-w-2xl text-slate-600">
          Andamiaje del frontend conectado a SLCDM.Api. El dashboard comprueba
          el cliente HTTP contra GET /api/health.
        </p>
      </div>

      <dl className="grid gap-4 sm:grid-cols-2">
        <div className="rounded-lg border border-slate-200 bg-white p-4">
          <dt className="text-xs font-semibold uppercase tracking-wide text-slate-500">
            VITE_API_URL
          </dt>
          <dd className="mt-1 break-all font-mono text-sm text-slate-800">
            {env.apiUrl || '(no definida)'}
          </dd>
        </div>
        <div className="rounded-lg border border-slate-200 bg-white p-4">
          <dt className="text-xs font-semibold uppercase tracking-wide text-slate-500">
            VITE_USE_API_MOCK
          </dt>
          <dd className="mt-1 font-mono text-sm text-slate-800">{String(env.useApiMock)}</dd>
        </div>
      </dl>

      <div className="rounded-lg border border-slate-200 bg-white p-5">
        <h3 className="text-sm font-semibold text-slate-900">Estado del cliente HTTP</h3>
        <p className="mt-1 text-sm text-slate-500">
          {env.useApiMock
            ? 'Modo mock: no se llama a la API real.'
            : 'Modo API: GET /api/health.'}
        </p>

        <div className="mt-4">
          <FeedbackState
            status={status}
            loadingMessage="Comprobando el cliente HTTP..."
            errorMessage={getErrorMessage(error)}
          >
            {data ? (
              <div className="rounded-md bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
                <p className="font-medium">{data.message}</p>
                <p className="mt-1 text-emerald-700">Origen: {data.source}</p>
              </div>
            ) : null}
          </FeedbackState>
        </div>
      </div>
    </section>
  );
}
