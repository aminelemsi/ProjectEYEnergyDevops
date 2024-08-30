FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src

# Copy the solution and all project files.
COPY ["EY.Energy.API/EY.Energy.API.csproj", "EY.Energy.API/"]
COPY ["EY.Energy.Application/EY.Energy.Application.csproj", "EY.Energy.Application/"]
COPY ["EY.Energy.Infrastructure/EY.Energy.Infrastructure.csproj", "EY.Energy.Infrastructure/"]
COPY . .

# Restore dependencies
RUN dotnet restore "EY.Energy.API/EY.Energy.API.csproj"

# Build and publish the API project
RUN dotnet publish "EY.Energy.API/EY.Energy.API.csproj" -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "EY.Energy.API.dll"]
