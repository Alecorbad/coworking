"use client";

import Alert from "@mui/material/Alert";
import { PageHeader } from "@/components/layout/page-header";
import { EmptyState } from "@/components/ui/empty-state";
import { ErrorState } from "@/components/ui/error-state";
import { LoadingState } from "@/components/ui/loading-state";
import { useSettings } from "@/features/settings/api";
import { differsFromCoworkingTimeZone, getBrowserTimeZone } from "@/lib/time-zone";

export default function AvailabilityPage() {
  const settings = useSettings();

  return (
    <>
      <PageHeader
        title="Disponibilità"
        help="Le sale libere e occupate nell'arco della giornata."
      />
      {settings.isPending && <LoadingState />}
      {settings.isError && <ErrorState error={settings.error} onRetry={() => settings.refetch()} />}
      {settings.isSuccess && (
        <>
          {differsFromCoworkingTimeZone(settings.data.timeZone, getBrowserTimeZone()) && (
            <Alert severity="info" role="status">
              Orari mostrati nel fuso del coworking ({settings.data.timeZone}).
            </Alert>
          )}
          <EmptyState message="La griglia delle disponibilità arriverà con le sale e le prenotazioni." />
        </>
      )}
    </>
  );
}
