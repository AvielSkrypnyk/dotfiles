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

show_banner() {
    local subtitle="$1"

    clear
    echo

    # ASCII logo, inline so it always shows (even when helpers are fetched over
    # the network). printf reuses the format for each line arg; %s keeps the
    # backslashes literal. Avoids a heredoc, which doesn't survive eval.
    printf "${COLOR_PEACH}%s${COLOR_RESET}\n" \
        '     _        _      _ _       ____        _       ' \
        '    / \__   _(_) ___| ( )___  |  _ \  ___ | |_ ___ ' \
        '   / _ \ \ / / |/ _ \ |// __| | | | |/ _ \| __/ __|' \
        '  / ___ \ V /| |  __/ | \__ \ | |_| | (_) | |_\__ \' \
        ' /_/   \_\_/ |_|\___|_| |___/ |____/ \___/ \__|___/'

    echo
    printf "${COLOR_BLUE}         %s${COLOR_RESET}\n" "$subtitle"
    printf "${COLOR_TEXT}            by %s${COLOR_RESET}\n" "$AUTHOR_NAME"
    echo
    printf "${COLOR_PEACH}---------------------------------------------------${COLOR_RESET}\n"
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

    local frames=('⠋' '⠙' '⠹' '⠸' '⠼' '⠴' '⠦' '⠧' '⠇' '⠏')
    local n=${#frames[@]}
    local i=0

    # Redraw the same line each tick while the command runs
    while kill -0 "$pid" 2>/dev/null; do
        printf "\r${COLOR_PEACH}%s${COLOR_RESET} %s" "${frames[i]}" "$message"
        i=$(( (i + 1) % n ))
        sleep 0.1
    done

    if wait "$pid"; then
        printf "\r${COLOR_GREEN}✓${COLOR_RESET} %s\n" "$message"
    else
        printf "\r${COLOR_PEACH}✗${COLOR_RESET} %s\n" "$message"
    fi
}

show_done() {
    echo
    printf "${COLOR_PEACH}---------------------------------------------------${COLOR_RESET}\n"
    echo
    printf "${COLOR_GREEN}[DONE]${COLOR_RESET} %s\n" "$1"
    echo
}
