#!/usr/bin/env python3
"""Generate the README structure tree block.

Spacer lines are added only between folder siblings.
"""

from pathlib import Path

ROOTS = ["bootstrap", "common", "linux", "macos", "unix", "windows"]

GIT_DIR_NAME = ".git"
DIR_SUFFIX = "/"

CODE_BLOCK_START = "```text"
CODE_BLOCK_END = "```"
TREE_ROOT_LABEL = "dotfiles/"

BRANCH_MIDDLE = "├── "
BRANCH_LAST = "└── "

PREFIX_CONTINUE = "│   "
PREFIX_BLANK = "    "
SPACER_STEM = "│"


def sorted_entries(path):
    """Return folders first, then files, each group sorted by name."""
    entries = []
    for item in path.iterdir():
        if item.name == GIT_DIR_NAME:
            continue
        entries.append(item)

    return sorted(entries, key=lambda item: (not item.is_dir(), item.name.lower(), item.name))


def format_name(entry):
    if entry.is_dir():
        return entry.name + DIR_SUFFIX
    return entry.name


def is_last_index(index, size):
    return index == size - 1


def connector_for_index(index, size):
    if is_last_index(index, size):
        return BRANCH_LAST
    return BRANCH_MIDDLE


def child_prefix(parent_prefix, entry_is_last):
    if entry_is_last:
        return parent_prefix + PREFIX_BLANK
    return parent_prefix + PREFIX_CONTINUE


def needs_folder_spacer(entries, index):
    """Return True only when current and next siblings are both folders."""
    current = entries[index]
    is_last = is_last_index(index, len(entries))

    if is_last:
        return False

    if not current.is_dir():
        return False

    next_item = entries[index + 1]
    return next_item.is_dir()


def print_tree(path, prefix):
    entries = sorted_entries(path)

    for i, entry in enumerate(entries):
        is_last = is_last_index(i, len(entries))
        connector = connector_for_index(i, len(entries))

        print(prefix + connector + format_name(entry))

        if entry.is_dir():
            next_prefix = child_prefix(prefix, is_last)
            print_tree(entry, next_prefix)

        if needs_folder_spacer(entries, i):
            print(prefix + SPACER_STEM)


def collect_existing_roots(root_names):
    roots = []
    for root_name in root_names:
        root_path = Path(root_name)
        if root_path.exists():
            roots.append(root_path)
    return roots


def root_prefix(is_last_root):
    if is_last_root:
        return PREFIX_BLANK
    return PREFIX_CONTINUE


def main():
    print(CODE_BLOCK_START)
    print(TREE_ROOT_LABEL)

    roots = collect_existing_roots(ROOTS)

    for i, root in enumerate(roots):
        is_last_root = is_last_index(i, len(roots))
        connector = connector_for_index(i, len(roots))

        print(connector + f"{root.name}/")
        print_tree(root, root_prefix(is_last_root))

        if not is_last_root:
            print(SPACER_STEM)

    print(CODE_BLOCK_END)


if __name__ == "__main__":
    main()
