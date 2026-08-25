# whkd

Hotkey daemon for Windows using [whkd](https://github.com/LGUG2Z/whkd), driving the [komorebi](https://github.com/LGUG2Z/komorebi) tiling window manager.

The Windows counterpart to [skhd](skhd.md), and its config format is directly inspired by it.

---

## Overview

`whkdrc` maps keyboard shortcuts to `komorebic` commands and app launchers.

It does the following:

- Focuses and moves windows
- Switches layouts (bsp / toggle tiling)
- Toggles floating windows
- Launches applications
- Locks the screen

Config file: [`windows/.config/whkdrc`](../windows/.config/whkdrc), linked to `~/.config/whkdrc`.

---

## Usage

whkd is started automatically at login by `bootstrap/install.ps1`, which
creates a hidden shortcut in the Startup folder:

```text
%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\whkd.lnk
```

Start it manually:

```powershell
whkd
```

whkd has no reload command, so restart the process after editing the config:

```powershell
Get-Process whkd | Stop-Process
whkd
```

---

## Example

```text
.shell powershell

# toggle float
alt + t : komorebic toggle-float

# change layout
ctrl + alt + z : komorebic change-layout bsp

# move window
alt + win + left : komorebic move left

# focus window
win + ctrl + left : komorebic focus left

# launch app
win + shift + return : wt
```

---

## Keybindings

| Shortcut               | Action                     |
| ---------------------- | -------------------------- |
| `alt + t`              | Toggle float               |
| `ctrl + alt + z`       | Layout: bsp                |
| `ctrl + alt + x`       | Toggle tiling              |
| `alt + win + ←↓↑→`     | Move window                |
| `win + ctrl + ←↓↑→`    | Focus window               |
| `win + shift + return` | Open Windows Terminal      |
| `win + shift + f`      | Open Firefox               |
| `win + shift + l`      | Lock screen                |

---

## Setup

Install komorebi and whkd:

```powershell
winget install -e --id LGUG2Z.komorebi
winget install -e --id LGUG2Z.whkd
```

Both are installed by `bootstrap/install.ps1`.

---

## Requirements

- Windows
- `whkd` (on PATH)
- `komorebic` (on PATH)
- A running `komorebi` instance for the window management bindings

---

## Notes

- `.shell powershell` sets the interpreter for every command; valid values are `cmd`, `pwsh`, and `powershell`
- whkd reads `~/.config/whkdrc` by default, overridable with the `WHKD_CONFIG_HOME` environment variable
- Per-app bindings are supported with `alt + n [ Firefox : ... ]` blocks, which must sit immediately below the `.shell` directive
- A `.pause` directive can bind a hotkey that toggles every other binding on and off
- Lock screen uses `rundll32.exe user32.dll,LockWorkStation` rather than a `komorebic` command
- Shortcuts differ from [skhd](skhd.md) mostly by swapping `cmd` for `win`, since `cmd` has no Windows equivalent
