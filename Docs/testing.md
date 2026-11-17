# Testing Guide

## Running Tests

From the repository root:

```powershell
dotnet test Zeron.sln -c Release
```

Test project: `ZeronTests` (MSTest).

## Test Categories

| Area | Location | Description |
|------|----------|-------------|
| Unit tests | `ZeronTests/ZCore/`, `ZeronTests/ZServers/`, `ZeronTests/Samples/` | Core utilities, auth, task dispatch, alerts, sample gate plugin |
| Server E2E | `ZeronTests/Server/ServerE2ETests.cs` | Full HTTP flow via `WebApplicationFactory` |
| Agent diagnostics | `ZeronTests/ZServers/AgentDiagnosticServerTests.cs` | Connection state logic |

## E2E Integration Tests

E2E tests use `ZeronServerWebApplicationFactory` (`WebApplicationFactory<Program>`) with:

- Environment: `Testing` (skips NetMQ PUB and background workers)
- Isolated SQLite database per factory instance
- In-memory configuration for API keys and admin credentials

### Covered Scenarios

1. **HeartbeatWithoutApiKeyReturnsUnauthorized** — rejects missing agent key
2. **AgentHeartbeatTaskDispatchFlow** — heartbeat → online → create task → pending task → report result
3. **AgentDiagnosticsReturnsHealthyState** — diagnostic API after successful heartbeat
4. **PackageDeployCreatesTask** — catalog + deploy API creates ManagedPackage task
5. **DotNetGateCancelInstallFailsAssignmentE2ETest** — deploy → pending assignment → in-process `gate.install` Cancel → `/api/tasks/results` → assignment leaves heartbeat pending list (no real installer; fake `FilePath`)
6. **DotNetGatePauseThenResumeCompletesAssignmentE2ETest** — same flow with Pause/Resume; benign `cmd.exe /c exit 0` as installer substitute

### Example

```powershell
dotnet test ZeronTests/ZeronTests.csproj --filter "FullyQualifiedName~ServerE2ETests"
dotnet test ZeronTests/ZeronTests.csproj --filter "FullyQualifiedName~DotNetGate"
```

## Demand host (unit)

| Test | Verifies |
|------|----------|
| `DemandCliTests.WantsForegroundHostRecognizesConsoleFlagTest` | `--console` / `-c` foreground flags |
| `DemandCliTests.IsTopshelfServiceCommandTest` | Topshelf `install` verb detection (Windows service path) |

## Server plugin (unit)

| Test | Verifies |
|------|----------|
| `ZeronServerPluginLoaderTests.LoadServerPluginsFindsSampleAssemblyTest` | `LoadServerPlugins` loads sample DLL |

## Plugin policy (unit)

| Test | Verifies |
|------|----------|
| `PluginAssemblyPolicyTests.AllowAllWhenEmptyOrStarTest` | `*` / empty allow-list |
| `PluginAssemblyPolicyTests.AllowListMatchesFileNameOnlyTest` | Explicit DLL names |

## Secret resolution (unit)

| Test | Verifies |
|------|----------|
| `SecureKeyStorageTests.ResolveConfiguredSecretEnvPrefixTest` | `env:VAR` |
| `SecureKeyStorageTests.ResolveConfiguredSecretFilePrefixTest` | `file:` absolute path |
| `ApiKeyValidatorTests.ValidateConfiguredEnvReferenceTest` | NetMQ key validate via `env:` |

CI job `publish-agent-linux` publishes `zeron-agent-linux-x64.zip` (`dotnet publish … -f net10.0 -r linux-x64` on Ubuntu) and runs `scripts/linux-smoke.sh` (boot-only) before zipping.

**CLI pitfall:** `dotnet restore -f` is `--force`, not framework. Use `dotnet publish -f net10.0` (or `msbuild /p:TargetFramework=net10.0`) when restoring a specific TFM.

Manual boot smoke: [deployment.md — Linux agent smoke test](./deployment.md#linux-agent-smoke-test).

## Server bus publish (unit)

| Test | Verifies |
|------|----------|
| `TaskDispatcherServerTests.ReportResultAsyncPublishesAssignmentAndTaskCompletedOnBusTest` | `assignment.completed` + `task.completed` after result report |
| `TaskDispatcherServerTests.DispatchPendingAssignmentsAsyncGateCancelPublishesFailedAssignmentOnBusTest` | Gate dispatch Cancel → `assignment.completed` (`failed`) |
| `AuditLogServerTests.WriteAsyncPublishesAuditWrittenOnBusTest` | `audit.written` after `WriteAsync` |

## EF Core Migrations

Migrations live in `Zeron.Server/Data/Migrations/`.

### Add a migration

```powershell
cd Zeron.Server
dotnet ef migrations add MigrationName --project Zeron.Server.csproj
```

### Apply migrations

Automatic on server startup, or manually:

```powershell
dotnet ef database update --project Zeron.Server.csproj
```

Design-time factory: `ZeronServerDbContextFactory` uses `appsettings.json` for `Zeron:DatabasePath`.

### Upgrading from pre-migration databases

If you previously ran `Zeron.Server` before EF migrations were introduced, the existing SQLite file may have been created with `EnsureCreated()`. On startup you might see:

`SQLite Error 1: 'table "Agents" already exists'.`

The server now detects this legacy state and baselines the database automatically. Restart the server after updating.

If problems persist, back up and remove the database file (default `Data/zeron-server.db`) and restart to create a fresh schema.

## CI

Workflow: [`.github/workflows/ci.yml`](../.github/workflows/ci.yml)

| Job | Purpose |
|-----|---------|
| `build-and-test` | Restore, build, test on `windows-latest` |
| `publish` | Publish `Zeron.Server` + `Zeron.Demand` (win-x64), zip, upload artifacts |

Artifacts (retention 30 days):

- `zeron-server-win-x64` → `zeron-server-win-x64.zip`
- `zeron-agent-win-x64` → `zeron-agent-win-x64.zip`
- `zeron-agent-linux-x64` → `zeron-agent-linux-x64.zip`

Triggers: push/PR to `main`, `master`, or `develop`.

E2E tests do not require external services; each run uses a temporary SQLite file.
