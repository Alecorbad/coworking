"use client";

import { useState } from "react";
import NextLink from "next/link";
import { useParams } from "next/navigation";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import Button from "@mui/material/Button";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { PageHeader } from "@/components/layout/page-header";
import { EmptyState } from "@/components/ui/empty-state";
import { ErrorState } from "@/components/ui/error-state";
import { LoadingState } from "@/components/ui/loading-state";
import { useCustomerBookings } from "@/features/bookings/api";
import { BookingsTable } from "@/features/bookings/bookings-table";
import { useCustomer } from "@/features/customers/api";
import { CustomerDialog } from "@/features/customers/customer-dialog";
import { formatDate } from "@/lib/coworking-time";

/** Un dato del cliente, con l'etichetta sopra il valore. */
function Field({ label, value }: { label: string; value: string }) {
  return (
    <Stack spacing={1}>
      <Typography variant="caption" sx={{ color: "text.secondary" }}>
        {label}
      </Typography>
      <Typography>{value}</Typography>
    </Stack>
  );
}

/** Il dettaglio di un cliente: i suoi dati e tutte le sue prenotazioni, che si possono annullare da qui. */
export default function CustomerPage() {
  const { id } = useParams<{ id: string }>();
  const customer = useCustomer(Number(id));
  const [editing, setEditing] = useState(false);

  const back = (
    <Button component={NextLink} href="/customers" startIcon={<ArrowBackIcon />}>
      Tutti i clienti
    </Button>
  );

  if (customer.isPending) return <LoadingState rows={4} />;
  if (customer.isError)
    return (
      <>
        <PageHeader title="Cliente" tools={back} />
        <ErrorState error={customer.error} onRetry={() => void customer.refetch()} />
      </>
    );

  const c = customer.data;
  return (
    <>
      <PageHeader
        title={`${c.firstName} ${c.lastName}`}
        tools={
          <>
            {back}
            <Button
              variant="outlined"
              startIcon={<EditOutlinedIcon />}
              onClick={() => setEditing(true)}
            >
              Modifica
            </Button>
          </>
        }
      />
      <Stack spacing={6}>
        <Paper sx={{ p: 6 }}>
          <Stack direction="row" spacing={8}>
            <Field label="Email" value={c.email} />
            <Field label="Registrato il" value={formatDate(c.registeredAt)} />
          </Stack>
        </Paper>
        <Stack spacing={3}>
          <Typography variant="h2">Prenotazioni</Typography>
          <CustomerBookings customerId={c.id} />
        </Stack>
      </Stack>
      {editing && <CustomerDialog customer={c} onClose={() => setEditing(false)} />}
    </>
  );
}

/** Le prenotazioni del cliente, dalla più vecchia, comprese le annullate. */
function CustomerBookings({ customerId }: { customerId: number }) {
  const bookings = useCustomerBookings(customerId);

  if (bookings.isPending) return <LoadingState />;
  if (bookings.isError)
    return <ErrorState error={bookings.error} onRetry={() => void bookings.refetch()} />;
  if (bookings.data.length === 0)
    return <EmptyState message="Nessuna prenotazione per questo cliente." />;
  return <BookingsTable bookings={bookings.data} showCustomer={false} />;
}
