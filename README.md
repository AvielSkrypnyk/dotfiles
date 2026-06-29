# Dotfiles

Personal configuration files managed with GNU Stow.

## Structure

```text
dotfiles/
  common/
    bin/
      qobuz-meta

    scripts/
      flac/
        qobuz-meta/
          qobuz-meta
          README.md
          requirements.txt

    .config/
      btop/
      htop/
      neofetch/
      spicetify/
      cava/
    .zshrc
    .zshenv
    .zprofile
    .gitconfig

  macos/
    bin/
      wallpaper-switcher

    scripts/
      wallpaper-switcher/
        wallpaper-switcher
        README.md

    .config/
      yabai/
      skhd/
      raycast/
      iterm2/

  windows/
    scripts/
      komorebi/
        start-komorebi.bat
        README.md
        requirements.txt
```

- `common` contains cross-platform configs and reusable scripts
- `macos` contains macOS-specific configs and scripts
- `windows` contains Windows-specific configs and scripts

---

## Requirements

- Git
- GNU Stow

---

## Installation

Clone the repository:

```sh
git clone https://github.com/AvielSkrypnyk/dotfiles.git ~/dotfiles
cd ~/dotfiles
```

Apply configurations (macOS / Linux):

```sh
stow common
stow macos    # or: stow linux later
```

> Windows: copy scripts manually — Stow symlinks need Developer Mode or admin.

---

## PATH Setup

Make scripts available globally (macOS / Linux):

```sh
export PATH="$HOME/dotfiles/common/bin:$HOME/dotfiles/macos/bin:$PATH"
# linux bin will be added later: $HOME/dotfiles/linux/bin
```

---

## Scripts

Collection of small CLI utilities.

### flac

- [qobuz-meta](common/scripts/flac/qobuz-meta/README.md) — processes `.flac` files and embeds metadata

### macos

- [wallpaper-switcher](macos/scripts/wallpaper-switcher/README.md) — random wallpaper setter

### windows

- [komorebi](windows/scripts/komorebi/README.md) — tiling window manager config

---

## Notes

- Scripts are exposed via each package's `bin`
- `scripts/` contains source code, `bin/` contains executables
- Structure is designed for scalability and reuse
- Machine-specific data is excluded