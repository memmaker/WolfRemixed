#!/usr/bin/env bash
# Build the browser version and publish it to https://ruzzoli.de/games/wolf-remixed/play/
set -euo pipefail
cd "$(dirname "$0")"
./build_web.sh
rsync -az --delete /Users/felix/Projects/fx-games/site/wolf-remixed/play/ ruzzoli.de:/var/www/ruzzoli.de/games/wolf-remixed/play/
echo "published: https://ruzzoli.de/games/wolf-remixed/play/"
