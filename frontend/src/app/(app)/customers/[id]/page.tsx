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
import { useCustomer } from "@/features/customers/api";
import { CustomerDialog } from "@/features/customers/customer-dialog";
import { useSettings } from "@/features/settings/api";
import { formatDate } from "@/lib/format";

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

export default function CustomerPage() {
  const { id } = useParams<{ id: string }>();
  const customer = useCustomer(Number(id));
  const settings = useSettings();
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
            <Field
              label="Registrato il"
              value={settings.isSuccess ? formatDate(c.registeredAt, settings.data.timeZone) : "…"}
            />
          </Stack>
        </Paper>
        <Stack spacing={3}>
          <Typography variant="h2">Prenotazioni</Typography>
          <EmptyState message="Nessuna prenotazione per questo cliente." />
        </Stack>
      </Stack>
      {editing && <CustomerDialog customer={c} onClose={() => setEditing(false)} />}
    </>
  );
}
