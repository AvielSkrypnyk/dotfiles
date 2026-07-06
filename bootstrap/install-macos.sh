#!/usr/bin/env bash

set -e

REPO="https://github.com/AvielSkrypnyk/dotfiles.git"
DOTFILES="$HOME/dotfiles"

# ------------------------------------
# Homebrew
# ------------------------------------

if ! command -v brew >/dev/null 2>&1; then

    /bin/bash -c \
    "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

fi

brew tap homebrew/cask-fonts

# ------------------------------------
# Packages
# ------------------------------------

brew install \
    git \
    stow \
    zsh \
    starship \
    btop \
    fastfetch \
    yabai \
    skhd

brew install --cask \
    font-hack-nerd-font

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
stow macos

# ------------------------------------
# Start Services
# ------------------------------------

brew services start yabai || true
brew services start skhd || true

# ------------------------------------
# Default Shell
# ------------------------------------

chsh -s "$(which zsh)" || true

echo ""
echo "Bootstrap completed."
echo ""
echo "Set iTerm2/Terminal font to:"
echo "Hack Nerd Font Mono"
echo ""
echo "macOS permissions required:"
echo "System Settings > Privacy & Security > Accessibility"
echo "Enable:"
echo "  - yabai"
echo "  - skhd"
echo ""
echo "Reload:"
echo "  yabai --restart-service"
echo "  skhd --restart-service"