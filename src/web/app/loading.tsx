export default function Loading() {
  return (
    <div
      aria-busy="true"
      aria-label="Cargando usuarios"
      className="loading-stack"
      role="status"
    >
      <span className="sr-only">Cargando usuarios…</span>
      <div className="skeleton h-8 w-36" />
      <div className="skeleton h-12 w-80 max-w-full" />
      <div className="skeleton mt-8 h-20 w-full" />
      <div className="skeleton mt-6 h-96 w-full" />
    </div>
  );
}
