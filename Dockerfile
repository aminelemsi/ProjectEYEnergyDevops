# Utiliser l'image officielle .NET SDK pour construire l'application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copier les fichiers .csproj et restaurer les dépendances
COPY EY.Energy.API/*.csproj ./EY.Energy.API/
COPY EY.Energy.Application/*.csproj ./EY.Energy.Application/
COPY EY.Energy.Infrastructure/*.csproj ./EY.Energy.Infrastructure/

RUN dotnet restore EY.Energy.API/EY.Energy.API.csproj

# Copier tout le code source et compiler l'application
COPY . ./
RUN dotnet publish EY.Energy.API/EY.Energy.API.csproj -c Release -o out

# Utiliser l'image officielle .NET runtime pour exécuter l'application
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/out .

# Exposer le port 80
EXPOSE 80

# Commande d'exécution de l'application
ENTRYPOINT ["dotnet", "EY.Energy.API.dll"]