#!/usr/bin/env bash
# Builds the KNI/Blazor web version and copies it to the site.
# Usage: ./build_web.sh [dest]   (default: ../fx-games/site/wolf-remixed/play)
set -euo pipefail
cd "$(dirname "$0")"
DEST="${1:-../fx-games/site/wolf-remixed/play}"
export PATH="$HOME/.dotnet:$PATH" DOTNET_CLI_TELEMETRY_OPTOUT=1

OUT="$(mktemp -d)"
# build first so the prebuilt .xnb copied into wwwroot/Content are picked up by publish
dotnet build WolfRemixed.Web -c Release
dotnet publish WolfRemixed.Web -c Release -o "$OUT"

rm -rf "$DEST"
mkdir -p "$DEST"
cp -R "$OUT/wwwroot/." "$DEST/"
# nginx serves the .gz via gzip_static; .br is unused
find "$DEST" -name '*.br' -delete
rm -rf "$OUT"
du -sh "$DEST"
