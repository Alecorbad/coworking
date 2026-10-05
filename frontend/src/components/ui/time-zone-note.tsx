import Typography from "@mui/material/Typography";
import { zoneLabel } from "@/lib/coworking-time";

// Dove si scelgono giorno e ora: ricorda che valgono nel fuso del coworking, non in quello del computer.
export function TimeZoneNote() {
  return (
    // Pagina prerenderizzata: nel browser vale lo scarto di oggi, anche se diverso da quello della build.
    <Typography variant="caption" sx={{ color: "text.secondary" }} suppressHydrationWarning>
      {zoneLabel()}: giorno e orari valgono qui, qualunque sia il fuso del computer.
    </Typography>
  );
}
