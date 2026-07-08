# wallpaper-switcher

Simple macOS CLI tool to randomly change desktop wallpaper.

---

## Overview

`wallpaper-switcher` selects a random image from a directory and sets it as the wallpaper for all desktops.

---

## Usage

```sh
wallpaper-switcher <wallpaper_directory>
```

If no directory is provided, the default is:

```sh
~/Pictures/Wallpapers
```

---

## Examples

```sh
wallpaper-switcher ~/Documents/Wallpapers
```

```sh
wallpaper-switcher
```

---

## Folder Structure

```text
Wallpapers/
  wallpaper1.jpg
  wallpaper2.png
  wallpaper3.jpeg
```

- All images inside the folder are used  
- Subdirectories are also included  

---

## Requirements

- macOS  
- `osascript` (built-in)  
- `find`  

---

## Notes

- Wallpaper is applied to all desktops (macOS limitation)
- Supported formats: `.jpg`, `.jpeg`, `.png`
- Hidden files are ignored
- If no valid images are found, nothing is changed

---

## Integration (skhd)

Example of mine config:

```sh
cmd + shift - w : wallpaper-switcher ~/Documents/Wallpapers/catppuccin_wallpapers
```