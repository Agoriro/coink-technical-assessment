import type { Metadata } from "next";
import Link from "next/link";
import type { ReactNode } from "react";

import "./globals.css";

export const metadata: Metadata = {
  title: {
    default: "Usuarios | COINK",
    template: "%s | COINK",
  },
  description:
    "Gestión de usuarios y ubicación geográfica para la evaluación técnica COINK.",
};

export default function RootLayout({
  children,
}: Readonly<{ children: ReactNode }>) {
  return (
    <html lang="es">
      <body>
        <div className="site-shell">
          <header className="site-header">
            <div className="site-header-inner">
              <Link aria-label="COINK, inicio" className="brand" href="/">
                <span className="brand-mark" aria-hidden="true">
                  C
                </span>
                <span>COINK</span>
              </Link>
              <div className="header-context">
                <span className="status-dot" aria-hidden="true" />
                Gestión de usuarios
              </div>
            </div>
          </header>
          <main className="main-content">{children}</main>
          <footer className="site-footer">
            <p>Evaluación técnica · Interfaz de administración</p>
            <p>Colombia</p>
          </footer>
        </div>
      </body>
    </html>
  );
}
