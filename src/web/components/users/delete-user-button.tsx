"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";

import { deleteUser } from "@/lib/api/client";
import { ApiError } from "@/lib/api/request";

type DeleteUserButtonProps = {
  userId: number;
  userName: string;
};

export function DeleteUserButton({ userId, userName }: DeleteUserButtonProps) {
  const router = useRouter();
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function remove() {
    const confirmed = window.confirm(
      `¿Eliminar permanentemente a ${userName}? Esta acción no se puede deshacer.`,
    );
    if (!confirmed) {
      return;
    }

    setDeleting(true);
    setError(null);
    try {
      await deleteUser(userId);
      router.push("/");
      router.refresh();
    } catch (caughtError) {
      setError(
        caughtError instanceof ApiError
          ? (caughtError.problem.detail ??
              "La API no pudo eliminar el usuario.")
          : "No fue posible comunicarse con la API.",
      );
      setDeleting(false);
    }
  }

  return (
    <div className="flex flex-col items-end gap-2">
      <button
        className="button button-danger"
        disabled={deleting}
        onClick={remove}
        type="button"
      >
        {deleting ? "Eliminando…" : "Eliminar"}
      </button>
      {error ? (
        <p className="max-w-xs text-right text-sm text-red-700" role="alert">
          {error}
        </p>
      ) : null}
    </div>
  );
}
