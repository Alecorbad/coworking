import { createTheme } from "@mui/material/styles";
import { itIT } from "@mui/material/locale";
import { colors, fontFamily, radius } from "./tokens";

// Tema chiaro e scuro dal primo giorno (D-6); variabili CSS e selezione
// automatica secondo la preferenza del sistema operativo.
export const theme = createTheme(
  {
    cssVariables: true,
    colorSchemes: {
      light: {
        palette: {
          primary: { main: colors.accent.light },
          background: { default: colors.neutral[50], paper: "#ffffff" },
          text: { primary: colors.neutral[800], secondary: colors.neutral[700] },
          divider: colors.neutral[200],
        },
      },
      dark: {
        palette: {
          primary: { main: colors.accent.dark },
          background: { default: colors.night[950], paper: colors.night[900] },
          text: { primary: colors.neutral[50], secondary: colors.neutral[400] },
        },
      },
    },
    typography: { fontFamily },
    shape: { borderRadius: radius },
  },
  itIT,
);
