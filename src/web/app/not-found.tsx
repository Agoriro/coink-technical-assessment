import Link from "next/link";

export default function NotFound() {
  return (
    <section className="not-found">
      <p className="eyebrow">404</p>
      <h1>No encontramos este usuario</h1>
      <p>El registro no existe o ya fue eliminado.</p>
      <Link className="button button-primary mt-6" href="/">
        Volver al directorio
      </Link>
    </section>
  );
}
