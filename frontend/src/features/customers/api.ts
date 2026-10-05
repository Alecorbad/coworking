import { useMutation, useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api/client";
import { useRefreshAfterSave } from "@/lib/api/refresh";
import type { components } from "@/lib/api/schema";
import { unwrap } from "@/lib/api/unwrap";

export type Customer = components["schemas"]["CustomerResponse"];
export type CreateCustomer = components["schemas"]["CreateCustomerRequest"];
export type UpdateCustomer = components["schemas"]["UpdateCustomerRequest"];

/** Chiave comune delle query dei clienti: invalidarla rilegge sia l'elenco sia ogni dettaglio. */
export const customersQueryKey = ["customers"] as const;

/** Tutti i clienti, in ordine di cognome. */
export function useCustomers() {
  return useQuery({
    queryKey: customersQueryKey,
    queryFn: () => unwrap(api.GET("/api/customers")),
  });
}

/** Un cliente, per la pagina di dettaglio. */
export function useCustomer(id: number) {
  return useQuery({
    queryKey: [...customersQueryKey, id],
    queryFn: () => unwrap(api.GET("/api/customers/{id}", { params: { path: { id } } })),
  });
}

/** Registra un cliente; dopo il salvataggio, o un 409, elenco e dettagli si rileggono. */
export function useCreateCustomer() {
  return useMutation({
    mutationFn: (body: CreateCustomer) => unwrap(api.POST("/api/customers", { body })),
    ...useRefreshAfterSave(customersQueryKey),
  });
}

/** Modifica il cliente indicato; poi elenco e dettagli si rileggono. */
export function useUpdateCustomer(id: number) {
  return useMutation({
    mutationFn: (body: UpdateCustomer) =>
      unwrap(api.PUT("/api/customers/{id}", { params: { path: { id } }, body })),
    ...useRefreshAfterSave(customersQueryKey),
  });
}

/** Cancella un cliente; poi elenco e dettagli si rileggono. */
export function useDeleteCustomer() {
  return useMutation({
    mutationFn: (id: number) =>
      unwrap(api.DELETE("/api/customers/{id}", { params: { path: { id } } })),
    ...useRefreshAfterSave(customersQueryKey),
  });
}
