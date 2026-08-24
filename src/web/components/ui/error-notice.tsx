import Link from "next/link";

type ErrorNoticeProps = {
  title?: string;
  message: string;
  retryHref?: string;
};

export function ErrorNotice({
  title = "No pudimos cargar esta información",
  message,
  retryHref,
}: ErrorNoticeProps) {
  return (
    <section aria-live="polite" className="error-notice" role="alert">
      <div className="error-icon">!</div>
      <div>
        <h2>{title}</h2>
        <p>{message}</p>
        {retryHref ? (
          <Link className="text-link mt-3 inline-flex" href={retryHref}>
            Intentar de nuevo
          </Link>
        ) : null}
      </div>
    </section>
  );
}
