# Dotfiles

Personal configuration files managed with GNU Stow.

## Structure

```text
dotfiles/
  common/
    scripts/
      flac/
        qobuz-meta/
          main.py
          requirements.txt
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

- `common` contains configs used across systems
- `macos` contains macOS-specific configs

## Requirements

- Git
- GNU Stow

## Installation

Clone the repository:

```sh
git clone https://github.com/AvielSkrypnyk/dotfiles.git ~/dotfiles
cd ~/dotfiles
```

Apply the configurations:

```sh
stow common
stow macos
```

## Notes

- This setup uses symlinks managed by GNU Stow
- Only manually maintained configurations are tracked
- Machine-specific or generated files are excluded

## Scripts

Collection of small CLI utilities located in `common/scripts`.

### flac

- `qobuz-meta` — processes `.flac` files and embeds metadata

For detailed usage and setup instructions, see:

`common/scripts/flac/qobuz-meta/README.md`