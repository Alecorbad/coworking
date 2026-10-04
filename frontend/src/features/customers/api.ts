// Ruolo:   hook dei clienti: elenco, dettaglio e modifiche
// Riceve:  i valori letti dal modulo del cliente
// Produce: le query ["customers"] e ["customers", id], le mutazioni di creazione, modifica e cancellazione
// Regola:  dopo un salvataggio, riuscito o respinto con 409, elenco e dettaglio si rileggono
// Vedi:    features/customers/customer-dialog.tsx, app/(app)/customers/

import { useMutation, useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api/client";
import { useRefreshAfterSave } from "@/lib/api/refresh";
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

export function useCreateCustomer() {
  return useMutation({
    mutationFn: (body: CreateCustomer) => unwrap(api.POST("/api/customers", { body })),
    ...useRefreshAfterSave(customersQueryKey),
  });
}

export function useUpdateCustomer(id: number) {
  return useMutation({
    mutationFn: (body: UpdateCustomer) =>
      unwrap(api.PUT("/api/customers/{id}", { params: { path: { id } }, body })),
    ...useRefreshAfterSave(customersQueryKey),
  });
}

export function useDeleteCustomer() {
  return useMutation({
    mutationFn: (id: number) =>
      unwrap(api.DELETE("/api/customers/{id}", { params: { path: { id } } })),
    ...useRefreshAfterSave(customersQueryKey),
  });
}
