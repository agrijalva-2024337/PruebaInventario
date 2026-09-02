import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { AlertBanner } from '@/shared/components/AlertBanner';
import { Badge } from '@/shared/components/Badge';
import { PageHeader } from '@/shared/components/PageHeader';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';
import { formatDate } from '@/shared/utils/dates';
import { useAuth } from '@/features/auth/AuthContext';
import { estadoBadgeVariant, estadoNombre } from '@/features/activos/activoEstado';
import * as consultaService from '@/features/consulta/consultaService';

function Field({ label, value }) {
  return (
    <div>
      <dt className="text-xs uppercase tracking-wide text-slate-500">{label}</dt>
      <dd className="mt-0.5 text-sm text-slate-900">{value || '—'}</dd>
    </div>
  );
}

export function ConsultaActivoPage() {
  const { id } = useParams();
  const { isAuthenticated } = useAuth();
  const activoId = Number(id);
  const [activo, setActivo] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState(null);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setIsLoading(true);
      setErrorMessage(null);
      try {
        const item = await consultaService.getConsultaActivo(activoId);
        if (!cancelled) {
          setActivo(item);
        }
      } catch (error) {
        if (!cancelled) {
          setActivo(null);
          setErrorMessage(getErrorMessage(error) || 'No se encontró este activo.');
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    if (Number.isFinite(activoId) && activoId > 0) {
      load();
    } else {
      setIsLoading(false);
      setErrorMessage('El código del activo no es válido.');
    }

    return () => {
      cancelled = true;
    };
  }, [activoId]);

  return (
    <section className="space-y-6">
      <PageHeader
        title={activo?.nombre ?? 'Consulta de activo'}
        description="Datos públicos del equipo. No se muestra costo, factura ni historial."
      />
      {errorMessage ? <AlertBanner variant="error" message={errorMessage} /> : null}
      {isLoading ? <p className="text-sm text-slate-500">Cargando ficha del activo...</p> : null}

      {activo ? (
        <dl className="grid gap-3 rounded-lg border border-slate-200 bg-white p-4 sm:grid-cols-2">
          <Field label="Código interno" value={activo.codigoInterno} />
          <Field label="Serie" value={activo.numeroSerie} />
          <Field label="Estado" value={<Badge variant={estadoBadgeVariant(activo)}>{estadoNombre(activo)}</Badge>} />
          <Field label="Categoría" value={activo.nombreCategoria} />
          <Field label="Marca / modelo" value={[activo.marca, activo.modelo].filter(Boolean).join(' ')} />
          <Field label="Empresa" value={activo.nombreEmpresa} />
          <Field label="Sede" value={activo.nombreSede} />
          <Field label="Ubicación" value={activo.nombreUbicacion} />
          <Field label="Área" value={activo.nombreArea} />
          <Field label="Responsable" value={activo.nombreResponsable} />
          <Field
            label="Garantía hasta"
            value={activo.fechaVencimientoGarantia ? formatDate(activo.fechaVencimientoGarantia) : null}
          />
          {activo.descripcion ? (
            <div className="sm:col-span-2">
              <dt className="text-xs uppercase tracking-wide text-slate-500">Descripción</dt>
              <dd className="mt-0.5 text-sm text-slate-900">{activo.descripcion}</dd>
            </div>
          ) : null}
        </dl>
      ) : null}

      <div className="flex flex-wrap gap-3 text-sm">
        <Link className="font-medium text-slate-900 underline" to="/escanear">
          Escanear otro QR
        </Link>
        {isAuthenticated && activo ? (
          <Link className="font-medium text-slate-900 underline" to={`/activos/${activo.id}`}>
            Abrir ficha completa
          </Link>
        ) : (
          <Link className="text-slate-600 underline" to="/login">
            Entrar al inventario
          </Link>
        )}
      </div>
    </section>
  );
}
