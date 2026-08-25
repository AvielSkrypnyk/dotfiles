# komorebi

CLI config for tiling windows on Windows using [komorebi](https://github.com/LGUG2Z/komorebi).

Keybindings live in [whkd](../../../docs/whkd.md); this script only starts and styles komorebi.

---

## Overview

`start-komorebi.ps1` boots the tiling window manager and applies a Catppuccin-style look.

It does the following:

- Starts the `komorebi` process
- Sets a border width and accent colour (catppuccin-macchiato peach)
- Applies container and workspace padding
- Runs as a single PowerShell script

---

## Usage

```powershell
.\start-komorebi.ps1
```

---

## Example

```powershell
komorebic start
komorebic border-width 5
komorebic border-colour 245 169 127
komorebic container-padding 0 0 5
komorebic workspace-padding 0 0 5
```

---

## Setup

Install komorebi:

```powershell
winget install -e --id LGUG2Z.komorebi
```

---

## Requirements

```powershell
winget install -e --id LGUG2Z.komorebi
```

- Windows  
- `komorebic` (on PATH)  

---

## Notes

- whkd handles keybindings — see [whkd](../../../docs/whkd.md)
- `bootstrap/install.ps1` adds a Startup shortcut so this script runs at login
- If `komorebic` is blocked, corporate policy may prevent tiling