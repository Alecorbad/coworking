import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import type { ReactNode } from "react";

/** Stato vuoto di una pagina: spiega perché non c'è nulla e, se serve, offre l'azione per rimediare. */
export function EmptyState({ message, action }: { message: string; action?: ReactNode }) {
  return (
    <Stack spacing={3} sx={{ py: 8, alignItems: "center" }} role="status">
      <Typography sx={{ color: "text.secondary" }}>{message}</Typography>
      {action}
    </Stack>
  );
}
