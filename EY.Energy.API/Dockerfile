# Étape 1 : Construire l'image à partir du SDK .NET
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY . ./

# Restaurer les dépendances du projet principal

RUN dotnet restore

RUN dotnet publish -c Release -o out

# Étape 2 : Construire l'image finale à partir de l'image runtime ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "EY.Energy.API.dll"]
