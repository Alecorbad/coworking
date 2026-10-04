import { PageHeader } from "@/components/layout/page-header";
import { EmptyState } from "@/components/ui/empty-state";

export default function Page() {
  return (
    <>
      <PageHeader title="Clienti" help="L'anagrafica di chi prenota le sale." />
      <EmptyState message="Nessun cliente ancora: l'anagrafica arriverà in una fase successiva." />
    </>
  );
}
