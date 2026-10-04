import { PageHeader } from "@/components/layout/page-header";
import { EmptyState } from "@/components/ui/empty-state";

export default function Page() {
  return (
    <>
      <PageHeader title="Sale" help="Le sale riunioni del coworking." />
      <EmptyState message="Nessuna sala ancora: la gestione delle sale arriverà in una fase successiva." />
    </>
  );
}
