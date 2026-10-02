// Unica fonte dei valori di stile grezzi (D-6): tema e componenti leggono da qui,
// le pagine leggono solo il tema.

export const colors = {
  accent: { light: "#1f4bff", dark: "#51a2ff" },
  neutral: {
    50: "#f5f5f5",
    200: "#e5e5e5",
    400: "#a3a3a3",
    700: "#404040",
    800: "#262626",
  },
  night: { 950: "#050b1a", 900: "#0f172a" },
} as const;

export const fontFamily = '"Rubik", system-ui, sans-serif';

export const radius = 8;
