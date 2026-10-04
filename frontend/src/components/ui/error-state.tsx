import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import { errorMessage } from "@/lib/error-messages";

// Stato d'errore: messaggio scelto dal code e "Riprova" quando c'è da riprovare.
export function ErrorState({ error, onRetry }: { error: unknown; onRetry?: () => void }) {
  return (
    <Alert
      severity="error"
      action={
        onRetry && (
          <Button color="inherit" size="small" onClick={onRetry}>
            Riprova
          </Button>
        )
      }
    >
      {errorMessage(error)}
    </Alert>
  );
}
