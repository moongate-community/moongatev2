#!/bin/bash

RELEASE="Debug"
# Detect OS and architecture
UNAME_OS="$(uname -s)"
UNAME_ARCH="$(uname -m)"

# Map architecture
case "$UNAME_ARCH" in
arm64 | aarch64) ARCH="arm64" ;;
x86_64) ARCH="x64" ;;
*)
  echo "Unsupported architecture: $UNAME_ARCH"
  exit 1
  ;;
esac

# Map operating system
case "$UNAME_OS" in
Darwin) RID="osx-$ARCH" ;;
Linux) RID="linux-$ARCH" ;;
MINGW* | MSYS* | CYGWIN*) RID="win-$ARCH" ;;
*)
  echo "Unsupported operating system: $UNAME_OS"
  exit 1
  ;;
esac

# Build and run
# Do not pass PublishAot globally from CLI: it propagates to analyzer projects
# (e.g. netstandard source generators) and can fail with NETSDK1207.
# Explicit RID restore avoids NETSDK1047 (missing net10.0/<rid> target in assets file).
dotnet restore -r "$RID" src/Moongate.Server/Moongate.Server.csproj &&
  dotnet publish -r "$RID" -o dist -c "$RELEASE" src/Moongate.Server/Moongate.Server.csproj --no-restore &&
  ./dist/Moongate.Server "$@" &&
  rm -rf dist
