import { redirect } from "next/navigation";

/** La radice del sito porta subito alla Disponibilità, la pagina principale. */
export default function HomePage() {
  redirect("/availability");
}
