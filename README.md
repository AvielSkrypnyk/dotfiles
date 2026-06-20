# Dotfiles

Personal configuration files managed with GNU Stow.

## Structure

```text
dotfiles/
  common/
    bin/
      qobuz-meta
      wallpaper-switcher

    scripts/
      flac/
        qobuz-meta/
          qobuz-meta
          README.md
          requirements.txt

      macos/
        wallpaper-switcher/
          wallpaper-switcher
          README.md

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
    .config/
      yabai/
      skhd/
      raycast/
      iterm2/
```

- `common` contains configs and reusable scripts
- `macos` contains macOS-specific configs

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

Apply configurations:

```sh
stow common
stow macos
```

---

## PATH Setup

Make scripts available globally:

```sh
export PATH="$HOME/dotfiles/common/bin:$PATH"
```

---

## Scripts

Collection of small CLI utilities.

### flac

- [qobuz-meta](common/scripts/flac/qobuz-meta/README.md) — processes `.flac` files and embeds metadata

### macos

- [wallpaper-switcher](common/scripts/macos/wallpaper-switcher/README.md) — random wallpaper setter

---

## Notes

- Scripts are exposed via `common/bin`
- `scripts/` contains source code, `bin/` contains executables
- Structure is designed for scalability and reuse
- Machine-specific data is excluded