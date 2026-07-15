#!/usr/bin/env python3
from pathlib import Path

ROOTS = ["bootstrap", "common", "linux", "macos", "windows"]


def sorted_entries(path: Path):
    entries = [p for p in path.iterdir() if p.name != ".git"]
    return sorted(entries, key=lambda p: (not p.is_dir(), p.name.lower(), p.name))


def walk(path: Path, prefix: str):
    entries = sorted_entries(path)
    for i, entry in enumerate(entries):
        last = i == len(entries) - 1
        connector = "└── " if last else "├── "
        name = entry.name + "/" if entry.is_dir() else entry.name
        print(prefix + connector + name)

        if entry.is_dir():
            walk(entry, prefix + ("    " if last else "│   "))

        # Add a spacer line between siblings for readability.
        if not last:
            print(prefix + "│")


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
