"use client";

import { useEffect } from "react";

type ErrorPageProps = {
  error: Error & { digest?: string };
  reset: () => void;
};

export default function ErrorPage({ error, reset }: ErrorPageProps) {
  useEffect(() => {
    console.error("Unexpected user interface error", error);
  }, [error]);

  return (
    <section className="error-notice" role="alert">
      <div className="error-icon">!</div>
      <div>
        <h1>Algo salió mal</h1>
        <p>No pudimos completar esta vista. Tus datos no fueron modificados.</p>
        <button
          className="button button-secondary mt-4"
          onClick={reset}
          type="button"
        >
          Intentar de nuevo
        </button>
      </div>
    </section>
  );
}
