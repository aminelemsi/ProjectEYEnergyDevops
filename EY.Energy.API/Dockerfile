# Utiliser l'image officielle .NET SDK pour construire l'application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app 
#
# copy csproj and restore as distinct layers
COPY *.sln .
COPY EY.Energy.API/*.csproj ./EY.Energy.API/
COPY EY.Energy.Application/*.csproj ./EY.Energy.Application/
COPY EY.Energy.Infrastructure/*.csproj ./EY.Energy.Infrastructure/

RUN dotnet restore 
COPY EY.Energy.API/. ./EY.Energy.API/
COPY EY.Energy.Application/. ./EY.Energy.Application/
COPY EY.Energy.Infrastructure/. ./EY.Energy.Application/


WORKDIR /app/EY.Energy.API
RUN dotnet publish  -c Release -o out

# Utiliser l'image officielle .NET runtime pour exécuter l'application
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/EY.Energy/out ./

# Exposer le port 80
EXPOSE 80

# Commande d'exécution de l'application
ENTRYPOINT ["dotnet", "EY.Energy.API.dll"]
