import { useEffect, useState } from 'react';
import QRCode from 'qrcode';
import { Button } from '@/shared/components/Button';
import { buildActivoQrUrl } from '@/shared/utils/qr';

export function QrThumb({ activoId, size = 56, alt }) {
  const [src, setSrc] = useState('');
  const url = buildActivoQrUrl(activoId);

  useEffect(() => {
    let cancelled = false;
    QRCode.toDataURL(url, { width: size, margin: 0, errorCorrectionLevel: 'M' }).then((dataUrl) => {
      if (!cancelled) {
        setSrc(dataUrl);
      }
    });
    return () => {
      cancelled = true;
    };
  }, [url, size]);

  if (!src) {
    return <span className="inline-block bg-slate-100" style={{ width: size, height: size }} />;
  }

  return <img src={src} alt={alt ?? `QR ${activoId}`} width={size} height={size} className="inline-block" />;
}

export function QrCodeCard({ activoId, nombre }) {
  const [src, setSrc] = useState('');
  const url = buildActivoQrUrl(activoId);

  useEffect(() => {
    let cancelled = false;
    QRCode.toDataURL(url, { width: 280, margin: 1, errorCorrectionLevel: 'M' }).then((dataUrl) => {
      if (!cancelled) {
        setSrc(dataUrl);
      }
    });
    return () => {
      cancelled = true;
    };
  }, [url]);

  function downloadPng() {
    if (!src) {
      return;
    }
    const link = document.createElement('a');
    link.href = src;
    link.download = `qr-activo-${activoId}.png`;
    link.click();
  }

  return (
    <div className="space-y-3 text-center">
      {src ? (
        <img src={src} alt={`Código QR de ${nombre ?? `activo ${activoId}`}`} className="mx-auto" />
      ) : (
        <p className="text-sm text-slate-500">Generando QR...</p>
      )}
      <p className="text-sm font-medium text-slate-800">{nombre ?? `Activo #${activoId}`}</p>
      <p className="break-all font-mono text-xs text-slate-500">{url}</p>
      <Button type="button" variant="secondary" onClick={downloadPng} disabled={!src}>
        Descargar QR
      </Button>
      <p className="text-xs text-slate-500">
        Al escanearlo, cualquiera ve la ficha pública del equipo. El personal con sesión
        puede abrir el inventario y registrar movimientos.
      </p>
    </div>
  );
}
