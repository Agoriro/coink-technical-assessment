import Link from "next/link";
import type { ReactNode } from "react";

import { ArrowLeftIcon } from "@/components/ui/icons";

type PageHeadingProps = {
  eyebrow: string;
  title: string;
  description: string;
  backHref?: string;
  actions?: ReactNode;
};

export function PageHeading({
  eyebrow,
  title,
  description,
  backHref,
  actions,
}: PageHeadingProps) {
  return (
    <div className="page-heading">
      <div className="min-w-0">
        {backHref ? (
          <Link className="back-link" href={backHref}>
            <ArrowLeftIcon className="size-4" />
            Volver a usuarios
          </Link>
        ) : null}
        <p className="eyebrow">{eyebrow}</p>
        <h1>{title}</h1>
        <p className="page-description">{description}</p>
      </div>
      {actions ? (
        <div className="flex shrink-0 flex-wrap gap-3">{actions}</div>
      ) : null}
    </div>
  );
}
