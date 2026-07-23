# skhd

Hotkey daemon for macOS using [skhd](https://github.com/koekeishiya/skhd), driving the [yabai](https://github.com/koekeishiya/yabai) tiling window manager.

---

## Overview

`skhdrc` maps keyboard shortcuts to `yabai` commands and app launchers.

It does the following:

- Focuses and moves windows
- Switches desktop layouts (bsp / float)
- Launches applications
- Toggles a gaming mode, lock screen, and wallpaper switcher

---

## Usage

```sh
skhd --start-service
```

Reload the config after editing:

```sh
skhd --reload
```

---

## Example

```sh
# focus window
cmd + ctrl - left  : yabai -m window --focus west

# move window
alt + cmd - left   : yabai -m window --warp west

# change layout
ctrl + alt - z     : yabai -m space --layout bsp

# launch app
cmd + shift - return : open -na "iTerm"
```

---

## Keybindings

| Shortcut              | Action                          |
| --------------------- | ------------------------------- |
| `alt - t`             | Toggle float + center on grid   |
| `ctrl + alt - z`      | Layout: bsp                     |
| `ctrl + alt - x`      | Layout: float                   |
| `alt + cmd - ←↓↑→`    | Move (warp) window              |
| `cmd + ctrl - ←↓↑→`   | Focus window                    |
| `cmd + shift - return`| Open iTerm                      |
| `cmd + shift - f`     | Open Firefox                    |
| `cmd + shift - g`     | Toggle gaming mode              |
| `cmd + shift - l`     | Lock screen                     |
| `cmd + shift - w`     | Random wallpaper                |

> Gaming mode currently only stops and restarts Ubersicht and Notion Calendar.

---

## Setup

Install skhd and yabai:

```sh
brew install koekeishiya/formulae/skhd
brew install koekeishiya/formulae/yabai
```

Start the service:

```sh
skhd --start-service
```

---

## Requirements

- macOS
- `skhd` (on PATH)
- `yabai` (on PATH)
- `wallpaper-switcher` (for the wallpaper binding)

---

## Notes

- `cmd + shift - w` switches the wallpaper for **not all** monitors, it works per space
- Lock screen relies on `Control + Command + Q` because it is more stable than AppleScript-based alternatives, and it requires a password to be set
