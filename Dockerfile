FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore "MoneySaver.Identity/MoneySaver.Identity.csproj" --configfile "./nuget.config"
# Build and publish a release
RUN dotnet publish "MoneySaver.Identity/MoneySaver.Identity.csproj" -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "MoneySaver.Identity.dll"]