#!/usr/bin/env python3
"""Generate the README structure tree block.

The output intentionally inserts spacer lines between siblings to improve
readability in dense trees.
"""

from pathlib import Path

ROOTS = ["bootstrap", "common", "linux", "macos", "windows"]


def sorted_entries(path: Path):
    entries = [p for p in path.iterdir() if p.name != ".git"]
    return sorted(entries, key=lambda p: (not p.is_dir(), p.name.lower(), p.name))


def entry_label(path: Path) -> str:
    return path.name + "/" if path.is_dir() else path.name


def spacer_line(prefix: str) -> str:
    # Keep spacer depth aligned with the current tree prefix.
    return prefix + "│"


def walk(path: Path, prefix: str):
    entries = sorted_entries(path)
    for i, entry in enumerate(entries):
        last = i == len(entries) - 1
        connector = "└── " if last else "├── "
        print(prefix + connector + entry_label(entry))

        if entry.is_dir():
            walk(entry, prefix + ("    " if last else "│   "))

        # Add a spacer line between siblings for readability.
        if not last:
            print(spacer_line(prefix))


def main():
    print("```text")
    print("dotfiles/")

    present_roots = [r for r in ROOTS if Path(r).exists()]
    for i, root_name in enumerate(present_roots):
        root = Path(root_name)
        last_root = i == len(present_roots) - 1
        connector = "└── " if last_root else "├── "
        print(connector + root_name + "/")
        walk(root, "    " if last_root else "│   ")

        # Add a spacer line between top-level roots for readability.
        if not last_root:
            print("│")

    print("```")


if __name__ == "__main__":
    main()
