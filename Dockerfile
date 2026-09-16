# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy and restore
COPY Cardiac-Monitoring-System/src/CardiacMonitoring.Api/CardiacMonitoring.Api.csproj \
     Cardiac-Monitoring-System/src/CardiacMonitoring.Api/
RUN dotnet restore "Cardiac-Monitoring-System/src/CardiacMonitoring.Api/CardiacMonitoring.Api.csproj"

# Copy everything and build
COPY Cardiac-Monitoring-System/src/CardiacMonitoring.Api/ \
     Cardiac-Monitoring-System/src/CardiacMonitoring.Api/
RUN dotnet publish "Cardiac-Monitoring-System/src/CardiacMonitoring.Api/CardiacMonitoring.Api.csproj" \
    -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway injects PORT environment variable
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CardiacMonitoring.Api.dll"]
