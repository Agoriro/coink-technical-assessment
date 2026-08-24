FROM mcr.microsoft.com/dotnet/sdk:10.0.202-alpine3.23@sha256:732cd42c6f659814c9804ad7b05c7f761e83ef8379c5b2fdc3af673353caff73 AS build
WORKDIR /source

ENV NUGET_XMLDOC_MODE=skip

COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/backend/Coink.Api/Coink.Api.csproj src/backend/Coink.Api/
COPY src/backend/Coink.Application/Coink.Application.csproj src/backend/Coink.Application/
COPY src/backend/Coink.Domain/Coink.Domain.csproj src/backend/Coink.Domain/
COPY src/backend/Coink.Infrastructure/Coink.Infrastructure.csproj src/backend/Coink.Infrastructure/

RUN dotnet restore src/backend/Coink.Api/Coink.Api.csproj

COPY src/backend/ src/backend/

RUN dotnet publish src/backend/Coink.Api/Coink.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine@sha256:c4b29bf368004ad9076c1ab9bc91fb373561e3905b4345637e14e8b8c57e3be8 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_EnableDiagnostics=0

COPY --from=build --chown=app:app /app/publish ./

USER app
EXPOSE 8080

HEALTHCHECK --interval=10s --timeout=5s --start-period=20s --retries=5 \
    CMD wget --no-verbose --tries=1 --spider http://127.0.0.1:8080/api/v1/countries || exit 1

ENTRYPOINT ["dotnet", "Coink.Api.dll"]
