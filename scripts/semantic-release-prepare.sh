#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 || -z "${1:-}" ]]; then
  echo "Usage: $0 <version>"
  exit 1
fi

VERSION="$1"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

FILES=(
  "Directory.Build.props"
)

while IFS= read -r file; do
  FILES+=("$file")
done < <(find src tests -name "*.csproj" -type f | sort)

for file in "${FILES[@]}"; do
  if [[ ! -f "$file" ]]; then
    continue
  fi

  sed -i.bak -E "s|<Version>[^<]+</Version>|<Version>${VERSION}</Version>|g" "$file"
  rm -f "${file}.bak"
done

echo "Updated project versions to ${VERSION}"
