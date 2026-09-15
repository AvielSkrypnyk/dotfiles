#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="$SCRIPT_DIR/bunq-suite/Bunq.Cli/Bunq.Cli.csproj"

OS_NAME="$(uname -s)"
ARCH_NAME="$(uname -m)"

RID="linux-x64"
if [[ "$OS_NAME" == "Darwin" ]]; then
  if [[ "$ARCH_NAME" == "arm64" ]]; then
    RID="osx-arm64"
  else
    RID="osx-x64"
  fi
else
  if [[ "$ARCH_NAME" == "aarch64" ]]; then
    RID="linux-arm64"
  fi
fi

DIST_BIN="$SCRIPT_DIR/dist/$RID/bunq"

if [[ -x "$DIST_BIN" ]]; then
  exec "$DIST_BIN" "$@"
fi

exec dotnet run --project "$PROJECT_PATH" -- "$@"
