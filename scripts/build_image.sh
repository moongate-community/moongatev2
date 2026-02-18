#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

IMAGE_TAG="${IMAGE_TAG:-moongate-server:local}"
DOCKERFILE_PATH="${DOCKERFILE_PATH:-$REPO_ROOT/Dockerfile}"
PLATFORM="${PLATFORM:-}"
PUSH_IMAGE="false"
NO_CACHE="false"

usage() {
  cat <<USAGE
Usage: $(basename "$0") [options]

Build Moongate Docker image from repository root Dockerfile.

Options:
  -t, --tag <tag>           Image tag (default: moongate-server:local)
  -f, --file <path>         Dockerfile path (default: ./Dockerfile)
  -p, --platform <platform> Buildx platform (e.g. linux/amd64,linux/arm64)
      --push                Push image after build (uses docker buildx)
      --no-cache            Build without cache
  -h, --help                Show this help
USAGE
}

while [[ $# -gt 0 ]]; do
  case "$1" in
  -t | --tag)
    IMAGE_TAG="$2"
    shift 2
    ;;
  -f | --file)
    DOCKERFILE_PATH="$2"
    shift 2
    ;;
  -p | --platform)
    PLATFORM="$2"
    shift 2
    ;;
  --push)
    PUSH_IMAGE="true"
    shift
    ;;
  --no-cache)
    NO_CACHE="true"
    shift
    ;;
  -h | --help)
    usage
    exit 0
    ;;
  *)
    echo "Unknown option: $1" >&2
    usage
    exit 1
    ;;
  esac
done

if [[ ! -f "$DOCKERFILE_PATH" ]]; then
  echo "Dockerfile not found: $DOCKERFILE_PATH" >&2
  exit 1
fi

BUILD_CMD=(docker buildx build -f "$DOCKERFILE_PATH" -t "$IMAGE_TAG")

if [[ -n "$PLATFORM" ]]; then
  BUILD_CMD+=(--platform "$PLATFORM")
fi

if [[ "$NO_CACHE" == "true" ]]; then
  BUILD_CMD+=(--no-cache)
fi

if [[ "$PUSH_IMAGE" == "true" ]]; then
  BUILD_CMD+=(--push)
else
  BUILD_CMD+=(--load)
fi

BUILD_CMD+=("$REPO_ROOT")

echo "Building image: $IMAGE_TAG"
"${BUILD_CMD[@]}"

echo "Done."
