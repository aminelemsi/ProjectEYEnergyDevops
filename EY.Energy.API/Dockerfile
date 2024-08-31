# Étape 1 : Construire l'image à partir du SDK .NET
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copier l'intégralité du contenu du projet dans le conteneur
COPY . .

# Restaurer les dépendances du projet principal
RUN dotnet restore "EY.Energy.API/EY.Energy.API.csproj"

# Construire et publier le projet principal
RUN dotnet publish "EY.Energy.API/EY.Energy.API.csproj" -c Release -o /app/publish

# Étape 2 : Construire l'image finale à partir de l'image runtime ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "EY.Energy.API.dll"]
