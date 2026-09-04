import { useEffect, useMemo, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { AlertBanner } from '@/shared/components/AlertBanner';
import { Badge } from '@/shared/components/Badge';
import { PageHeader } from '@/shared/components/PageHeader';
import { env } from '@/shared/config/env';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';
import * as dispositivoService from '@/features/rastreo/dispositivoService';

function toNumber(value) {
  const n = Number(value);
  return Number.isFinite(n) ? n : null;
}

function esCoordUtil(lat, lng) {
  if (lat == null || lng == null) {
    return false;
  }
  if (Math.abs(lat) < 0.05 && Math.abs(lng) < 0.05) {
    return false;
  }
  return lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180;
}

function etiquetaOrigen(origen) {
  if (origen === 'gps') {
    return 'GPS del equipo';
  }
  if (origen === 'wifi') {
    return 'Inferida por Wi-Fi';
  }
  return 'Última posición';
}

function puntoVisible(row) {
  const deviceLat = toNumber(row.ultimaLatitud);
  const deviceLng = toNumber(row.ultimaLongitud);
  if (esCoordUtil(deviceLat, deviceLng)) {
    return {
      lat: deviceLat,
      lng: deviceLng,
      etiqueta: row.ubicacionDetectada?.nombre || etiquetaOrigen(row.origenCoordenada),
      origen: row.origenCoordenada === 'gps' ? 'gps' : 'detectada',
    };
  }

  const detectedLat = toNumber(row.ubicacionDetectada?.latitud);
  const detectedLng = toNumber(row.ubicacionDetectada?.longitud);
  if (esCoordUtil(detectedLat, detectedLng)) {
    return {
      lat: detectedLat,
      lng: detectedLng,
      etiqueta: row.ubicacionDetectada.nombre,
      origen: 'detectada',
    };
  }

  return null;
}

function loadGoogleMaps(apiKey) {
  if (window.google?.maps) {
    return Promise.resolve(window.google.maps);
  }

  const existing = document.getElementById('google-maps-js');
  if (existing) {
    return new Promise((resolve, reject) => {
      existing.addEventListener('load', () => resolve(window.google.maps), { once: true });
      existing.addEventListener('error', () => reject(new Error('No se pudo cargar Google Maps.')), {
        once: true,
      });
    });
  }

  return new Promise((resolve, reject) => {
    const script = document.createElement('script');
    script.id = 'google-maps-js';
    script.src = `https://maps.googleapis.com/maps/api/js?key=${encodeURIComponent(apiKey)}`;
    script.async = true;
    script.onload = () => resolve(window.google.maps);
    script.onerror = () => reject(new Error('No se pudo cargar Google Maps. Revisa la API key.'));
    document.head.appendChild(script);
  });
}

export function MapaRastreoPage() {
  const containerRef = useRef(null);
  const mapCleanupRef = useRef(null);
  const [rows, setRows] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [banner, setBanner] = useState(null);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setIsLoading(true);
      try {
        const data = await dispositivoService.getMapa();
        if (!cancelled) {
          setRows(Array.isArray(data) ? data : []);
        }
      } catch (error) {
        if (!cancelled) {
          setBanner({ variant: 'error', message: getErrorMessage(error) });
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, []);

  const puntos = useMemo(
    () =>
      rows
        .map((row) => ({ row, punto: puntoVisible(row) }))
        .filter((item) => item.punto),
    [rows],
  );

  useEffect(() => {
    const el = containerRef.current;
    if (!el || puntos.length === 0) {
      return undefined;
    }

    let cancelled = false;

    async function renderMap() {
      if (mapCleanupRef.current) {
        mapCleanupRef.current();
        mapCleanupRef.current = null;
      }

      const center = { lat: puntos[0].punto.lat, lng: puntos[0].punto.lng };

      if (env.googleMapsApiKey) {
        const maps = await loadGoogleMaps(env.googleMapsApiKey);
        if (cancelled || !containerRef.current) {
          return;
        }

        const map = new maps.Map(containerRef.current, {
          center,
          zoom: 12,
          mapTypeControl: false,
        });
        const bounds = new maps.LatLngBounds();

        puntos.forEach(({ row, punto }) => {
          const position = { lat: punto.lat, lng: punto.lng };
          bounds.extend(position);
          const marker = new maps.Marker({
            map,
            position,
            title: row.nombreActivo,
          });
          const info = new maps.InfoWindow({
            content: `<strong>${row.nombreActivo}</strong><br/>${punto.etiqueta}<br/>${
              row.fueraDeRango ? 'Fuera de rango' : 'En ubicacion'
            }`,
          });
          marker.addListener('click', () => info.open({ map, anchor: marker }));
        });

        if (puntos.length > 1) {
          map.fitBounds(bounds, 48);
        }

        mapCleanupRef.current = () => {
          containerRef.current?.replaceChildren();
        };
        return;
      }

      const leaflet = await import('leaflet');
      await import('leaflet/dist/leaflet.css');
      if (cancelled || !containerRef.current) {
        return;
      }

      const L = leaflet.default;
      const map = L.map(containerRef.current).setView([center.lat, center.lng], 12);
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap',
        maxZoom: 19,
      }).addTo(map);

      const bounds = [];
      puntos.forEach(({ row, punto }) => {
        bounds.push([punto.lat, punto.lng]);
        L.circleMarker([punto.lat, punto.lng], {
          radius: 10,
          color: row.fueraDeRango ? '#b91c1c' : '#15803d',
          fillColor: row.fueraDeRango ? '#ef4444' : '#22c55e',
          fillOpacity: 0.9,
          weight: 2,
        })
          .addTo(map)
          .bindPopup(
            `<strong>${row.nombreActivo}</strong><br/>${punto.etiqueta}<br/>${
              row.fueraDeRango ? 'Fuera de rango' : 'En ubicación'
            }`,
          );
      });

      if (bounds.length > 1) {
        map.fitBounds(bounds, { padding: [40, 40] });
      }

      mapCleanupRef.current = () => {
        map.remove();
      };
    }

    renderMap().catch((error) => {
      setBanner({ variant: 'error', message: getErrorMessage(error) });
    });

    return () => {
      cancelled = true;
      if (mapCleanupRef.current) {
        mapCleanupRef.current();
        mapCleanupRef.current = null;
      }
    };
  }, [puntos]);

  return (
    <section className="space-y-6">
      <PageHeader
        title="Mapa de equipos"
        description={
          env.googleMapsApiKey
            ? 'El pin es la última posición real del equipo (GPS usable o red Wi-Fi catalogada). No se usa 0,0 ni la sede asignada como si el equipo estuviera ahí. El mapa usa Google Maps.'
            : 'El pin es la última posición real del equipo (GPS usable o red Wi-Fi catalogada). No se usa 0,0 ni la sede asignada como si el equipo estuviera ahí. OpenStreetMap (sin API key).'
        }
      />

      {banner ? (
        <AlertBanner
          variant={banner.variant}
          message={banner.message}
          onDismiss={() => setBanner(null)}
        />
      ) : null}

      {isLoading ? <p className="text-sm text-slate-500">Cargando posiciones...</p> : null}

      {!isLoading && puntos.length === 0 ? (
        <p className="text-sm text-slate-500">
          No hay equipos con coordenadas. El agente debe enviar un ping, o catalogá el BSSID y
          asegurate de que la ubicación tenga latitud y longitud.
        </p>
      ) : (
        <div
          ref={containerRef}
          className="h-[520px] w-full overflow-hidden rounded-lg border border-slate-200 bg-slate-100"
        />
      )}

      {rows.length > 0 ? (
        <ul className="space-y-2 text-sm">
          {rows.map((row) => {
            const punto = puntoVisible(row);
            return (
              <li
                key={row.idActivo}
                className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-slate-200 bg-white px-3 py-2"
              >
                <div>
                  <Link className="font-medium text-slate-900 underline" to={`/activos/${row.idActivo}`}>
                    {row.nombreActivo}
                  </Link>
                  <p className="text-slate-500">
                    {punto
                      ? `${punto.etiqueta} (${punto.origen})`
                      : 'Sin coordenadas reales: red desconocida, GPS vacío (0,0) o ubicación sin lat/lng válidos'}
                  </p>
                </div>
                <Badge variant={row.fueraDeRango ? 'danger' : 'success'}>
                  {row.fueraDeRango ? 'Fuera de rango' : 'En ubicación'}
                </Badge>
              </li>
            );
          })}
        </ul>
      ) : null}
    </section>
  );
}
