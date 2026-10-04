"use client";

import { useState } from "react";
import NextLink from "next/link";
import AddIcon from "@mui/icons-material/Add";
import DeleteOutlinedIcon from "@mui/icons-material/DeleteOutlined";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import Button from "@mui/material/Button";
import IconButton from "@mui/material/IconButton";
import Link from "@mui/material/Link";
import Paper from "@mui/material/Paper";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Tooltip from "@mui/material/Tooltip";
import { PageHeader } from "@/components/layout/page-header";
import { ConfirmDialog } from "@/components/ui/confirm-dialog";
import { EmptyState } from "@/components/ui/empty-state";
import { ErrorState } from "@/components/ui/error-state";
import { LoadingState } from "@/components/ui/loading-state";
import { type Customer, useCustomers, useDeleteCustomer } from "@/features/customers/api";
import { CustomerDialog } from "@/features/customers/customer-dialog";
import { useSettings } from "@/features/settings/api";
import { formatDate } from "@/lib/format";

// Finestra aperta: nessuna, nuovo cliente, modifica o cancellazione di un cliente.
type Open =
  | { kind: "create" }
  | { kind: "edit"; customer: Customer }
  | { kind: "delete"; customer: Customer };

export default function CustomersPage() {
  const customers = useCustomers();
  const settings = useSettings();
  const remove = useDeleteCustomer();
  const [open, setOpen] = useState<Open | null>(null);

  const close = () => {
    remove.reset();
    setOpen(null);
  };

  const newCustomer = (
    <Button variant="contained" startIcon={<AddIcon />} onClick={() => setOpen({ kind: "create" })}>
      Nuovo cliente
    </Button>
  );

  return (
    <>
      <PageHeader title="Clienti" help="L'anagrafica di chi prenota le sale." tools={newCustomer} />
      {customers.isPending && <LoadingState />}
      {customers.isError && (
        <ErrorState error={customers.error} onRetry={() => void customers.refetch()} />
      )}
      {customers.isSuccess && customers.data.length === 0 && (
        <EmptyState message="Nessun cliente: registra il primo per poter prenotare." />
      )}
      {customers.isSuccess && customers.data.length > 0 && (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Cliente</TableCell>
                <TableCell>Email</TableCell>
                <TableCell>Registrato il</TableCell>
                <TableCell align="right">Azioni</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {customers.data.map((customer) => {
                const name = `${customer.lastName} ${customer.firstName}`;
                return (
                  <TableRow key={customer.id}>
                    <TableCell>
                      <Link component={NextLink} href={`/customers/${customer.id}`}>
                        {name}
                      </Link>
                    </TableCell>
                    <TableCell>{customer.email}</TableCell>
                    <TableCell>
                      {settings.isSuccess
                        ? formatDate(customer.registeredAt, settings.data.timeZone)
                        : "…"}
                    </TableCell>
                    <TableCell align="right">
                      <Tooltip title="Modifica">
                        <IconButton
                          size="small"
                          aria-label={`Modifica ${name}`}
                          onClick={() => setOpen({ kind: "edit", customer })}
                        >
                          <EditOutlinedIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Elimina">
                        <IconButton
                          size="small"
                          aria-label={`Elimina ${name}`}
                          onClick={() => setOpen({ kind: "delete", customer })}
                        >
                          <DeleteOutlinedIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </TableContainer>
      )}
      {open?.kind === "create" && <CustomerDialog onClose={close} />}
      {open?.kind === "edit" && <CustomerDialog customer={open.customer} onClose={close} />}
      {open?.kind === "delete" && (
        <ConfirmDialog
          title="Eliminare il cliente?"
          message={`${open.customer.firstName} ${open.customer.lastName} sparirà dall'anagrafica.`}
          confirmLabel="Elimina"
          pending={remove.isPending}
          error={remove.error}
          onConfirm={() => remove.mutate(open.customer.id, { onSuccess: close })}
          onClose={close}
        />
      )}
    </>
  );
}
