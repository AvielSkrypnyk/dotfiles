#!/usr/bin/env bash
# ------------------------------------
# Shared bootstrap helpers
# Colors, banner and status output.
# Change branding/theme here once.
# ------------------------------------

# Catppuccin Macchiato palette
COLOR_PEACH='\033[38;2;245;169;127m'
COLOR_BLUE='\033[38;2;138;173;244m'
COLOR_GREEN='\033[38;2;166;218;149m'
COLOR_TEXT='\033[38;2;202;211;245m'
COLOR_RESET='\033[0m'

AUTHOR_NAME="Aviel Skrypnyk"

# Directory of this library (banner.txt lives alongside)
HELPERS_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

show_banner() {
    local subtitle="$1"
    local banner_file="$HELPERS_DIR/banner.txt"

    clear
    echo

    if [ -f "$banner_file" ]; then
        while IFS= read -r line; do
            printf "${COLOR_PEACH}%s${COLOR_RESET}\n" "$line"
        done < "$banner_file"
    fi

    echo
    printf "${COLOR_BLUE}         %s${COLOR_RESET}\n" "$subtitle"
    printf "${COLOR_TEXT}            by %s${COLOR_RESET}\n" "$AUTHOR_NAME"
    echo
    printf "${COLOR_PEACH}  ---------------------------------------------------${COLOR_RESET}\n"
    echo
}

show_loading() {
    printf "${COLOR_PEACH}::${COLOR_RESET} %s\n" "$1"
}

# Run a command in the background with an animated spinner.
# Only use for non-interactive steps: output is hidden, so it must
# never wrap something that prompts (e.g. sudo).
run_with_spinner() {
    local message="$1"
    shift

    "$@" >/dev/null 2>&1 &
    local pid=$!

    local frames='|/-\'
    local i=0

    # Redraw the same line each tick while the command runs
    while kill -0 "$pid" 2>/dev/null; do
        i=$(( (i + 1) % 4 ))
        printf "\r${COLOR_PEACH} %s${COLOR_RESET} %s" "${frames:$i:1}" "$message"
        sleep 0.12
    done

    if wait "$pid"; then
        printf "\r${COLOR_GREEN} [OK]${COLOR_RESET} %s\n" "$message"
    else
        printf "\r${COLOR_PEACH} [!!]${COLOR_RESET} %s\n" "$message"
    fi
}

show_done() {
    echo
    printf "${COLOR_PEACH}  ---------------------------------------------------${COLOR_RESET}\n"
    echo
    printf "${COLOR_GREEN}  [DONE]${COLOR_RESET} %s\n" "$1"
    echo
}
