#!/usr/bin/env bash

set -euo pipefail

DOTFILES_DIR="${HOME}/dotfiles"
VERSION_FILE="${DOTFILES_DIR}/VERSION"
REMOTE_URL="https://raw.githubusercontent.com/AvielSkrypnyk/dotfiles/main/VERSION"

local_version=$(<"${VERSION_FILE}")

remote_version=$(
    curl -fsSL "$REMOTE_URL" 2>/dev/null || true
)

if [[ -z "$remote_version" ]]; then
    printf '%s (offline)\n' "$local_version"
elif [[ "$local_version" == "$remote_version" ]]; then
    printf '%s ✓\n' "$local_version"
else
    printf '%s → %s\n' "$local_version" "$remote_version"
fi