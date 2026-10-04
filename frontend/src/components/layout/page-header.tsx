import HelpOutlineIcon from "@mui/icons-material/HelpOutlineOutlined";
import IconButton from "@mui/material/IconButton";
import Stack from "@mui/material/Stack";
import Tooltip from "@mui/material/Tooltip";
import Typography from "@mui/material/Typography";
import type { ReactNode } from "react";

// Unica intestazione di pagina: titolo e aiuto a sinistra, strumenti a destra.
export function PageHeader({
  title,
  help,
  tools,
}: {
  title: string;
  help?: string;
  tools?: ReactNode;
}) {
  return (
    <Stack
      direction="row"
      spacing={4}
      sx={{ mb: 6, alignItems: "center", justifyContent: "space-between" }}
    >
      <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
        <Typography variant="h1">{title}</Typography>
        {help && (
          <Tooltip title={help}>
            <IconButton size="small" aria-label={`Aiuto: ${title}`}>
              <HelpOutlineIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        )}
      </Stack>
      {tools && (
        <Stack direction="row" spacing={2}>
          {tools}
        </Stack>
      )}
    </Stack>
  );
}
