FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SedaWears.slnx ./
COPY SedaWears.Presentation/SedaWears.Presentation.csproj   SedaWears.Presentation/
COPY SedaWears.Application/SedaWears.Application.csproj      SedaWears.Application/
COPY SedaWears.Domain/SedaWears.Domain.csproj                SedaWears.Domain/
COPY SedaWears.Infrastructure/SedaWears.Infrastructure.csproj SedaWears.Infrastructure/

RUN dotnet restore

COPY . .

ARG VERSION=0.0.1

RUN dotnet publish SedaWears.Presentation/SedaWears.Presentation.csproj \
  -c Release \
  -o /app/publish \
  --no-restore \
  -p:Version=$VERSION

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --chown=app:app --from=build /app/publish .
COPY --chown=app:app --from=ghcr.io/alexaka1/distroless-dotnet-healthchecks:1 / /healthchecks

USER app

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=3 \
  CMD ["/healthchecks/Distroless.HealthChecks", "--uri", "http://localhost:8080/health"]

ENTRYPOINT ["dotnet", "SedaWears.Presentation.dll"]
