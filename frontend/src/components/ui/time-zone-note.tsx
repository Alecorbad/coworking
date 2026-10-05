import Typography from "@mui/material/Typography";
import { zoneLabel } from "@/lib/coworking-time";

/**
 * Nota da mettere vicino ai campi di giorno e ora: ricorda che si intendono nell'ora di Roma,
 * anche se il computer è in un altro fuso.
 */
export function TimeZoneNote() {
  return (
    // La pagina è preparata durante la build: nel browser il testo si ricalcola con lo scarto di oggi.
    <Typography variant="caption" sx={{ color: "text.secondary" }} suppressHydrationWarning>
      {zoneLabel()}. Giorno e orari si intendono in quest’ora, anche se il computer è in un altro
      fuso.
    </Typography>
  );
}
