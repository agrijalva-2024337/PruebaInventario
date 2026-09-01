export function NotFoundPage() {
  return (
    <section className="space-y-3">
      <p className="text-sm font-medium uppercase tracking-widest text-slate-500">Error 404</p>
      <h2 className="text-3xl font-semibold tracking-tight text-slate-900">Página no encontrada</h2>
      <p className="max-w-xl text-slate-600">
        La ruta que buscás no existe o todavía no está disponible en este sprint.
      </p>
    </section>
  );
}
