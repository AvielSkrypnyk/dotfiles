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

show_banner "macOS dotfiles bootstrap"

show_loading "Installing packages..."

# ------------------------------------
# Homebrew
# ------------------------------------

if ! command -v brew >/dev/null 2>&1; then

  # Install Homebrew, the macOS package manager (unattended)
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
stow macos

# ------------------------------------
# Start Services
# ------------------------------------

# Start the yabai tiling manager and skhd hotkey daemon as services
brew services start yabai || true
brew services start skhd || true

# ------------------------------------
# Default Shell
# ------------------------------------

# Make zsh the default login shell; ignore failure (e.g. non-interactive)
chsh -s "$(which zsh)" || true

show_done "Aviel's Dots are ready."
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
