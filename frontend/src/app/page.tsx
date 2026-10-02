"use client";

import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Container from "@mui/material/Container";
import Skeleton from "@mui/material/Skeleton";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { useSettings } from "@/features/settings/api";
import { differsFromCoworkingTimeZone, getBrowserTimeZone } from "@/lib/time-zone";

export default function HomePage() {
  const settings = useSettings();

  return (
    <Container maxWidth="sm" sx={{ py: 6 }}>
      <Stack spacing={2}>
        <Typography variant="h4" component="h1">
          Coworking
        </Typography>

        {settings.isPending && <Skeleton variant="text" width={240} />}

        {settings.isError && (
          <Alert
            severity="error"
            action={
              <Button color="inherit" size="small" onClick={() => settings.refetch()}>
                Riprova
              </Button>
            }
          >
            Il server non risponde.
          </Alert>
        )}

        {settings.isSuccess && (
          <>
            <Typography>Fuso del coworking: {settings.data.timeZone}</Typography>
            {differsFromCoworkingTimeZone(settings.data.timeZone, getBrowserTimeZone()) && (
              <Alert severity="info" role="status">
                Orari mostrati nel fuso del coworking ({settings.data.timeZone}).
              </Alert>
            )}
          </>
        )}
      </Stack>
    </Container>
  );
}
