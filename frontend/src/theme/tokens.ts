/**
 * I valori di stile grezzi, in un posto solo: colori, testo, spaziatura, raggi, dimensioni.
 * Per cambiare accento, raggio o densità di tutta l'app basta modificare una riga qui;
 * `theme.ts` li traduce nel tema di MUI.
 */

/** La palette, con la variante chiara e quella scura di ogni colore. */
export const colors = {
  // L'accento si usa solo per l'azione principale, la selezione e il focus.
  accent: { light: "#1f4bff", dark: "#51a2ff" },
  // Nel tema scuro il testo sull'accento è scuro: sul blu chiaro si legge meglio.
  onAccent: { light: "#ffffff", dark: "#050b1a" },
  neutral: {
    50: "#f5f5f5",
    100: "#ececec",
    200: "#e5e5e5",
    300: "#d4d4d4",
    400: "#a3a3a3",
    500: "#737373",
    700: "#404040",
    800: "#262626",
  },
  night: { 950: "#050b1a", 900: "#0f172a", 800: "#1b2438", 700: "#2a3550" },
  // Colori di errore, avviso e successo: mai da soli, sempre accanto a un'etichetta.
  status: {
    error: { light: "#b42318", dark: "#f97066" },
    warning: { light: "#b54708", dark: "#fdb022" },
    success: { light: "#067647", dark: "#47cd89" },
  },
} as const;

/** Il carattere dell'app, con il ripiego di sistema mentre si carica. */
export const fontFamily = '"Rubik", system-ui, sans-serif';

/** Le cinque dimensioni di testo dell'app, in px. */
export const fontSize = {
  pageTitle: 20,
  sectionTitle: 16,
  body: 14,
  control: 13,
  label: 12,
} as const;

/** I tre pesi del carattere usati nell'app. */
export const fontWeight = { regular: 400, medium: 500, semibold: 600 } as const;

/**
 * L'unità di spaziatura: in MUI `spacing(1)` vale 4 px, quindi 2 = 8, 3 = 12, 4 = 16, 6 = 24, 8 = 32.
 * Tutti gli spazi dell'app sono multipli di 4.
 */
export const spacingUnit = 4;

/** Il raggio degli angoli: uno per tutto, uno più stretto per le prenotazioni nella griglia. */
export const radius = {
  base: 6,
  booking: 4,
} as const;

/** Altezze e larghezze fisse, in px: controlli, righe della griglia, barra, icone, contenuto. */
export const sizes = {
  control: 32,
  gridRow: 48,
  appBar: 48,
  icon: 18,
  contentMaxWidth: 1200,
} as const;

/** Durata delle transizioni, solo su hover e focus. Chi chiede meno movimento al sistema non le vede. */
export const motionMs = 150;

/** L'unica ombra dell'app, per menu e finestre che stanno sopra la pagina. */
export const overlayShadow = "0 8px 24px rgba(0, 0, 0, 0.16)";
