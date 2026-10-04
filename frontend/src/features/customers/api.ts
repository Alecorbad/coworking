// Ruolo:   hook dei clienti: elenco, dettaglio e modifiche
// Riceve:  i valori letti dal modulo del cliente
// Produce: le query ["customers"] e ["customers", id], le mutazioni di creazione, modifica e cancellazione
// Regola:  dopo un salvataggio, riuscito o respinto con 409, elenco e dettaglio si rileggono
// Vedi:    features/customers/customer-dialog.tsx, app/(app)/customers/

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { api } from "@/lib/api/client";
import { ApiError } from "@/lib/api/api-error";
import type { components } from "@/lib/api/schema";
import { unwrap } from "@/lib/api/unwrap";

export type Customer = components["schemas"]["CustomerResponse"];
export type CreateCustomer = components["schemas"]["CreateCustomerRequest"];
export type UpdateCustomer = components["schemas"]["UpdateCustomerRequest"];

// Prefisso comune: invalidarlo rilegge sia l'elenco sia ogni dettaglio.
export const customersQueryKey = ["customers"] as const;

export function useCustomers() {
  return useQuery({
    queryKey: customersQueryKey,
    queryFn: () => unwrap(api.GET("/api/customers")),
  });
}

export function useCustomer(id: number) {
  return useQuery({
    queryKey: [...customersQueryKey, id],
    queryFn: () => unwrap(api.GET("/api/customers/{id}", { params: { path: { id } } })),
  });
}

// Successo o 409 (email presa, salvato da un altro operatore): in entrambi i casi i dati
// sullo schermo possono essere vecchi, quindi si rileggono.
function useRefreshCustomers() {
  const queryClient = useQueryClient();
  const refresh = () => queryClient.invalidateQueries({ queryKey: customersQueryKey });
  return {
    onSuccess: refresh,
    onError: (error: unknown) => {
      if (error instanceof ApiError && error.status === 409) void refresh();
    },
  };
}

export function useCreateCustomer() {
  return useMutation({
    mutationFn: (body: CreateCustomer) => unwrap(api.POST("/api/customers", { body })),
    ...useRefreshCustomers(),
  });
}

export function useUpdateCustomer(id: number) {
  return useMutation({
    mutationFn: (body: UpdateCustomer) =>
      unwrap(api.PUT("/api/customers/{id}", { params: { path: { id } }, body })),
    ...useRefreshCustomers(),
  });
}

export function useDeleteCustomer() {
  return useMutation({
    mutationFn: (id: number) =>
      unwrap(api.DELETE("/api/customers/{id}", { params: { path: { id } } })),
    ...useRefreshCustomers(),
  });
}
