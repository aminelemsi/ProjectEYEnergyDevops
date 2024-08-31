# Étape 1 : Construire l'image à partir du SDK .NET
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copier les fichiers .csproj de chaque projet pour les restaurer individuellement
COPY ["EY.Energy.API/EY.Energy.API.csproj", "EY.Energy.API/"]
COPY ["EY.Energy.Application/EY.Energy.Application.csproj", "EY.Energy.Application/"]
COPY ["EY.Energy.Infrastructure/EY.Energy.Infrastructure.csproj", "EY.Energy.Infrastructure/"]

# Restaurer les dépendances de chaque projet
RUN dotnet restore "EY.Energy.API/EY.Energy.API.csproj"

# Copier tous les fichiers et construire les projets
COPY . .
WORKDIR /src/EY.Energy.API
RUN dotnet build "EY.Energy.API.csproj" -c Release -o /app/build

# Publier l'application
RUN dotnet publish "EY.Energy.API.csproj" -c Release -o /app/publish

# Étape 2 : Configurer l'image runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Spécifier le point d'entrée de l'application
ENTRYPOINT ["dotnet", "EY.Energy.API.dll"]
