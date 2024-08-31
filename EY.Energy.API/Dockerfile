# Utiliser l'image officielle .NET SDK pour construire l'application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app 

# Copier les fichiers .sln et .csproj
COPY *.sln . 
COPY EY.Energy.API/*.csproj ./EY.Energy.API/
COPY EY.Energy.Application/*.csproj ./EY.Energy.Application/
COPY EY.Energy.Infrastructure/*.csproj ./EY.Energy.Infrastructure/

# Restaurer les dépendances
RUN dotnet restore

# Copier tous les fichiers nécessaires
COPY . .

# Construire et publier le projet
WORKDIR /app/EY.Energy.API
RUN dotnet publish -c Release -o out

# Utiliser l'image officielle .NET runtime pour exécuter l'application
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/EY.Energy.API/out ./

# Exposer le port 80
EXPOSE 80

# Commande d'exécution de l'application
ENTRYPOINT ["dotnet", "EY.Energy.API.dll"]
