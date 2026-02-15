# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS publish
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
WORKDIR /src

# NativeAOT prerequisites on Alpine
RUN apk add --no-cache clang build-base zlib-dev

# Copy only project metadata first to maximize restore-layer caching
COPY Directory.Build.props ./
COPY Moongate.slnx ./
COPY Moongate.Network/Moongate.Network.csproj Moongate.Network/
COPY src/Moongate.Core/Moongate.Core.csproj src/Moongate.Core/
COPY src/Moongate.Server/Moongate.Server.csproj src/Moongate.Server/

RUN dotnet restore src/Moongate.Server/Moongate.Server.csproj

# Copy sources required by Moongate.Server
COPY Moongate.Network/ Moongate.Network/
COPY src/Moongate.Core/ src/Moongate.Core/
COPY src/Moongate.Server/ src/Moongate.Server/

# Publish native AOT binary for musl (Alpine)
RUN set -eux; \
    ARCH="${TARGETARCH:-amd64}"; \
    if [ "$ARCH" = "amd64" ]; then ARCH="x64"; fi; \
    if [ "$ARCH" = "arm64" ]; then ARCH="arm64"; fi; \
    dotnet publish src/Moongate.Server/Moongate.Server.csproj \
      -c "$BUILD_CONFIGURATION" \
      -o /out \
      -r "linux-musl-$ARCH" \
      --self-contained true \
      -p:PublishAot=true \
      -p:StripSymbols=true \
      -p:DebuggerSupport=false \
      -p:InvariantGlobalization=true

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS final
WORKDIR /app

RUN addgroup -S moongate && adduser -S -G moongate -h /app moongate

COPY --from=publish /out/ ./

RUN mkdir -p /app/data /app/logs /app/scripts && chown -R moongate:moongate /app

ENV MOONGATE_SERVER_ROOT=/app
EXPOSE 2593/tcp

USER moongate
ENTRYPOINT ["./Moongate.Server"]
