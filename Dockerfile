# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copier les fichiers projet
COPY *.sln .
COPY *.csproj .

# Restaurer les dépendances
RUN dotnet restore

# Copier le reste du code
COPY . .

# Publier l'application
RUN dotnet publish *.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update && \
    apt-get install -y \
    libfontconfig1 \
    libfreetype6 \
    libpng16-16 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "comicsbox.dll"]