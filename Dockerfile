# Étape 1 : Construire l'application
# Utilisation d'une image .NET SDK pour la compilation
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copier les fichiers .csproj et restaurer les dépendances
COPY EY.Energy.API/*.csproj ./
RUN dotnet restore

# Copier le reste des fichiers et compiler l'application
COPY . ./
RUN dotnet publish EY.Energy.API.csproj -c Release -o out

# Étape 2 : Créer l'image runtime
# Utilisation d'une image runtime pour l'exécution de l'application
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

# Copier les fichiers compilés depuis l'étape de build
COPY --from=build /app/out .

# Exposer le port sur lequel l'application écoute
EXPOSE 80

# Configurer le point d'entrée pour exécuter l'application
ENTRYPOINT ["dotnet", "EY.Energy.API.dll"]
