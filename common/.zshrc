# -----------------------------
# Oh My Zsh
# -----------------------------

export ZSH="$HOME/.oh-my-zsh"

# -----------------------------
# PATH
# -----------------------------

typeset -U path PATH

path=(
  "$HOME/bin"
  "$HOME/.local/bin"
  "$HOME/dotfiles/common/bin"
  $path
)

# macOS
if [[ "$OSTYPE" == darwin* ]]; then
  [[ -d "$HOME/dotfiles/macos/bin" ]] &&
    path=("$HOME/dotfiles/macos/bin" $path)

  [[ -d "/opt/homebrew/bin" ]] &&
    path=("/opt/homebrew/bin" $path)

  [[ -d "/opt/homebrew/sbin" ]] &&
    path=("/opt/homebrew/sbin" $path)

  [[ -d "/opt/local/bin" ]] &&
    path=("/opt/local/bin" $path)

  [[ -d "/opt/local/sbin" ]] &&
    path=("/opt/local/sbin" $path)
fi

# Linux
if [[ "$OSTYPE" == linux* ]]; then
  [[ -d "$HOME/dotfiles/linux/bin" ]] &&
    path=("$HOME/dotfiles/linux/bin" $path)
fi

# Spicetify
[[ -d "$HOME/.spicetify" ]] &&
  path+=("$HOME/.spicetify")

export PATH

# -----------------------------
# Plugins
# -----------------------------

plugins=(
  git
  zsh-autosuggestions
  zsh-syntax-highlighting
)

source "$ZSH/oh-my-zsh.sh"

# -----------------------------
# Starship
# -----------------------------

command -v starship >/dev/null &&
  eval "$(starship init zsh)"

# -----------------------------
# Startup
# -----------------------------

if [[ $- == *i* ]]; then
  command -v fastfetch >/dev/null && fastfetch
fi

# -----------------------------
# Editor
# -----------------------------

export EDITOR="nvim"
export VISUAL="nvim"

# -----------------------------
# Aliases
# -----------------------------

alias vim="nvim"
alias vi="nvim"

alias gs="git status"
alias ga="git add"
alias gc="git commit"
alias gp="git push"