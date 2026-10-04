import MenuItem from "@mui/material/MenuItem";
import TextField, { type TextFieldProps } from "@mui/material/TextField";
import { HALF_HOURS } from "@/lib/coworking-time";

// Menu delle mezz'ore ("08:00", "08:30", ...): gli orari di sale e prenotazioni vanno a passi di
// mezz'ora, come le celle della griglia. Con allowEmpty c'è anche «—», cioè nessun orario.
export function TimeSelect({ allowEmpty, ...props }: TextFieldProps & { allowEmpty?: boolean }) {
  return (
    <TextField select {...props}>
      {allowEmpty && <MenuItem value="">—</MenuItem>}
      {HALF_HOURS.map((slot) => (
        <MenuItem key={slot} value={slot}>
          {slot}
        </MenuItem>
      ))}
    </TextField>
  );
}
