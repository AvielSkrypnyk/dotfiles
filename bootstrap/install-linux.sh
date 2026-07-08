#!/usr/bin/env bash

set -e

REPO="https://github.com/AvielSkrypnyk/dotfiles.git"
DOTFILES="$HOME/dotfiles"

# ------------------------------------
# Shared helpers
# ------------------------------------

# Use the local helpers when cloned; otherwise fetch them over the network
# so the piped "curl | bash" install keeps working.
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" 2>/dev/null && pwd)"

if [ -f "$SCRIPT_DIR/lib/helpers.sh" ]; then
    # shellcheck source=lib/helpers.sh
    . "$SCRIPT_DIR/lib/helpers.sh"
else
    eval "$(curl -fsSL https://raw.githubusercontent.com/AvielSkrypnyk/dotfiles/main/bootstrap/lib/helpers.sh)"
fi

show_banner "Linux dotfiles bootstrap"

show_loading "Installing packages..."

if command -v apt >/dev/null 2>&1; then

    sudo apt update

    sudo apt install -y \
        git \
        curl \
        wget \
        stow \
        zsh \
        btop \
        htop \
        fastfetch \
        unzip

elif command -v dnf >/dev/null 2>&1; then

    sudo dnf install -y \
        git \
        curl \
        wget \
        stow \
        zsh \
        btop \
        htop \
        fastfetch \
        unzip

elif command -v pacman >/dev/null 2>&1; then

    sudo pacman -Sy --noconfirm \
        git \
        curl \
        wget \
        stow \
        zsh \
        btop \
        htop \
        fastfetch \
        unzip

else

    echo "Unsupported Linux distribution"
    exit 1

fi

# ------------------------------------
# Starship
# ------------------------------------

if ! command -v starship >/dev/null 2>&1; then
    # Install the Starship prompt (unattended, auto-confirm)
    curl -sS https://starship.rs/install.sh | sh -s -- -y
fi

# ------------------------------------
# Hack Nerd Font
# ------------------------------------

mkdir -p "$HOME/.local/share/fonts"

# Unique temp dir for the download; extracted fonts go to the user font path
TMP_DIR=$(mktemp -d)

run_with_spinner "Downloading Hack Nerd Font" \
    wget -O "$TMP_DIR/Hack.zip" \
    https://github.com/ryanoasis/nerd-fonts/releases/latest/download/Hack.zip

unzip -o \
"$TMP_DIR/Hack.zip" \
-d "$HOME/.local/share/fonts"

# ------------------------------------
# Oh My Zsh
# ------------------------------------

if [ ! -d "$HOME/.oh-my-zsh" ]; then

    # Install Oh My Zsh, but don't let it switch the shell or relaunch,
    # so this script keeps running to the end
    RUNZSH=no CHSH=no \
    sh -c "$(curl -fsSL https://raw.githubusercontent.com/ohmyzsh/ohmyzsh/master/tools/install.sh)"

fi

# ------------------------------------
# Plugins
# ------------------------------------

# Use an existing ZSH_CUSTOM if set, otherwise fall back to the default path
ZSH_CUSTOM="${ZSH_CUSTOM:-$HOME/.oh-my-zsh/custom}"

if [ ! -d "$ZSH_CUSTOM/plugins/zsh-autosuggestions" ]; then
    run_with_spinner "Installing zsh-autosuggestions" \
    git clone \
    https://github.com/zsh-users/zsh-autosuggestions \
    "$ZSH_CUSTOM/plugins/zsh-autosuggestions"
fi

if [ ! -d "$ZSH_CUSTOM/plugins/zsh-syntax-highlighting" ]; then
    run_with_spinner "Installing zsh-syntax-highlighting" \
    git clone \
    https://github.com/zsh-users/zsh-syntax-highlighting \
    "$ZSH_CUSTOM/plugins/zsh-syntax-highlighting"
fi

# ------------------------------------
# Clone Dotfiles (always fresh)
# ------------------------------------

# Remove any existing copy so every run starts from a clean clone
if [ -d "$DOTFILES" ]; then
    show_loading "Existing $DOTFILES found, removing for a fresh clone"
    cd "$HOME"
    rm -rf "$DOTFILES"
fi

run_with_spinner "Cloning dotfiles" git clone --depth=1 "$REPO" "$DOTFILES"

# ------------------------------------
# Git Configuration
# ------------------------------------

# Only prompt for git name/email if they aren't configured yet
GIT_NAME="$(git config --global user.name 2>/dev/null || true)"
GIT_EMAIL="$(git config --global user.email 2>/dev/null || true)"

if [ -z "$GIT_NAME" ] || [ -z "$GIT_EMAIL" ]; then

    echo ""
    echo "Git configuration"

    read -rp "Git user name: " GIT_NAME
    read -rp "Git email: " GIT_EMAIL

    git config --global user.name "$GIT_NAME"
    git config --global user.email "$GIT_EMAIL"
    git config --global init.defaultBranch main

    echo "Git configured."

else

    echo "Git already configured."

fi

# ------------------------------------
# Apply Configs
# ------------------------------------

cd "$DOTFILES"

# Oh My Zsh and zsh create default dotfiles that would block stow; back up
# any real (non-symlink) copies so stow can link ours in
for file in .zshrc .zshenv .zprofile; do
    target="$HOME/$file"
    if [ -e "$target" ] && [ ! -L "$target" ]; then
        mv "$target" "$target.bak"
    fi
done

stow common
stow linux

# ------------------------------------
# Default Shell
# ------------------------------------

# Set zsh as the default login shell; ignore failure (e.g. non-interactive)
chsh -s "$(which zsh)" || true

show_done "Aviel's Dots are ready."
echo "Set terminal font to:"
echo "Hack Nerd Font Mono"