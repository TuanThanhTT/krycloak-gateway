# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY KeycloakGateway.EmailWorker/ ./
RUN dotnet restore KeycloakGateway.EmailWorker.csproj
RUN dotnet publish KeycloakGateway.EmailWorker.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "KeycloakGateway.EmailWorker.dll"]
