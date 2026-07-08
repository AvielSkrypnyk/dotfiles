# komorebi

CLI config for tiling windows on Windows using [komorebi](https://github.com/LGUG2Z/komorebi).

Works without a keybinding daemon (no whkd / AHK).

---

## Overview

`start-komorebi.bat` boots the tiling window manager and applies a Catppuccin-style look.

It does the following:

- Starts the `komorebi` process
- Sets a border width and accent colour (catppuccin-macchiato peach)
- Applies container and workspace padding
- Runs as a single batch file

---

## Usage

```bat
start-komorebi.bat
```

---

## Example

```bat
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

- whkd is **not** used — start manually or via Task Scheduler
- If `komorebic` is blocked, corporate policy may prevent tiling