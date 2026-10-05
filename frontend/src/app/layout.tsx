import type { Metadata } from "next";
import { AppRouterCacheProvider } from "@mui/material-nextjs/v16-appRouter";
import "@fontsource/rubik/400.css";
import "@fontsource/rubik/500.css";
import "@fontsource/rubik/600.css";
import { Providers } from "./providers";

export const metadata: Metadata = {
  title: "Coworking",
  description: "Prenotazione sale riunioni",
};

/** La pagina HTML di base: lingua italiana, carattere Rubik, cache degli stili MUI e Providers. */
export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="it">
      <body>
        <AppRouterCacheProvider>
          <Providers>{children}</Providers>
        </AppRouterCacheProvider>
      </body>
    </html>
  );
}
