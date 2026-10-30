import os
import subprocess
from pathlib import Path

from release_rules import next_version, release_type


def git(*args: str) -> str:
    command = ["git", *args]
    output = subprocess.check_output(command, text=True, encoding="utf-8")
    return output.strip()


def get_pending_titles() -> list[str]:
    # The last commit that changed VERSION marks the start of unreleased work.
    version_commit = git("log", "-1", "--format=%H", "--", "VERSION")
    if not version_commit:
        raise ValueError("Commit VERSION before enabling releases.")

    # Git's start..end range excludes the starting commit and includes the end.
    commit_range = f"{version_commit}..HEAD"
    titles = git("log", "--reverse", "--format=%s", commit_range).splitlines()

    # A release commit is already released. Any other VERSION commit is the
    # first setup (for example, the PR that added VERSION), so it still counts.
    first_title = git("log", "-1", "--format=%s", version_commit)
    if not first_title.startswith("chore(release):"):
        titles.insert(0, first_title)
    return titles


def determine_release_type(titles: list[str]) -> str | None:
    changes = [release_type(title) for title in titles]
    for change in ["major", "minor", "patch"]:
        if change in changes:
            return change
    return None


def prepare_release() -> None:
    titles = get_pending_titles()
    release_increment = determine_release_type(titles)
    if release_increment is None:
        return

    version_path = Path("VERSION")
    current_version = version_path.read_text(encoding="utf-8")
    version = next_version(current_version, release_increment)

    version_path.write_text(f"{version}\n", encoding="utf-8")
    # Actions reads this file to pass the version to the publish step.
    with Path(os.environ["GITHUB_OUTPUT"]).open("a", encoding="utf-8") as output:
        output.write(f"version={version}\n")


if __name__ == "__main__":
    prepare_release()