import os
import sys

from release_rules import release_type


EXIT_SUCCESS = 0
EXIT_FAILURE = 1


def main() -> int:
    try:
        release_type(os.environ["PR_TITLE"])
    except ValueError as error:
        print(str(error), file=sys.stderr)
        return EXIT_FAILURE
    return EXIT_SUCCESS


if __name__ == "__main__":
    # GitHub Actions passes the check for exit code 0 and fails it for any other code.
    exit_code = main()
    sys.exit(exit_code)