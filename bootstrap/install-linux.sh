#!/usr/bin/env bash

set -e

REPO="https://github.com/AvielSkrypnyk/dotfiles.git"
DOTFILES="$HOME/dotfiles"

echo "Installing packages..."

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

    sudo dnf check-update || true \
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
    curl -sS https://starship.rs/install.sh | sh -s -- -y
fi

# ------------------------------------
# Hack Nerd Font
# ------------------------------------

mkdir -p "$HOME/.local/share/fonts"

TMP_DIR=$(mktemp -d)

wget -O "$TMP_DIR/Hack.zip" \
https://github.com/ryanoasis/nerd-fonts/releases/latest/download/Hack.zip

unzip -o \
"$TMP_DIR/Hack.zip" \
-d "$HOME/.local/share/fonts"

# ------------------------------------
# Oh My Zsh
# ------------------------------------

if [ ! -d "$HOME/.oh-my-zsh" ]; then

    RUNZSH=no CHSH=no \
    sh -c "$(curl -fsSL https://raw.githubusercontent.com/ohmyzsh/ohmyzsh/master/tools/install.sh)"

fi

# ------------------------------------
# Plugins
# ------------------------------------

ZSH_CUSTOM="${ZSH_CUSTOM:-$HOME/.oh-my-zsh/custom}"

if [ ! -d "$ZSH_CUSTOM/plugins/zsh-autosuggestions" ]; then
    git clone \
    https://github.com/zsh-users/zsh-autosuggestions \
    "$ZSH_CUSTOM/plugins/zsh-autosuggestions"
fi

if [ ! -d "$ZSH_CUSTOM/plugins/zsh-syntax-highlighting" ]; then
    git clone \
    https://github.com/zsh-users/zsh-syntax-highlighting \
    "$ZSH_CUSTOM/plugins/zsh-syntax-highlighting"
fi

# ------------------------------------
# Clone Dotfiles
# ------------------------------------

if [ ! -d "$DOTFILES" ]; then
    git clone "$REPO" "$DOTFILES"
fi

# ------------------------------------
# Git Configuration
# ------------------------------------

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

stow common
stow linux

# ------------------------------------
# Default Shell
# ------------------------------------

chsh -s "$(which zsh)" || true

echo ""
echo "Bootstrap completed."
echo ""
echo "Set terminal font to:"
echo "Hack Nerd Font Mono"