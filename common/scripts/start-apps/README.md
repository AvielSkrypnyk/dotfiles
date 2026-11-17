# start-apps

Interactive Windows app launcher for starting a chosen set of apps. The list
of apps and their install paths is kept in [`apps.json`](./apps.json); the
launcher is [`start-apps.ps1`](./start-apps.ps1).

## Run it

From PowerShell in this repository:

```powershell
.\common\scripts\start-apps\start-apps.ps1
```

If installed by `bootstrap/install.ps1`, the PowerShell profile also provides
the `start-apps` command:

```powershell
start-apps
```

The script prompts for one of three profiles:

| Choice | Profile | Apps currently configured |
|---|---|---|
| 1 | Basic | Discord (Legcord, or Vesktop if Legcord is not found) |
| 2 | Semi-Code | VS Code, GitKraken |
| 3 | Fully-Code | VS Code, GitKraken, WebStorm, Azure VPN, Postman, Rider |

Apps in `AlwaysApps` start for every profile, before apps in the selected
profile. The current always-on list is Firefox, Teams, Outlook, KeePassXC,
Obsidian, and GitHub Copilot.

The script skips apps whose configured paths cannot be found, showing a
warning, and continues with the remaining apps. When it finishes, press Enter
to close the PowerShell window.

## Start apps at sign-in

During `bootstrap/install.ps1`, answer `y` to the optional
“Open start-apps (choose apps to start) at login?” prompt. This adds a
`Start-apps.lnk` shortcut to the current user's Windows Startup folder. At
sign-in, the launcher opens an interactive PowerShell window so you can select
a profile. It is intentionally not run hidden.

If you answered no during setup, you can run `start-apps` manually or rerun
the installer and enable the option. To disable automatic launch, remove
`Start-apps.lnk` from:

```text
%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup
```

## Configure apps

Edit `apps.json`. Each top-level profile is an object whose property names are
the app names shown in status messages and whose values are executable paths:

```json
{
  "AlwaysApps": {
    "Example App": "%LOCALAPPDATA%\\Programs\\Example App\\Example.exe"
  },
  "BasicApps": {},
  "SemiCodeApps": {},
  "FullyCodeApps": {}
}
```

Use the existing profile names exactly: `AlwaysApps`, `BasicApps`,
`SemiCodeApps`, and `FullyCodeApps`.

### Adding an app

1. Find the app's executable (`.exe`) path on your machine.
2. Choose `AlwaysApps` if it should start for every selection; otherwise
   choose one of the three profile objects.
3. Add a JSON property with a readable app name and its path. Escape each
   backslash as `\\` in JSON.
4. If there are multiple possible install locations, provide an array with
   the preferred path first and fallbacks after it.
5. Save the file and run `start-apps` to test. Confirm the app starts and that
   an unavailable path is skipped as expected.

For example, to add an app to the Basic profile with a fallback location:

```json
"BasicApps": {
  "Example App": [
    "%LOCALAPPDATA%\\Programs\\Example App\\Example.exe",
    "%ProgramFiles%\\Example App\\Example.exe"
  ]
}
```

The first path option that resolves to an existing item is used; later
fallbacks are not checked once a match is found. If no path matches, the app
is skipped with a warning.

## Path behavior

- Environment variables such as `%LOCALAPPDATA%` and `%ProgramFiles%` are
  expanded at launch, so paths are resolved for the current Windows user.
- `*` is supported in paths. For example,
  `%LOCALAPPDATA%\\Programs\\WebStorm*\\bin\\webstorm64.exe` can match an
  installed WebStorm version folder such as `WebStorm 2025.3.2`.
- The path options are checked in order. For a wildcard path that matches
  multiple items, the launcher uses the last item returned by PowerShell's
  `Get-Item`; the script expects this to be the newest matching version.
- The JetBrains entries use this version-folder pattern for WebStorm and
  Rider. Rider currently resolves from
  `%LOCALAPPDATA%\\Programs\\JetBrains Rider*\\bin\\rider64.exe`. If Rider is
  installed in a different location or its folder layout changes, replace or
  add a path option for the actual `rider64.exe` location. Put a preferred
  fixed path before wildcard fallbacks when you want to prioritize it.

Each configured value should point to the app executable, not just its
installation directory. The launcher starts that executable without extra
command-line arguments.