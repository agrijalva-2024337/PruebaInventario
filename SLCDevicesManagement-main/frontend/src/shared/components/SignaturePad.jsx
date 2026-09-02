import { useEffect, useRef } from 'react';
import { Button } from '@/shared/components/Button';

export function SignaturePad({
  label,
  name,
  onChange,
  error,
  required = false,
  disabled = false,
}) {
  const canvasRef = useRef(null);
  const drawingRef = useRef(false);
  const onChangeRef = useRef(onChange);
  onChangeRef.current = onChange;

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) {
      return undefined;
    }

    const context = canvas.getContext('2d');
    const ratio = window.devicePixelRatio || 1;
    const width = canvas.clientWidth || 320;
    const height = 110;
    canvas.width = width * ratio;
    canvas.height = height * ratio;
    context.setTransform(ratio, 0, 0, ratio, 0, 0);
    context.lineWidth = 2;
    context.lineCap = 'round';
    context.strokeStyle = '#0f172a';
    context.fillStyle = '#ffffff';
    context.fillRect(0, 0, width, height);

    function point(event) {
      const rect = canvas.getBoundingClientRect();
      const source = event.touches?.[0] ?? event;
      return { x: source.clientX - rect.left, y: source.clientY - rect.top };
    }

    function start(event) {
      if (disabled) {
        return;
      }
      event.preventDefault();
      drawingRef.current = true;
      const { x, y } = point(event);
      context.beginPath();
      context.moveTo(x, y);
    }

    function move(event) {
      if (!drawingRef.current || disabled) {
        return;
      }
      event.preventDefault();
      const { x, y } = point(event);
      context.lineTo(x, y);
      context.stroke();
    }

    function end() {
      if (!drawingRef.current) {
        return;
      }
      drawingRef.current = false;
      onChangeRef.current({ name, value: canvas.toDataURL('image/png') });
    }

    canvas.addEventListener('pointerdown', start);
    canvas.addEventListener('pointermove', move);
    window.addEventListener('pointerup', end);
    canvas.addEventListener('touchstart', start, { passive: false });
    canvas.addEventListener('touchmove', move, { passive: false });
    window.addEventListener('touchend', end);

    return () => {
      canvas.removeEventListener('pointerdown', start);
      canvas.removeEventListener('pointermove', move);
      window.removeEventListener('pointerup', end);
      canvas.removeEventListener('touchstart', start);
      canvas.removeEventListener('touchmove', move);
      window.removeEventListener('touchend', end);
    };
  }, [disabled, name]);

  function handleClear() {
    const canvas = canvasRef.current;
    if (canvas) {
      const context = canvas.getContext('2d');
      const ratio = window.devicePixelRatio || 1;
      context.setTransform(ratio, 0, 0, ratio, 0, 0);
      context.fillStyle = '#ffffff';
      context.fillRect(0, 0, canvas.clientWidth || 320, 110);
    }
    onChange({ name, value: '' });
  }

  return (
    <div>
      <p className="text-sm font-medium text-slate-700">
        {label}
        {required ? <span className="text-red-600"> *</span> : null}
      </p>
      <canvas
        ref={canvasRef}
        className="mt-1 h-[110px] w-full touch-none rounded-md border border-slate-200 bg-white"
      />
      <div className="mt-2">
        <Button type="button" variant="ghost" onClick={handleClear} disabled={disabled}>
          Limpiar firma
        </Button>
      </div>
      {error ? <p className="mt-1 text-xs text-red-600">{error}</p> : null}
    </div>
  );
}
