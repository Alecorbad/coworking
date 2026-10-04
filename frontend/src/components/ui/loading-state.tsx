import Skeleton from "@mui/material/Skeleton";
import Stack from "@mui/material/Stack";

// Stato di caricamento: scheletro della forma finale, annunciato come "in caricamento".
export function LoadingState({ rows = 3 }: { rows?: number }) {
  return (
    <Stack spacing={2} role="status" aria-label="Caricamento in corso">
      {Array.from({ length: rows }, (_, i) => (
        <Skeleton key={i} variant="rounded" height={48} />
      ))}
    </Stack>
  );
}
