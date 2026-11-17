# Zeron.Edition.Sample.Server

Thin **edition host** for `Zeron.Server`. Same Dashboard and APIs as stock Server, plus:

- `INavMenuContribution` / `IAlertRowActionContribution` (see `Zeron.Edition.Sample`)
- Stricter **Server plugin** allow-list (`SampleServerObservePlugin.dll`)
- `PluginsEnabled: true` in local `appsettings.json` (overrides base when merged)

## 3-minute smoke (dev)

```powershell
# Build sample server plugin
dotnet build ..\SampleServerObservePlugin\SampleServerObservePlugin.csproj -c Release
New-Item -ItemType Directory -Force .\plugins | Out-Null
Copy-Item ..\SampleServerObservePlugin\bin\Release\net10.0\SampleServerObservePlugin.dll .\plugins\

# Run edition host (from this project directory after build)
dotnet run -c Release --project Zeron.Edition.Sample.Server.csproj
```

1. Open `http://localhost:5000` — nav includes **Edition sample**.
2. Sign in — **Alerts** open row may show **Edition help** (external link).
3. Log / console: `ZeronServerPluginHost ready` and plugin load line.
4. Agent heartbeat → log line from `SampleServerObservePlugin observed agent.connected`.

Stock OSS host: run `Zeron.Server` instead (`PluginsEnabled` defaults to `false`).

See [Docs/plugins-runbook.md](../../Docs/plugins-runbook.md).