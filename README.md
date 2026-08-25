# Dotfiles

Personal configuration files managed with GNU Stow on macOS/Linux
and native Windows junctions on Windows.

## Structure

<!-- STRUCTURE -->
```text
dotfiles/
├── bootstrap/
│   ├── lib/
│   │   ├── helpers.ps1
│   │   └── helpers.sh
│   ├── install-linux.sh
│   ├── install-macos.sh
│   └── install.ps1
│
├── common/
│   ├── .config/
│   │   ├── btop/
│   │   │   ├── themes/
│   │   │   │   ├── catppuccin_frappe.theme
│   │   │   │   ├── catppuccin_latte.theme
│   │   │   │   ├── catppuccin_macchiato.theme
│   │   │   │   └── catppuccin_mocha.theme
│   │   │   └── btop.conf
│   │   │
│   │   ├── cava/
│   │   │   ├── shaders/
│   │   │   │   ├── bar_spectrum.frag
│   │   │   │   ├── eye_of_phi.frag
│   │   │   │   ├── northern_lights.frag
│   │   │   │   ├── orion_circle.frag
│   │   │   │   ├── orion_circle_rotate.frag
│   │   │   │   ├── orion_saturn_core.frag
│   │   │   │   ├── orion_saturn_subring.frag
│   │   │   │   ├── pass_through.vert
│   │   │   │   ├── spectrogram.frag
│   │   │   │   └── winamp_line_style_spectrum.frag
│   │   │   │
│   │   │   ├── themes/
│   │   │   │   ├── frappe.cava
│   │   │   │   ├── latte.cava
│   │   │   │   ├── macchiato.cava
│   │   │   │   ├── mocha.cava
│   │   │   │   ├── solarized_dark.cava
│   │   │   │   └── tricolor.cava
│   │   │   └── config
│   │   │
│   │   ├── fastfetch/
│   │   │   └── config.jsonc
│   │   │
│   │   ├── htop/
│   │   │   └── htoprc
│   │   │
│   │   ├── neofetch/
│   │   │   └── config.conf
│   │   └── starship.toml
│   │
│   ├── bin/
│   │   └── qobuz-meta
│   │
│   ├── scripts/
│   │   └── flac/
│   │       └── qobuz-meta/
│   │           ├── qobuz-meta
│   │           ├── README.md
│   │           └── requirements.txt
│   ├── .zprofile
│   ├── .zshenv
│   └── .zshrc
│
├── docs/
│   ├── skhd.md
│   └── whkd.md
│
├── linux/
│   └── .config/
│       └── hypr/
│           ├── hyprland.lua
│           ├── hyprland.lua.save
│           ├── monitors.conf
│           └── workspaces.conf
│
├── macos/
│   ├── .config/
│   │   ├── skhd/
│   │   │   └── skhdrc
│   │   │
│   │   └── yabai/
│   │       └── yabairc
│   │
│   ├── bin/
│   │   └── wallpaper-switcher
│   │
│   └── scripts/
│       └── wallpaper-switcher/
│           ├── README.md
│           └── wallpaper-switcher
│
├── unix/
│   └── .config/
│       └── kitty/
│           ├── themes/
│           │   ├── frappe.conf
│           │   ├── latte.conf
│           │   ├── macchiato.conf
│           │   └── mocha.conf
│           └── kitty.conf
│
└── windows/
    ├── .config/
    │   ├── komorebi/
    │   │   ├── applications.json
    │   │   └── komorebi.json
    │   └── whkdrc
    │
    └── scripts/
        └── komorebi/
            ├── README.md
            ├── requirements.txt
            └── start-komorebi.ps1
```
<!-- /STRUCTURE -->

## Overview

- `common` contains shared configurations and utilities
- `unix` contains Linux/macOS shared configurations
- `linux` contains Linux-specific configurations
- `macos` contains macOS-specific configurations
- `windows` contains Windows-specific configurations
- `bootstrap` contains machine provisioning scripts
- `docs` contains reference guides for the configurations

---

## Quick Install

### Linux

Supports:

- Ubuntu
- Debian
- Fedora
- Arch Linux

```sh
curl -fsSL https://raw.githubusercontent.com/AvielSkrypnyk/dotfiles/main/bootstrap/install-linux.sh | bash
```

### macOS

```sh
curl -fsSL https://raw.githubusercontent.com/AvielSkrypnyk/dotfiles/main/bootstrap/install-macos.sh | bash
```

### Windows

```powershell
irm https://raw.githubusercontent.com/AvielSkrypnyk/dotfiles/main/bootstrap/install.ps1 | iex
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

### Linux/macOS Shared Packages

- kitty

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
stow unix
stow macos
```

### Manual Linux setup

```sh
stow common
stow unix
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

### Windows scripts

- [komorebi](windows/scripts/komorebi/README.md) -
  helper script for komorebi startup manual

---

## Configuration Guides

Reference docs for the window management configs, including keybinding tables.

- [skhd](docs/skhd.md) -
  hotkey daemon driving the yabai window manager on macOS
- [whkd](docs/whkd.md) -
  hotkey daemon driving the komorebi window manager on Windows

---

## Notes

- `bin/` contains executables exposed through PATH
- `scripts/` contains source code and supporting files
- `.config/` mirrors the final configuration layout
- GNU Stow is used on macOS and Linux
- Windows uses junctions and hard links instead of symbolic links
- Starship automatically loads `~/.config/starship.toml`
- New machines can be provisioned using a single bootstrap command

---

## Environment

```text
Windows  -> PowerShell + Komorebi
macOS    -> iTerm2 + Yabai + skhd
Linux    -> Kitty

Shared   -> Neovim + Starship
Theme    -> Catppuccin Macchiato (Peach)
```

## Repository

<!-- STATS -->
```text
Commits     : 85
Open PRs    : 1
Last Update : 2026-08-25
```
<!-- /STATS -->
