#!/bin/bash

WALLPAPER_DIR="$HOME/Documents/Wallpapers/catpuccin_wallpapers"
WALLPAPER_PATH=$(find "$WALLPAPER_DIR" -type f \( -iname "*.jpg" -o -iname "*.png" -o -iname "*.jpeg" -o -iname "*.heic" \) | sort -R | head -1)
WALLPAPER_NAME=$(basename "$WALLPAPER_PATH")


if [ -z "$WALLPAPER_PATH" ]; then
    osascript -e "display notification \"No wallpapers found!\" with title \"Skhd\""
    exit 1
fi

osascript <<EOF
tell application "System Events"
    tell every desktop
        set picture to "$WALLPAPER_PATH"
    end tell
end tell
display notification "Wallpaper changed to $WALLPAPER_NAME" with title "Skhd"
EOF