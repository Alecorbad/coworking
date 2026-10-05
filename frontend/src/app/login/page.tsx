"use client";

import type { FormEvent } from "react";
import { useRouter } from "next/navigation";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { useLogin } from "@/features/auth/api";
import { errorMessage } from "@/lib/error-messages";

/** La pagina di accesso: l'unica che si vede senza sessione. Dopo l'accesso porta alla Disponibilità. */
export default function LoginPage() {
  const router = useRouter();
  const login = useLogin();

  // I campi si leggono solo all'invio, e il browser controlla soltanto che siano compilati:
  // le verifiche vere le fa l'API, e il messaggio d'errore arriva da error-messages.
  const onSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    login.mutate(
      { email: String(form.get("email")), password: String(form.get("password")) },
      { onSuccess: () => router.replace("/availability") },
    );
  };

  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        p: 4,
      }}
    >
      <Paper component="main" sx={{ width: "100%", maxWidth: 360, p: 6 }}>
        <Stack component="form" spacing={4} onSubmit={onSubmit}>
          <Typography variant="h1">Accedi</Typography>
          {login.isError && <Alert severity="error">{errorMessage(login.error)}</Alert>}
          <TextField
            name="email"
            label="Email"
            type="email"
            autoComplete="username"
            autoFocus
            required
          />
          <TextField
            name="password"
            label="Password"
            type="password"
            autoComplete="current-password"
            required
          />
          <Button type="submit" variant="contained" disabled={login.isPending}>
            Accedi
          </Button>
        </Stack>
      </Paper>
    </Box>
  );
}
