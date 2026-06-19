# Dotfiles

Personal configuration files managed with GNU Stow.

## Structure

```text
dotfiles/
  common/
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

- `common` contains configs used across systems.
- `macos` contains macOS-specific configs.

## Requirements

- Git
- GNU Stow

## Installation

Clone the repository:

```sh
git clone https://github.com/yourusername/dotfiles.git ~/dotfiles
cd ~/dotfiles
```

Apply the configurations:

```sh
stow common
stow macos
```

## Notes

- This setup uses symlinks managed by GNU Stow.
- Only manually maintained configurations are tracked.
- Machine-specific or generated files are excluded.