import { createTheme, type Shadows } from "@mui/material/styles";
import { itIT } from "@mui/material/locale";
import {
  colors,
  fontFamily,
  fontSize,
  fontWeight,
  motionMs,
  overlayShadow,
  radius,
  sizes,
  spacingUnit,
} from "./tokens";

// Converte i px in rem, così il testo segue la dimensione scelta dall'utente nel browser.
const rem = (px: number) => `${px / 16}rem`;

// MUI usa l'ombra 8 per menu e popover e la 24 per le finestre: sono le sole accese, le altre sono spente.
const shadows = Array.from({ length: 25 }, (_, i) =>
  i === 8 || i === 24 ? overlayShadow : "none",
) as Shadows;

/**
 * Il tema MUI dell'app, chiaro e scuro, costruito dai valori di `tokens.ts`. Si sceglie da solo in
 * base alla preferenza del sistema operativo. Le pagine non decidono mai lo stile: tutte le
 * personalizzazioni dei componenti MUI stanno qui.
 */
export const theme = createTheme(
  {
    cssVariables: true,
    colorSchemes: {
      light: {
        palette: {
          primary: { main: colors.accent.light, contrastText: colors.onAccent.light },
          error: { main: colors.status.error.light },
          warning: { main: colors.status.warning.light },
          success: { main: colors.status.success.light },
          info: { main: colors.accent.light },
          background: { default: colors.neutral[50], paper: "#ffffff" },
          text: { primary: colors.neutral[800], secondary: colors.neutral[700] },
          divider: colors.neutral[300],
        },
      },
      dark: {
        palette: {
          primary: { main: colors.accent.dark, contrastText: colors.onAccent.dark },
          error: { main: colors.status.error.dark },
          warning: { main: colors.status.warning.dark },
          success: { main: colors.status.success.dark },
          info: { main: colors.accent.dark },
          background: { default: colors.night[950], paper: colors.night[900] },
          text: { primary: colors.neutral[50], secondary: colors.neutral[400] },
          divider: colors.night[700],
        },
      },
    },
    spacing: spacingUnit,
    shape: { borderRadius: radius.base },
    shadows,
    transitions: {
      // Una sola durata: nessuna animazione decorativa.
      duration: {
        shortest: motionMs,
        shorter: motionMs,
        short: motionMs,
        standard: motionMs,
        complex: motionMs,
        enteringScreen: motionMs,
        leavingScreen: motionMs,
      },
    },
    typography: {
      fontFamily,
      fontWeightRegular: fontWeight.regular,
      fontWeightMedium: fontWeight.medium,
      fontWeightBold: fontWeight.semibold,
      // h1: titolo pagina, h2: titolo sezione. Le altre varianti ricadono sulle cinque dimensioni.
      h1: { fontSize: rem(fontSize.pageTitle), fontWeight: fontWeight.semibold, lineHeight: 1.3 },
      h2: {
        fontSize: rem(fontSize.sectionTitle),
        fontWeight: fontWeight.semibold,
        lineHeight: 1.4,
      },
      h3: {
        fontSize: rem(fontSize.sectionTitle),
        fontWeight: fontWeight.semibold,
        lineHeight: 1.4,
      },
      subtitle1: { fontSize: rem(fontSize.body), fontWeight: fontWeight.medium, lineHeight: 1.5 },
      subtitle2: { fontSize: rem(fontSize.label), fontWeight: fontWeight.medium, lineHeight: 1.5 },
      body1: { fontSize: rem(fontSize.body), lineHeight: 1.5 },
      body2: { fontSize: rem(fontSize.body), lineHeight: 1.5 },
      button: {
        fontSize: rem(fontSize.control),
        fontWeight: fontWeight.medium,
        textTransform: "none",
      },
      caption: { fontSize: rem(fontSize.label), lineHeight: 1.5 },
      overline: { fontSize: rem(fontSize.label), textTransform: "none", letterSpacing: 0 },
    },
    components: {
      MuiCssBaseline: {
        styleOverrides: {
          body: { fontVariantNumeric: "tabular-nums" },
          // Il focus da tastiera si vede sempre, con l'accento.
          ":focus-visible": {
            outline: "2px solid var(--mui-palette-primary-main)",
            outlineOffset: 2,
          },
          // Chi chiede meno movimento non ne ha nemmeno quello da 150 ms.
          "@media (prefers-reduced-motion: reduce)": {
            "*, *::before, *::after": {
              transitionDuration: "0.01ms !important",
              animationDuration: "0.01ms !important",
            },
          },
        },
      },
      MuiButtonBase: { defaultProps: { disableRipple: true } },
      MuiButton: {
        defaultProps: { disableElevation: true },
        styleOverrides: {
          root: {
            minHeight: sizes.control,
            padding: "0 12px",
            transition: `background-color ${motionMs}ms, border-color ${motionMs}ms, color ${motionMs}ms`,
          },
          startIcon: { "& > *:first-of-type": { fontSize: sizes.icon } },
        },
      },
      MuiSvgIcon: { styleOverrides: { fontSizeSmall: { fontSize: sizes.icon } } },
      // Bordo da 1 px al posto dell'ombra.
      MuiPaper: {
        styleOverrides: {
          root: ({ theme: t }) => ({
            border: `1px solid ${t.vars.palette.divider}`,
            backgroundImage: "none",
          }),
        },
      },
      MuiAppBar: {
        defaultProps: { color: "default", elevation: 0 },
        styleOverrides: {
          root: ({ theme: t }) => ({
            border: 0,
            borderBottom: `1px solid ${t.vars.palette.divider}`,
            backgroundColor: t.vars.palette.background.paper,
            color: t.vars.palette.text.primary,
          }),
        },
      },
      MuiToolbar: { styleOverrides: { root: { minHeight: `${sizes.appBar}px !important` } } },
      MuiTextField: { defaultProps: { size: "small", variant: "outlined", fullWidth: true } },
      MuiInputBase: {
        styleOverrides: {
          root: { fontSize: rem(fontSize.control), minHeight: sizes.control },
          input: { height: "auto" },
        },
      },
      MuiInputLabel: { styleOverrides: { root: { fontSize: rem(fontSize.control) } } },
      MuiFormHelperText: { styleOverrides: { root: { fontSize: rem(fontSize.label) } } },
      MuiAlert: {
        defaultProps: { variant: "outlined" },
        styleOverrides: { root: { fontSize: rem(fontSize.body), alignItems: "center" } },
      },
      // Nell'app le etichette segnano uno stato «spento», come una sala disattivata: bordo tratteggiato,
      // e il testo dice lo stato, così non si capisce solo dal colore.
      MuiChip: {
        defaultProps: { size: "small", variant: "outlined" },
        styleOverrides: {
          root: { fontSize: rem(fontSize.label) },
          outlined: { borderStyle: "dashed" },
        },
      },
      MuiTableCell: {
        styleOverrides: {
          head: ({ theme: t }) => ({
            fontWeight: fontWeight.medium,
            color: t.vars.palette.text.secondary,
          }),
        },
      },
      MuiDialogTitle: {
        styleOverrides: {
          root: { fontSize: rem(fontSize.sectionTitle), fontWeight: fontWeight.semibold },
        },
      },
      MuiSkeleton: { defaultProps: { animation: false } },
      MuiLink: { defaultProps: { underline: "hover" } },
    },
  },
  itIT,
);
