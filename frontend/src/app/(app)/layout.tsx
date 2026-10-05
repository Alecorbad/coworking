import type { ReactNode } from "react";
import { AppShell } from "@/components/layout/app-shell";

/** Le pagine riservate condividono la stessa struttura e il controllo della sessione. */
export default function AppLayout({ children }: { children: ReactNode }) {
  return <AppShell>{children}</AppShell>;
}
