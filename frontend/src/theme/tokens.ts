// Ruolo:   unica fonte dei valori di stile grezzi
// Riceve:  nessuno: sono costanti
// Produce: colori, dimensioni, raggi, spaziatura, durate
// Regola:  per cambiare accento, raggio o densità si modifica una riga qui
// Vedi:    theme/theme.ts

export const colors = {
  // L'accento serve solo ad azione principale, selezione e focus.
  accent: { light: "#1f4bff", dark: "#51a2ff" },
  // Testo scuro sull'accento: contrasto sufficiente sul blu chiaro del tema scuro.
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
  // Stati semantici: mai l'unico segnale, vanno sempre con un'etichetta.
  status: {
    error: { light: "#b42318", dark: "#f97066" },
    warning: { light: "#b54708", dark: "#fdb022" },
    success: { light: "#067647", dark: "#47cd89" },
  },
} as const;

export const fontFamily = '"Rubik", system-ui, sans-serif';

// Cinque dimensioni di testo, in px.
export const fontSize = {
  pageTitle: 20,
  sectionTitle: 16,
  body: 14,
  control: 13,
  label: 12,
} as const;

export const fontWeight = { regular: 400, medium: 500, semibold: 600 } as const;

// Multipli di 4 px: MUI spacing(1) = 4 px, quindi spacing(2) = 8, (3) = 12, (4) = 16, (6) = 24, (8) = 32.
export const spacingUnit = 4;

export const radius = {
  base: 6,
  // Le prenotazioni nella griglia hanno un angolo più stretto.
  booking: 4,
} as const;

export const sizes = {
  control: 32,
  gridRow: 48,
  appBar: 48,
  icon: 18,
  contentMaxWidth: 1200,
} as const;

// Transizioni solo su hover e focus; con prefers-reduced-motion il tema le spegne.
export const motionMs = 150;

// L'unica ombra del progetto: menu e finestre sovrapposte.
export const overlayShadow = "0 8px 24px rgba(0, 0, 0, 0.16)";
