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
COPY src/Moongate.Network/Moongate.Network.csproj src/Moongate.Network/
COPY src/Moongate.Abstractions/Moongate.Abstractions.csproj src/Moongate.Abstractions/
COPY src/Moongate.Core/Moongate.Core.csproj src/Moongate.Core/
COPY src/Moongate.Network.Packets/Moongate.Network.Packets.csproj src/Moongate.Network.Packets/
COPY src/Moongate.Network.Packets.Generators/Moongate.Network.Packets.Generators.csproj src/Moongate.Network.Packets.Generators/
COPY src/Moongate.Persistence/Moongate.Persistence.csproj src/Moongate.Persistence/
COPY src/Moongate.Scripting/Moongate.Scripting.csproj src/Moongate.Scripting/
COPY src/Moongate.Server.Http/Moongate.Server.Http.csproj src/Moongate.Server.Http/
COPY src/Moongate.UO.Data/Moongate.UO.Data.csproj src/Moongate.UO.Data/
COPY src/Moongate.Server/Moongate.Server.csproj src/Moongate.Server/

RUN dotnet restore src/Moongate.Server/Moongate.Server.csproj

# Copy sources required by Moongate.Server
COPY src/Moongate.Network/ src/Moongate.Network/
COPY src/Moongate.Abstractions/ src/Moongate.Abstractions/
COPY src/Moongate.Core/ src/Moongate.Core/
COPY src/Moongate.Network.Packets/ src/Moongate.Network.Packets/
COPY src/Moongate.Network.Packets.Generators/ src/Moongate.Network.Packets.Generators/
COPY src/Moongate.Persistence/ src/Moongate.Persistence/
COPY src/Moongate.Scripting/ src/Moongate.Scripting/
COPY src/Moongate.Server.Http/ src/Moongate.Server.Http/
COPY src/Moongate.UO.Data/ src/Moongate.UO.Data/
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
      -p:StripSymbols=true \
      -p:DebuggerSupport=false \
      -p:InvariantGlobalization=true

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS final
WORKDIR /opt/moongate

RUN addgroup -S moongate && adduser -S -G moongate -h /app moongate

COPY --from=publish /out/ ./

RUN mkdir -p /app /app/data /app/logs /app/scripts /uo && chown -R moongate:moongate /opt/moongate /app /uo

ENV MOONGATE_ROOT_DIRECTORY=/app
ENV MOONGATE_UO_DIRECTORY=/uo
EXPOSE 2593/tcp

USER moongate
ENTRYPOINT ["/opt/moongate/Moongate.Server"]
