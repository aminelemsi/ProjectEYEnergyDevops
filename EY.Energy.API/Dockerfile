# Utiliser l'image officielle .NET SDK pour construire l'application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app



COPY EY.Energy.API/*.csproj ./EY.Energy.API/
COPY EY.Energy.Application/*.csproj ./EY.Energy.Application/
COPY EY.Energy.Infrastructure/*.csproj ./EY.Energy.Infrastructure/
# Restaurer les dépendances du projet principal


RUN dotnet restore EY.Energy.API/EY.Energy.API.csproj

RUN dotnet publish EY.Energy.API/EY.Energy.API.csproj -c Release -o out

# Étape 2 : Construire l'image finale à partir de l'image runtime ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
EXPOSE 80

ENTRYPOINT ["dotnet", "EY.Energy.API.dll"]
