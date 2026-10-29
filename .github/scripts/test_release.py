import os
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

from release import determine_release_type
from release_rules import next_version, parse_version, release_type


class ReleaseRulesTests(unittest.TestCase):
    def test_GivenPendingTitles_WhenDetermineReleaseTypeCalled_ThenReturnLargestBump(self) -> None:
        # Arrange
        cases = [
            ([], None),
            (["docs: update setup"], None),
            (["fix: correct startup"], "patch"),
            (["feat: add notifications", "fix: correct startup"], "minor"),
            (["feat!: replace layout", "feat: add notifications"], "major"),
        ]
        for titles, expected in cases:
            with self.subTest(titles=titles):
                # Act
                actual = determine_release_type(titles)

                # Assert
                self.assertEqual(actual, expected)

    def test_GivenInvalidTitleAfterBreakingChange_WhenDetermineReleaseTypeCalled_ThenRaiseValueError(self) -> None:
        # Arrange
        titles = ["feat!: replace layout", "Invalid title"]

        # Act / Assert
        with self.assertRaises(ValueError):
            determine_release_type(titles)

    def test_GivenTitlesWithOptionalScopes_WhenReleaseTypeCalled_ThenReturnExpectedBump(self) -> None:
        # Arrange
        cases = [
            ("fix: correct uptime display", "patch"),
            ("fix(windows): correct uptime display", "patch"),
            ("feat: add update notifications", "minor"),
            ("feat(updates): add update notifications", "minor"),
            ("feat!: change configuration layout", "major"),
            ("feat(config)!: change configuration layout", "major"),
            ("fix(config)!: replace configuration", "major"),
            ("perf: reduce startup time", "patch"),
            ("revert: restore configuration", "patch"),
            ("docs: update setup instructions", None),
            ("chore: update repository stats", None),
            ("chore: sync readme metadata", None),
        ]
        for title, expected in cases:
            with self.subTest(title=title):
                # Act
                actual = release_type(title)

                # Assert
                self.assertEqual(actual, expected)

    def test_GivenInvalidTitle_WhenReleaseTypeCalled_ThenRaiseValueError(self) -> None:
        # Arrange
        invalid_titles = [
            "Update configuration",
            "feat:",
            "fix: ",
            "fix:  extra space",
            "fix: first\nsecond",
            "fix: title\n",
            "fix(): empty scope",
            "fix(config: missing bracket",
            "fix(config)): extra bracket",
            "fix((config)): nested scope",
        ]
        for title in invalid_titles:
            # Act / Assert
            with self.subTest(title=title), self.assertRaises(ValueError):
                release_type(title)

    def test_GivenValidVersion_WhenParseVersionCalled_ThenReturnThreeNumbers(self) -> None:
        # Arrange
        cases = [("v1.2.3\n", (1, 2, 3)), ("0.0.0", (0, 0, 0))]
        for version, expected in cases:
            with self.subTest(version=version):
                # Act
                actual = parse_version(version)

                # Assert
                self.assertEqual(actual, expected)

    def test_GivenInvalidVersion_WhenParseVersionCalled_ThenRaiseValueError(self) -> None:
        # Arrange
        invalid_versions = [
            "1.2", "1.2.3.4", "1..3", "1.x.3", "1.-2.3",
            "01.2.3", "1.02.3", "1.2.03", "1.\u0662.3", "invalid",
        ]
        for version in invalid_versions:
            # Act / Assert
            with self.subTest(version=version), self.assertRaises(ValueError):
                parse_version(version)

    def test_GivenUnknownReleaseType_WhenNextVersionCalled_ThenRaiseValueError(self) -> None:
        # Arrange
        version = "v1.0.0"
        update_type = "unknown"

        # Act / Assert
        with self.assertRaises(ValueError):
            next_version(version, update_type)

    def test_GivenValidVersionAndReleaseType_WhenNextVersionCalled_ThenReturnExpectedVersion(self) -> None:
        # Arrange
        cases = [
            ("v1.0.0\n", "patch", "v1.0.1"),
            ("v1.0.0", "minor", "v1.1.0"),
            ("v1.0.0", "major", "v2.0.0"),
            ("1.2.3", "minor", "v1.3.0"),
            ("v1.2.3", "major", "v2.0.0"),
        ]
        for version, update_type, expected in cases:
            with self.subTest(version=version, update_type=update_type):
                # Act
                actual = next_version(version, update_type)

                # Assert
                self.assertEqual(actual, expected)

    def test_GivenInvalidVersion_WhenNextVersionCalled_ThenRaiseValueError(self) -> None:
        # Arrange
        invalid_versions = ["v01.0.0", "invalid"]
        for version in invalid_versions:
            # Act / Assert
            with self.subTest(version=version), self.assertRaises(ValueError):
                next_version(version, "patch")


class TitleValidationTests(unittest.TestCase):
    def test_GivenPrTitle_WhenValidationScriptCalled_ThenReturnExpectedExitCode(self) -> None:
        # Arrange
        script = Path(__file__).with_name("validate_pr_title.py")
        for title, expected in [
            ("feat: add notifications", 0),
            ("feat(updates): add notifications", 0),
            ("chore: update repository stats", 0),
            ("chore: sync readme metadata", 0),
            ("Update configuration", 1),
            ("fix: first\nsecond", 1)
        ]:
            with self.subTest(title=title):
                environment = os.environ.copy()
                environment["PR_TITLE"] = title

                # Act
                result = subprocess.run(
                    [sys.executable, str(script)],
                    env=environment,
                    capture_output=True,
                    text=True,
                    check=False,
                )

                # Assert
                self.assertEqual(result.returncode, expected, result.stderr)
                if expected:
                    self.assertIn("Use a PR title", result.stderr)
                    self.assertNotIn("Traceback", result.stderr)


class ReleaseIntegrationTests(unittest.TestCase):
    def setUp(self) -> None:
        self.temporary = tempfile.TemporaryDirectory(prefix="dotfiles-release-")
        self.addCleanup(self.temporary.cleanup)
        self.directory = Path(self.temporary.name)
        self.output = self.directory / "output"
        self.git("init")
        self.git("config", "user.name", "Release Test")
        self.git("config", "user.email", "release@example.com")
        self.git("config", "core.autocrlf", "false")
        (self.directory / "VERSION").write_text("v1.0.0\n", encoding="utf-8")
        self.git("add", "VERSION")
        self.commit("chore: initialize version")

    def git(self, *args: str) -> str:
        return subprocess.check_output(
            ["git", *args], cwd=self.directory, text=True, encoding="utf-8"
        ).strip()

    def commit(self, title: str) -> None:
        self.git("-c", "commit.gpgsign=false", "commit", "--allow-empty", "-m", title)

    def prepare(self) -> None:
        environment = os.environ.copy()
        environment["RUNNER_TEMP"] = str(self.directory)
        environment["GITHUB_OUTPUT"] = str(self.output)
        script = Path(__file__).with_name("release.py")
        subprocess.run(
            [sys.executable, str(script)],
            cwd=self.directory,
            env=environment,
            check=True,
        )

    def test_GivenPendingChangesAndChangelog_WhenPrepareReleaseCalled_ThenPreserveHistory(self) -> None:
        # Arrange
        changelog_path = self.directory / "CHANGELOG.md"
        changelog_path.write_text("# Changelog\n\n## v1.0.0\n\n- Initial release\n", encoding="utf-8")
        self.git("add", "CHANGELOG.md")
        self.commit("docs: clarify setup")
        self.commit("fix: correct uptime display")
        self.commit("feat!: change configuration layout")

        # Act
        self.prepare()

        # Assert
        self.assertEqual(self.output.read_text(encoding="utf-8"), "version=v2.0.0\n")
        self.assertEqual((self.directory / "VERSION").read_text(encoding="utf-8"), "v2.0.0\n")
        changelog = changelog_path.read_text(encoding="utf-8")
        self.assertRegex(changelog, r"## v2\.0\.0 \(\d{4}-\d{2}-\d{2}\)")
        self.assertIn(
            "- docs: clarify setup\n- fix: correct uptime display\n"
            "- feat!: change configuration layout\n", changelog
        )
        self.assertIn("## v1.0.0\n\n- Initial release", changelog)
        self.assertEqual(
            (self.directory / "release-notes.md").read_text(encoding="utf-8"),
            "- docs: clarify setup\n- fix: correct uptime display\n"
            "- feat!: change configuration layout\n",
        )

    def test_GivenReleaseCommit_WhenPrepareReleaseCalled_ThenSkipDuplicateRelease(self) -> None:
        # Arrange
        version_path = self.directory / "VERSION"
        version_path.write_text("v2.0.0\n", encoding="utf-8")
        self.git("add", "VERSION")
        self.commit("chore(release): v2.0.0")

        # Act
        self.prepare()

        # Assert
        self.assertFalse(self.output.exists())
        self.assertEqual(version_path.read_text(encoding="utf-8"), "v2.0.0\n")

    def test_GivenMaintenanceCommits_WhenPrepareReleaseCalled_ThenSkipReleaseFiles(self) -> None:
        # Arrange
        version_path = self.directory / "VERSION"
        changelog_path = self.directory / "CHANGELOG.md"
        notes_path = self.directory / "release-notes.md"

        for title in ["chore: update repository stats", "chore: sync readme metadata"]:
            with self.subTest(title=title):
                # Arrange
                self.commit(title)

                # Act
                self.prepare()

                # Assert
                self.assertEqual(version_path.read_text(encoding="utf-8"), "v1.0.0\n")
                self.assertFalse(self.output.exists())
                self.assertFalse(changelog_path.exists())
                self.assertFalse(notes_path.exists())

    def test_GivenFeatureAfterMaintenance_WhenPrepareReleaseCalled_ThenCreateMinorRelease(self) -> None:
        # Arrange
        version_path = self.directory / "VERSION"
        changelog_path = self.directory / "CHANGELOG.md"
        self.commit("chore: update repository stats")
        self.commit("chore: sync readme metadata")
        self.commit("feat: add update notifications")

        # Act
        self.prepare()

        # Assert
        self.assertEqual(version_path.read_text(encoding="utf-8"), "v1.1.0\n")
        self.assertEqual(self.output.read_text(encoding="utf-8"), "version=v1.1.0\n")
        self.assertIn(
            "- feat: add update notifications\n",
            changelog_path.read_text(encoding="utf-8"),
        )

    def test_GivenFixWithoutChangelog_WhenPrepareReleaseCalled_ThenCreatePatchReleaseAndChangelog(self) -> None:
        # Arrange
        self.commit("fix: correct startup")

        # Act
        self.prepare()

        # Assert
        self.assertEqual(self.output.read_text(encoding="utf-8"), "version=v1.0.1\n")
        self.assertIn(
            "- fix: correct startup\n",
            (self.directory / "CHANGELOG.md").read_text(encoding="utf-8"),
        )


if __name__ == "__main__":
    unittest.main()