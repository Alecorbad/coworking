import { PageHeader } from "@/components/layout/page-header";
import { EmptyState } from "@/components/ui/empty-state";

export default function Page() {
  return (
    <>
      <PageHeader title="Prenotazioni" help="Le prenotazioni delle sale, dalla più vicina." />
      <EmptyState message="Le prenotazioni arriveranno con la griglia delle disponibilità." />
    </>
  );
}
