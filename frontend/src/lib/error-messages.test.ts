import { describe, expect, it } from "vitest";
import { ApiError, NETWORK_STATUS, toApiError } from "@/lib/api/api-error";
import { errorMessage } from "./error-messages";

describe("errorMessage", () => {
  it("sceglie il testo dal code, non dallo stato", () => {
    expect(errorMessage(new ApiError(401, "INVALID_CREDENTIALS"))).toBe(
      "Email o password non corrette.",
    );
  });

  it("ripiega sullo stato HTTP quando il code manca o è sconosciuto", () => {
    expect(errorMessage(new ApiError(503))).toContain("sul server");
    expect(errorMessage(new ApiError(404, "SOMETHING_NEW"))).toBe(
      "Qualcosa è andato storto. Riprova.",
    );
    expect(errorMessage(new ApiError(401))).toContain("sessione è scaduta");
  });

  it("distingue il server che non risponde", () => {
    expect(errorMessage(new ApiError(NETWORK_STATUS))).toContain("non risponde");
  });

  it("dà un testo anche per errori che non vengono dall'API", () => {
    expect(errorMessage(new Error("boom"))).toBe("Qualcosa è andato storto. Riprova.");
  });
});

describe("toApiError", () => {
  it("legge code ed errori per campo da un ProblemDetails", () => {
    const error = toApiError(400, {
      status: 400,
      title: "One or more validation errors occurred.",
      code: "VALIDATION_FAILED",
      errors: { Email: ["The Email field is not a valid e-mail address."], Bad: "non è una lista" },
    });

    expect(error.status).toBe(400);
    expect(error.code).toBe("VALIDATION_FAILED");
    expect(error.fieldErrors).toEqual({
      Email: ["The Email field is not a valid e-mail address."],
    });
  });

  it("regge un corpo che non è un ProblemDetails", () => {
    expect(toApiError(502, "<html>Bad gateway</html>")).toMatchObject({
      status: 502,
      code: undefined,
    });
    expect(toApiError(500, undefined).fieldErrors).toEqual({});
  });
});
