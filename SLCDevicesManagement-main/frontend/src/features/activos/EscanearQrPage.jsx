import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import jsQR from 'jsqr';
import { AlertBanner } from '@/shared/components/AlertBanner';
import { Button } from '@/shared/components/Button';
import { PageHeader } from '@/shared/components/PageHeader';
import { parseActivoIdFromQr } from '@/shared/utils/qr';

export function EscanearQrPage({ publicMode = false }) {
  const navigate = useNavigate();
  const videoRef = useRef(null);
  const canvasRef = useRef(null);
  const streamRef = useRef(null);
  const frameRef = useRef(0);
  const [error, setError] = useState(null);
  const [running, setRunning] = useState(false);

  useEffect(() => {
    return () => {
      cancelAnimationFrame(frameRef.current);
      streamRef.current?.getTracks().forEach((track) => track.stop());
    };
  }, []);

  function goToActivo(text) {
    const id = parseActivoIdFromQr(text);
    if (!id) {
      setError('El código no corresponde a un activo de DERCAS.');
      return false;
    }
    streamRef.current?.getTracks().forEach((track) => track.stop());
    navigate(publicMode ? `/consulta/${id}` : `/activos/${id}`);
    return true;
  }

  function scanFrame() {
    const video = videoRef.current;
    const canvas = canvasRef.current;
    if (!video || !canvas || video.readyState < 2) {
      frameRef.current = requestAnimationFrame(scanFrame);
      return;
    }

    const context = canvas.getContext('2d', { willReadFrequently: true });
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    context.drawImage(video, 0, 0, canvas.width, canvas.height);
    const image = context.getImageData(0, 0, canvas.width, canvas.height);
    const result = jsQR(image.data, image.width, image.height);
    if (result?.data && goToActivo(result.data)) {
      return;
    }
    frameRef.current = requestAnimationFrame(scanFrame);
  }

  async function startCamera() {
    setError(null);
    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'environment' },
        audio: false,
      });
      streamRef.current = stream;
      if (videoRef.current) {
        videoRef.current.srcObject = stream;
        await videoRef.current.play();
      }
      setRunning(true);
      frameRef.current = requestAnimationFrame(scanFrame);
    } catch {
      setError('No se pudo abrir la cámara. Prueba con una foto del QR.');
    }
  }

  function handleFile(event) {
    const file = event.target.files?.[0];
    if (!file) {
      return;
    }

    const image = new Image();
    image.onload = () => {
      const canvas = canvasRef.current;
      const context = canvas.getContext('2d');
      canvas.width = image.width;
      canvas.height = image.height;
      context.drawImage(image, 0, 0);
      const data = context.getImageData(0, 0, canvas.width, canvas.height);
      const result = jsQR(data.data, data.width, data.height);
      if (!result?.data || !goToActivo(result.data)) {
        setError('No se leyó un QR de activo en esa imagen.');
      }
    };
    image.src = URL.createObjectURL(file);
  }

  return (
    <section className="space-y-6">
      <PageHeader
        title="Escanear QR"
        description={
          publicMode
            ? 'No necesitas cuenta. Al leer el QR verás los datos públicos del equipo.'
            : 'Apunta la cámara al código del activo para abrir la ficha. Quien no tenga sesión verá solo la consulta pública.'
        }
      />
      {error ? <AlertBanner variant="error" message={error} onDismiss={() => setError(null)} /> : null}
      <div className="space-y-3 rounded-lg border border-slate-200 bg-white p-4">
        <video ref={videoRef} className="w-full rounded-md bg-slate-900" playsInline muted />
        <canvas ref={canvasRef} className="hidden" />
        <div className="flex flex-wrap gap-2">
          <Button type="button" onClick={startCamera} disabled={running}>
            Abrir cámara
          </Button>
          <label className="inline-flex cursor-pointer items-center rounded-md border border-slate-200 bg-white px-3 py-2 text-sm font-medium">
            Subir foto del QR
            <input type="file" accept="image/*" capture="environment" className="hidden" onChange={handleFile} />
          </label>
        </div>
      </div>
    </section>
  );
}
