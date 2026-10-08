# bunq CLI (C#)

Small CLI for bunq API authentication and local context management.

## TL;DR (Quick Insight)

- Interactive and command-based bunq login helper
- Runs bunq auth handshake: installation -> device registration -> session creation
- Stores context locally (tokens + keys + user id) for reuse
- Works with `menu`, `login`, `context-path`, `reset`
- Gives you the speed of getting the **UserId + SessionToken** of your account in Bunq and later you can use it for something else

Quick start:

```powershell
cd $env:USERPROFILE\dotfiles\common\scripts\bunq
.\bunq.ps1 login --sandbox
```

---

## Run on Windows

```powershell
cd $env:USERPROFILE\dotfiles\common\scripts\bunq
.\bunq.ps1
```

Alternative:

```bat
cd %USERPROFILE%\dotfiles\common\scripts\bunq
bunq.cmd
```

## Run on macOS

```bash
cd ~/dotfiles/common/scripts/bunq
./bunq.sh
```

## Run on Linux

```bash
cd ~/dotfiles/common/scripts/bunq
./bunq.sh
```

## Commands

- `menu` - open interactive menu
- `login` - run full login flow from command line
- `context-path` - print active context JSON path
- `reset` - delete local context JSON

If you run only `bunq` with no args, menu is opened by default.

## Flags (for `login`)

- `--api-key <value>`: API key
- `--sandbox`: use `https://public-api.sandbox.bunq.com/`
- `--production`: use `https://api.bunq.com/`
- `--base-url <url>`: custom API URL (advanced)
- `--description <text>`: device description
- `--permitted-ip <ip>`: permitted IP (repeatable)
- `--force`: force re-installation, device registration, and session recreation

## Common examples

```bash
# sandbox, key from env var
bunq login --sandbox

# explicit key and description
bunq login --api-key YOUR_KEY --description "my-laptop"

# custom IP allow-list
bunq login --sandbox --permitted-ip 1.2.3.4 --permitted-ip 5.6.7.8

# recreate all auth material
bunq login --sandbox --force
```

## Environment variables

- `BUNQ_API_KEY`: fallback API key when `--api-key` is not passed
- `BUNQ_CONTEXT_FILE`: override context JSON location

Example:

```bash
# macOS / Linux
export BUNQ_API_KEY="..."
export BUNQ_CONTEXT_FILE="$HOME/.bunq/context.json"
```

```powershell
# Windows PowerShell
$env:BUNQ_API_KEY="..."
$env:BUNQ_CONTEXT_FILE="$env:USERPROFILE\.bunq\context.json"
```

## Install as a global command (optional)

From `dotfiles/common/scripts/bunq/bunq-suite`:

```bash
dotnet pack Bunq.Cli/Bunq.Cli.csproj -c Release
dotnet tool install --global --add-source ./Bunq.Cli/bin/Release Bunq.Cli
```

Then use:

```bash
bunq menu
```

Update:

```bash
dotnet tool update --global --add-source ./Bunq.Cli/bin/Release Bunq.Cli
```

## Self-contained build (no local runtime required)

From `dotfiles/common/scripts/bunq`:

```bash
# Windows x64
dotnet publish bunq-suite/Bunq.Cli/Bunq.Cli.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o dist/win-x64

# Linux x64
dotnet publish bunq-suite/Bunq.Cli/Bunq.Cli.csproj -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true -o dist/linux-x64

# macOS ARM64
dotnet publish bunq-suite/Bunq.Cli/Bunq.Cli.csproj -c Release -r osx-arm64 --self-contained true /p:PublishSingleFile=true -o dist/osx-arm64
```

Wrappers (`bunq.ps1`, `bunq.cmd`, `bunq.sh`) first try `dist/<rid>/` and fallback to `dotnet run`.

---

## Deep Dive: How this CLI works

This CLI uses bunq API in 3 auth steps.  
Each step depends on data from the previous step.

### Handshake flowchart

```text
[Start]
   |
   v
[Generate RSA key pair (local)]
   |
   v
[POST /v1/installation]
   |-- returns: installation token + server public key
   v
[POST /v1/device-server]
   |-- sends: API key (+ optional permitted IPs)
   |-- returns: device registration accepted
   v
[POST /v1/session-server]
   |-- sends: API key + installation auth header
   |-- returns: session token + user id
   v
[Save context.json]
   |
   v
[Ready for next API features]
```

### Requests this app makes

| Step | Method + endpoint | Main input | Main output | Why needed |
|---|---|---|---|---|
| Installation | `POST /v1/installation` | Local RSA public key | Installation token, server public key | Starts trust/bootstrap with bunq |
| Device registration | `POST /v1/device-server` | API key, description, permitted IPs | Success/fail | Authorizes this device for the API key |
| Session creation | `POST /v1/session-server` | API key (+ installation auth header) | Session token, user id | Opens authenticated session for user operations |

### Headers and payload basics

- This app sends bunq-required headers like request id, geolocation, language, region, user-agent.
- `X-Bunq-Client-Authentication` is set with the **installation token** for device/session steps.
- API key is sent in request body as `secret`.

## Architecture and design choices

### 1) Why split into CLI / Core / Infrastructure?

This is **separation of concerns**:

- CLI handles interaction (input/output)
- Core holds use-case rules and orchestration
- Infrastructure handles external details (HTTP, files, cryptography)

Benefit: you can replace infrastructure details (for example storage backend) without rewriting CLI logic.

### 2) Why use interfaces in Core?

Core depends on abstractions (`IBunqApiService`, `IBunqContextRepository`, `IRsaService`), not concrete classes.

This is **dependency inversion**:

- keeps Core testable
- avoids hard-coupling business logic to one implementation
- allows mocking in tests

### 3) Why keep menu mode and command mode both?

Two user personas:

- beginner user: menu is discoverable and guided
- power user / scripts: commands + flags are automatable

This is a practical **UX + automation** tradeoff.

### 4) Why not fully validate API key at paste time?

Local checks can only validate format, not authenticity.
Real validation requires bunq server call (device registration/session call), because correctness depends on:

- real key value
- chosen environment (sandbox/production)
- allowed IP policy

So the app performs quick local sanity check, then server-side truth check in handshake.

> I chose this behavior so the tool can stop users who type one character and press Enter by mistake.

### 5) Why JSON context file?

The handshake creates state that must persist between runs (installation token, session token, keys, user id).
JSON gives:

- easy debugging (human-readable)
- easy backup/reset
- low operational complexity for local CLI use

> But real reason ( I just like how cool looks JSON ツ )

### 6) Why `BUNQ_CONTEXT_FILE` override?

Different machines and security setups need different locations.
Env override enables:

- custom secure paths
- workspace-local context in CI
- easier testing with temporary files

without code changes.

### 7) Why wrapper scripts instead of only `dotnet run`?

Wrappers provide a stable command (`bunq`) and hide runtime details.
They can run:

1. prebuilt self-contained binary (fast, no runtime dependency), or
2. fallback `dotnet run` (developer-friendly)

This improves developer experience and portability.

### 8) Why OperationResult pattern instead of throwing everywhere?

Returning `OperationResult` for expected operational failures (API errors, invalid state) keeps flow explicit:

- success/failure path is visible in code
- error messages are propagated with context
- avoids broad exception-driven control flow

Exceptions are still used for unexpected runtime failures.

## Project structure

- `Bunq.Cli`: CLI/UI layer (commands, menu, prompts, output helpers)
- `Bunq.Core`: business contracts + models + auth orchestration service
- `Bunq.Infrastructure`: technical implementations (HTTP bunq API, crypto, JSON repository)
- `Bunq.Tests`: NUnit unit tests (AAA style)
