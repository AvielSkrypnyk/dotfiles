RELEASE_TYPES = ["patch", "minor", "major"]
COMMIT_TYPES = [
    "feat", "fix", "perf", "revert", "docs", "style",
    "refactor", "test", "build", "ci", "chore",
]
TITLE_ERROR = (
    'Use a PR title such as "fix: description", '
    '"feat: description", or "feat!: description".'
)


def get_commit_type(header: str) -> str:
    commit_type = header
    if "(" in header:
        commit_type, separator, scope = header.partition("(")
        if not scope.endswith(")"):
            raise ValueError(TITLE_ERROR)

        # In "fix(linux)", the scope is "linux"; empty or nested scopes are invalid.
        scope = scope[:-1]
        if not scope:
            raise ValueError(TITLE_ERROR)
        if "(" in scope or ")" in scope:
            raise ValueError(TITLE_ERROR)

    if commit_type not in COMMIT_TYPES:
        raise ValueError(TITLE_ERROR)
    return commit_type


def release_type(title: str) -> str | None:
    if "\n" in title or "\r" in title:
        raise ValueError(TITLE_ERROR)

    header, separator, description = title.partition(": ")
    if not separator or not description:
        raise ValueError(TITLE_ERROR)
    if description[0].isspace():
        raise ValueError(TITLE_ERROR)

    breaking = header.endswith("!")
    if breaking:
        header = header.removesuffix("!")

    commit_type = get_commit_type(header)
    if breaking:
        return "major"
    if commit_type == "feat":
        return "minor"
    if commit_type in ["fix", "perf", "revert"]:
        return "patch"
    return None


def parse_version(current: str) -> tuple[int, int, int]:
    version = current.strip()
    version = version.removeprefix("v")

    parts = version.split(".")
    if len(parts) != 3:
        raise ValueError("VERSION needs three numbers, for example v1.2.3.")

    for part in parts:
        if not part.isascii():
            raise ValueError("VERSION must use digits from 0 to 9.")
        if not part.isdigit():
            raise ValueError("Each VERSION part must be a whole number.")
        # Zero is valid, but numbers such as 01 are not valid version components.
        if len(part) > 1 and part.startswith("0"):
            raise ValueError("VERSION parts cannot have leading zeros, such as 01.")

    major_text, minor_text, patch_text = parts
    major = int(major_text)
    minor = int(minor_text)
    patch = int(patch_text)
    return major, minor, patch


def next_version(current: str, release_update_type: str) -> str:
    if release_update_type not in RELEASE_TYPES:
        raise ValueError("Unknown release type")

    major, minor, patch = parse_version(current)
    if release_update_type == "major":
        return f"v{major + 1}.0.0"
    if release_update_type == "minor":
        return f"v{major}.{minor + 1}.0"
    return f"v{major}.{minor}.{patch + 1}"