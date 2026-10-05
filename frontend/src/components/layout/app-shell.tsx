"use client";

import { useEffect, type ReactNode } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import AppBar from "@mui/material/AppBar";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Container from "@mui/material/Container";
import Stack from "@mui/material/Stack";
import Toolbar from "@mui/material/Toolbar";
import Tooltip from "@mui/material/Tooltip";
import Typography from "@mui/material/Typography";
import CalendarMonthOutlinedIcon from "@mui/icons-material/CalendarMonthOutlined";
import EventNoteOutlinedIcon from "@mui/icons-material/EventNoteOutlined";
import LogoutIcon from "@mui/icons-material/Logout";
import MeetingRoomOutlinedIcon from "@mui/icons-material/MeetingRoomOutlined";
import PeopleOutlinedIcon from "@mui/icons-material/PeopleOutlined";
import ScheduleOutlinedIcon from "@mui/icons-material/ScheduleOutlined";
import { useLogout, useMe } from "@/features/auth/api";
import { ApiError } from "@/lib/api/api-error";
import { TIME_ZONE, zoneLabel } from "@/lib/coworking-time";
import { sizes } from "@/theme/tokens";
import { ErrorState } from "@/components/ui/error-state";
import { LoadingState } from "@/components/ui/loading-state";

const menu = [
  { href: "/availability", label: "Disponibilità", Icon: CalendarMonthOutlinedIcon },
  { href: "/bookings", label: "Prenotazioni", Icon: EventNoteOutlinedIcon },
  { href: "/customers", label: "Clienti", Icon: PeopleOutlinedIcon },
  { href: "/rooms", label: "Sale", Icon: MeetingRoomOutlinedIcon },
] as const;

/**
 * La struttura di ogni pagina dopo l'accesso: barra con menu, fuso e uscita, poi il contenuto.
 * Prima controlla la sessione: finché non sa chi è collegato mostra il caricamento, e senza
 * sessione porta alla pagina di accesso, senza mai mostrare il contenuto.
 */
export function AppShell({ children }: { children: ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const me = useMe();
  const logout = useLogout();

  const unauthenticated = me.error instanceof ApiError && me.error.status === 401;
  useEffect(() => {
    if (unauthenticated) router.replace("/login");
  }, [unauthenticated, router]);

  const content = () => {
    if (me.isError && !unauthenticated)
      return <ErrorState error={me.error} onRetry={() => me.refetch()} />;
    if (!me.isSuccess) return <LoadingState />;
    return children;
  };

  return (
    <Box sx={{ minHeight: "100vh" }}>
      <AppBar position="static" component="header">
        <Toolbar disableGutters sx={{ px: 4 }}>
          <Typography component="span" variant="subtitle1" sx={{ mr: 6 }}>
            Coworking
          </Typography>
          <Stack
            component="nav"
            aria-label="Principale"
            direction="row"
            spacing={1}
            sx={{ flexGrow: 1 }}
          >
            {menu.map(({ href, label, Icon }) => {
              const current = pathname.startsWith(href);
              return (
                <Button
                  key={href}
                  component={Link}
                  href={href}
                  color={current ? "primary" : "inherit"}
                  aria-current={current ? "page" : undefined}
                  startIcon={<Icon />}
                >
                  {label}
                </Button>
              );
            })}
          </Stack>
          {/* Sempre visibile: ogni orario dell'app è nel fuso del coworking. */}
          <Tooltip
            title={`Tutti gli orari sono nel fuso ${TIME_ZONE}, quello del coworking, qualunque sia il fuso del computer.`}
          >
            <Stack
              direction="row"
              spacing={1}
              sx={{ alignItems: "center", mr: 6, color: "text.secondary" }}
            >
              <ScheduleOutlinedIcon fontSize="small" />
              {/* Pagina prerenderizzata: nel browser vale lo scarto di oggi, anche se diverso da quello della build. */}
              <Typography variant="caption" suppressHydrationWarning>
                {zoneLabel()}
              </Typography>
            </Stack>
          </Tooltip>
          {me.isSuccess && (
            <Stack direction="row" spacing={3} sx={{ alignItems: "center" }}>
              <Typography variant="caption" sx={{ color: "text.secondary" }}>
                {me.data.email}
              </Typography>
              <Button
                color="inherit"
                startIcon={<LogoutIcon />}
                disabled={logout.isPending}
                onClick={() =>
                  logout.mutate(undefined, { onSuccess: () => router.replace("/login") })
                }
              >
                Esci
              </Button>
            </Stack>
          )}
        </Toolbar>
      </AppBar>
      <Container component="main" maxWidth={false} sx={{ maxWidth: sizes.contentMaxWidth, py: 6 }}>
        {content()}
      </Container>
    </Box>
  );
}
