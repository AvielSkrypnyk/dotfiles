# qobuz-meta

CLI tool for processing `.flac` music collections and organizing them using metadata.

Works with any FLAC source (Qobuz, downloads, rips, etc.).

---

## Overview

`qobuz-meta` is designed to clean and standardize album-based music collections.

It processes `.flac` files by:

- Embedding album artwork into each track
- Renaming files based on metadata (artist, album, track number, title)
- Copying processed files into an output directory
- Skipping files that already exist in the output

---

## Expected Folder Structure

Each album must be organized in its own directory.

### Example 1

```
Album Name/
  track1.flac
  track2.flac
  track3.flac
  cover.jpg
```

### Example 2

```
Album Name/
  01 - track.flac
  02 - track.flac
  folder.png
```

---

### Requirements

- All tracks must be inside a single album folder
- A cover image must be present (`.jpg` or `.png`)
- Files should already contain basic metadata (artist, album, title)

---

## Usage

```sh
qobuz-meta <input_folder> <output_folder>
```

---

## Example

```sh
qobuz-meta "~/Music/Raw" "~/Music/Processed"
```

---

## Options

```sh
--dry-run
```

Preview changes without modifying files.

---

## Setup

Create a local virtual environment:

```sh
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
```

---

## Requirements

```sh
pip install mutagen tqdm
```

---

## Notes

- Only `.flac` files are processed
- Cover images are embedded automatically into tracks
- If no cover is found, embedding is skipped
- Existing files in the output folder are not overwritten
- Designed for album-based collections (not loose files)

---

## Tips

- Keep album folders clean (no mixed albums)
- Use consistent naming for best results
- Works best with well-tagged FLAC files