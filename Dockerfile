FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SedaWears.slnx ./
COPY SedaWears.Presentation/SedaWears.Presentation.csproj   SedaWears.Presentation/
COPY SedaWears.Application/SedaWears.Application.csproj      SedaWears.Application/
COPY SedaWears.Domain/SedaWears.Domain.csproj                SedaWears.Domain/
COPY SedaWears.Infrastructure/SedaWears.Infrastructure.csproj SedaWears.Infrastructure/

RUN dotnet restore

COPY . .

RUN dotnet publish SedaWears.Presentation/SedaWears.Presentation.csproj \
  -c Release \
  -o /app/publish \
  --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

USER root
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

COPY --chown=app:app --from=build /app/publish .

USER app

ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "SedaWears.Presentation.dll"]
