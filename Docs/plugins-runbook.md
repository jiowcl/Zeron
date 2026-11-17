# .NET plugin operations runbook

Operational guide for **Server** (`IZeronServerPlugin`) and **Agent** (`IZeronAgentPlugin`) in-process plugins. Scripts remain observe-only; see [Script Host](./script-host.md).

Related: [Editions](./editions.md), [Event Bus](./event-bus.md), [Configuration](./configuration.md).

---

## Quick reference

| Role | Directory | Enable | Allow-list | Sample DLL |
|------|-----------|--------|------------|------------|
| **Server** | `plugins/` next to Server binary | `Zeron:PluginsEnabled` | `Zeron:PluginsAllowList` | `SampleServerObservePlugin.dll` |
| **Agent** | `script_plugins_dir` (default `plugins/`) | always scans when gate boots | `script_plugins_allowlist` in `App.config` | `SampleAgentGatePlugin.dll` |

**Never loaded by design:** `Zeron.*`, `System.*`, `Microsoft.*`, `netstandard*` file names.

---

## Server plugins

### Enable (production-shaped)

1. Build [SampleServerObservePlugin](../Samples/SampleServerObservePlugin/) (or your DLL).
2. Copy **only** approved DLLs into `plugins/` (create folder if missing).
3. Set `appsettings.json` (or environment-specific):

```json
"Zeron": {
  "PluginsEnabled": true,
  "PluginsDirectory": "plugins",
  "PluginsAllowList": "SampleServerObservePlugin.dll"
}
```

4. Restart Server (Windows Service or `dotnet Zeron.Server.dll`).

### Edition host (stricter policy + UI)

Use **`Zeron.Edition.Sample.Server`** — enables plugins in local `appsettings.json` and applies edition allow-list via `EditionSampleBootstrap.ApplyServerPluginPolicy()`.

```powershell
dotnet run -c Release --project Samples/Zeron.Edition.Sample.Server/Zeron.Edition.Sample.Server.csproj
```

### Log lines to expect

| Message | Meaning |
|---------|---------|
| `ZeronServerPluginHost ready. plugins=N dir=...` | Startup succeeded (`N` = loaded count) |
| `ZeronPluginLoader loaded server plugin '…'` | DLL accepted and initialized |
| `ZeronPluginLoader skipped 'X.dll' (load policy rejected)` | Allow-list or edition hook blocked file |
| `ZeronPluginLoader skipped 'X.dll': …` | Load/assembly error |
| `ZeronServerPluginHost plugins directory missing: …` | Folder absent (not an error; zero plugins) |
| `SampleServerObservePlugin observed agent.connected` | Sample plugin saw a bus event |

### Disable

Set `PluginsEnabled: false` and restart. Or empty `plugins/` and allow-list `*` with enabled false.

### Testing environment

`WebApplicationFactory` uses `ASPNETCORE_ENVIRONMENT=Testing` — **Server plugins do not start** (same gate as command PUB worker).

---

## Agent plugins (.NET Gate)

### Enable

1. Build [SampleAgentGatePlugin](../Samples/SampleAgentGatePlugin/).
2. Copy to agent `plugins/` (not named `Zeron.*.dll`).
3. Configure `App.config`:

```xml
<add key="script_plugins_dir" value="plugins" />
<add key="script_plugins_allowlist" value="SampleAgentGatePlugin.dll" />
```

4. Restart agent (Windows Service, `--console`, or systemd).

### Edition host

**`Zeron.Edition.Sample.Demand`** calls `EditionSampleBootstrap.RegisterDemand()` — edition policy allows **only** `SampleAgentGatePlugin.dll` (merged with config allow-list).

```powershell
dotnet run -c Release --project Samples/Zeron.Edition.Sample.Demand/Zeron.Edition.Sample.Demand.csproj -- --console
```

### Log lines to expect

| Message | Meaning |
|---------|---------|
| `ZeronGateBootstrapServer ready. timeoutMs=… plugins=N dir=…` | Gate + plugins up |
| `ZeronPluginLoader loaded agent plugin '…'` | Plugin initialized |
| `ZeronPluginLoader skipped '…' (load policy rejected)` | Config and/or edition denied DLL |
| `gate.cancelled` / install topics on bus | See [event-bus.md](./event-bus.md) |

### Smoke without full Server

- Agent: `./scripts/linux-smoke.sh` (Linux) or `--console` on Windows
- Gate sample: deploy package with plugin in `proceed` / `pause` / `cancel` mode — see SampleAgentGatePlugin README

---

## Merged allow-list rules (Agent)

Both must pass:

1. `script_plugins_allowlist` in `App.config` (`PluginAssemblyPolicy`)
2. Optional `DemandBoot.BeforeGateBootstrap` / edition hook

Server: `PluginsAllowList` in configuration **and** optional `EditionSampleBootstrap.ApplyServerPluginPolicy()` (edition hook runs before host loads; config applied inside `ZeronServerPluginHost`).

---

## Troubleshooting

| Symptom | Check |
|---------|--------|
| `plugins=0` | DLL missing, wrong name, allow-list, or reserved prefix |
| Plugin load policy rejected | File name not in allow-list; use exact `MyPlugin.dll` spelling |
| Server plugin never loads in unit tests | Testing environment skips plugin host |
| Duplicate/conflicting policy | Edition + config both restrict — list DLL in **both** |
| Linux agent | Same allow-list keys; plugins are .NET — use `net10.0` build of plugin if needed |

---

## 3-minute edition smoke checklist

**Server edition**

- [ ] Build `SampleServerObservePlugin.dll` → `plugins/`
- [ ] Run `Zeron.Edition.Sample.Server`
- [ ] Nav: **Edition sample** link
- [ ] Log: `ZeronServerPluginHost ready` with `plugins=1`

**Demand edition**

- [ ] Build `SampleAgentGatePlugin.dll` → `plugins/`
- [ ] Run `Zeron.Edition.Sample.Demand --console`
- [ ] Log: `plugins=1` in gate bootstrap line
- [ ] Unauthorized DLL rejected in log

Stock OSS binaries (`Zeron.Server`, `Zeron.Demand`) remain unchanged unless you configure plugins yourself.