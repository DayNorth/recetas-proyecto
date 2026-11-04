# --- compilación ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY recetas-api.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish recetas-api.csproj -c Release -o /app --no-restore

# --- ejecución ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
# QuestPDF (reporte en PDF) necesita fontconfig en Linux
RUN apt-get update && apt-get install -y --no-install-recommends libfontconfig1 && rm -rf /var/lib/apt/lists/*
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Recetas.Api.dll"]
