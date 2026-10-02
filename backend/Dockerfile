# Stadio di build: SDK completo, restore dai soli file di progetto per riusare la cache
# di Docker finché le dipendenze non cambiano.
FROM mcr.microsoft.com/dotnet/sdk:10.0.401 AS build
WORKDIR /src
COPY Directory.Build.props ./
COPY src/Coworking.Api/Coworking.Api.csproj src/Coworking.Api/
RUN dotnet restore src/Coworking.Api/Coworking.Api.csproj
COPY src/ src/
# Il documento OpenAPI è già committato in backend/openapi: qui non serve rigenerarlo.
RUN dotnet publish src/Coworking.Api/Coworking.Api.csproj -c Release -o /app --no-restore \
    -p:OpenApiGenerateDocuments=false

# Stadio finale: solo il runtime ASP.NET, variante standard con ICU e tzdata (fusi IANA).
FROM mcr.microsoft.com/dotnet/aspnet:10.0.12
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Coworking.Api.dll"]
