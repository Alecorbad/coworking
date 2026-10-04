import { QueryObserver } from "@tanstack/react-query";
import { describe, expect, it, vi } from "vitest";
import { meQueryKey } from "@/features/auth/api";
import { ApiError } from "@/lib/api/api-error";
import { createQueryClient } from "./providers";

describe("sessione e risposte 401", () => {
  it("lascia il 401 di me nello stato di errore senza rilanciare la richiesta", async () => {
    const client = createQueryClient();
    const fetchMe = vi.fn().mockRejectedValue(new ApiError(401, "UNAUTHORIZED"));
    const me = new QueryObserver(client, {
      queryKey: meQueryKey,
      queryFn: fetchMe,
      retry: false,
    });
    const unsubscribe = me.subscribe(() => {});
    try {
      const result = await me.refetch();
      expect(result.status).toBe("error");
      expect(fetchMe).toHaveBeenCalledTimes(1);
      expect(result.error).toMatchObject({ status: 401 });
    } finally {
      unsubscribe();
      client.clear();
    }
  });

  it("un 401 dei dati rilegge me una volta e rende visibile la sessione scaduta", async () => {
    const client = createQueryClient();
    client.setQueryData(meQueryKey, { email: "receptionist@example.test" });
    const fetchMe = vi.fn().mockRejectedValue(new ApiError(401, "UNAUTHORIZED"));
    const me = new QueryObserver(client, {
      queryKey: meQueryKey,
      queryFn: fetchMe,
      retry: false,
      staleTime: Infinity,
    });
    let sessionLost!: () => void;
    const lost = new Promise<void>((resolve) => {
      sessionLost = resolve;
    });
    const unsubscribe = me.subscribe((result) => {
      if (result.isError) sessionLost();
    });
    try {
      await expect(
        client.fetchQuery({
          queryKey: ["rooms"],
          queryFn: () => Promise.reject(new ApiError(401, "UNAUTHORIZED")),
          retry: false,
        }),
      ).rejects.toMatchObject({ status: 401 });
      await lost;
      expect(fetchMe).toHaveBeenCalledTimes(1);
      expect(me.getCurrentResult().error).toMatchObject({ status: 401 });
    } finally {
      unsubscribe();
      client.clear();
    }
  });

  it("le credenziali errate restano nel login senza invalidare me", async () => {
    const client = createQueryClient();
    client.setQueryData(meQueryKey, { email: "receptionist@example.test" });
    try {
      const mutation = client.getMutationCache().build(client, {
        mutationFn: () => Promise.reject(new ApiError(401, "INVALID_CREDENTIALS")),
      });
      await expect(mutation.execute(undefined)).rejects.toMatchObject({
        code: "INVALID_CREDENTIALS",
      });
      expect(client.getQueryState(meQueryKey)?.isInvalidated).toBe(false);
    } finally {
      client.clear();
    }
  });
});
