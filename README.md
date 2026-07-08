# Dotfiles

Personal configuration files managed with GNU Stow on macOS/Linux
and native Windows junctions on Windows.

## Structure

```text
dotfiles/
├── bootstrap/
│   ├── lib/
│   │   ├── helpers.ps1
│   │   └── helpers.sh
│   │
│   ├── install-linux.sh
│   ├── install-macos.sh
│   └── install.ps1
│
├── common/
│   ├── bin/
│   │   └── qobuz-meta
│   │
│   ├── scripts/
│   │   └── flac/
│   │       └── qobuz-meta/
│   │           ├── qobuz-meta
│   │           ├── README.md
│   │           └── requirements.txt
│   │
│   ├── .config/
│   │   ├── btop/
│   │   ├── htop/
│   │   ├── fastfetch/
│   │   ├── neofetch/
│   │   ├── cava/
│   │   ├── spicetify/
│   │   ├── nvim/
│   │   └── starship.toml
│   │
│   ├── .zshrc
│   ├── .zshenv
│   └── .zprofile
│
├── linux/
│   ├── bin/
│   └── .config/
│
├── macos/
│   ├── bin/
│   │   └── wallpaper-switcher
│   │
│   ├── scripts/
│   │   └── wallpaper-switcher/
│   │       ├── wallpaper-switcher
│   │       └── README.md
│   │
│   └── .config/
│       ├── yabai/
│       ├── skhd/
│       │   ├── skhdrc
│       │   └── README.md
│       ├── raycast/
│       └── iterm2/
│
└── windows/
    ├── scripts/
    │   └── komorebi/
    │       ├── start-komorebi.ps1
    │       ├── README.md
    │       └── requirements.txt
    │
    └── .config/
        ├── komorebi/
        │   ├── komorebi.json
        │   ├── applications.json
        │   └── komorebi.bar.json
        │
        └── whkdrc
```

## Overview

- `common` contains shared configurations and utilities
- `linux` contains Linux-specific configurations
- `macos` contains macOS-specific configurations
- `windows` contains Windows-specific configurations
- `bootstrap` contains machine provisioning scripts

---

## Quick Install

### Linux

Supports:

- Ubuntu
- Debian
- Fedora
- Arch Linux

```sh
curl -fsSL \
"https://raw.githubusercontent.com/AvielSkrypnyk/dotfiles/main/bootstrap/"\
"install-linux.sh" \
| bash
```

### macOS

```sh
curl -fsSL \
"https://raw.githubusercontent.com/AvielSkrypnyk/dotfiles/main/bootstrap/"\
"install-macos.sh" \
| bash
```

### Windows

```powershell
irm (
  "https://raw.githubusercontent.com/AvielSkrypnyk/dotfiles/main/bootstrap/" +
  "install.ps1"
)
| iex
```

---

## What Gets Installed

### Shell

- zsh
- oh-my-zsh
- starship
- zsh-autosuggestions
- zsh-syntax-highlighting

### Terminal Utilities

- git
- curl
- wget
- stow
- fastfetch
- btop
- htop

### Fonts

- Hack Nerd Font

### Packages for macOS

- Homebrew
- yabai
- skhd

### Packages for Windows

- PowerShell 7
- Windows Terminal
- komorebi
- whkd

## Git Configuration

Git user information is configured during bootstrap.

If no Git configuration exists, the installer will prompt for:

```text
Git user name
Git email
```

The values are written to the local machine and are not stored in this repository.

---

## Manual Installation

Clone the repository:

```sh
git clone https://github.com/AvielSkrypnyk/dotfiles.git ~/dotfiles
cd ~/dotfiles
```

### Manual macOS setup

```sh
stow common
stow macos
```

### Manual Linux setup

```sh
stow common
stow linux
```

### Manual Windows setup

Run:

```powershell
.\bootstrap\install.ps1
```

---

## PATH

The following directories are automatically added when available:

```text
$HOME/bin
$HOME/.local/bin
$HOME/dotfiles/common/bin
$HOME/dotfiles/macos/bin
$HOME/dotfiles/linux/bin
```

---

## Scripts

Collection of small CLI utilities.

### flac

- [qobuz-meta](common/scripts/flac/qobuz-meta/README.md) -
  processes `.flac` files and embeds metadata

### macOS scripts

- [wallpaper-switcher](macos/scripts/wallpaper-switcher/README.md) -
  random wallpaper setter
- [skhd](macos/.config/skhd/README.md) -
  hotkey daemon driving the yabai window manager

### Windows scripts

- [komorebi](windows/scripts/komorebi/README.md) -
  helper script for komorebi startup manual

---

## Notes

- `bin/` contains executables exposed through PATH
- `scripts/` contains source code and supporting files
- `.config/` mirrors the final configuration layout
- GNU Stow is used on macOS and Linux
- Windows uses junctions and hard links instead of symbolic links
- Starship automatically loads `~/.config/starship.toml`
- New machines can be provisioned using a single bootstrap command
