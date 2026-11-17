# Zeron.Edition.Sample.Demand

Thin **edition host** for `Zeron.Demand`. Registers `EditionSampleBootstrap` so only **`SampleAgentGatePlugin.dll`** passes the edition plugin policy (in addition to `script_plugins_allowlist` in `App.config`).

## 3-minute smoke (dev)

```powershell
dotnet build ..\SampleAgentGatePlugin\SampleAgentGatePlugin.csproj -c Release
New-Item -ItemType Directory -Force .\plugins | Out-Null
Copy-Item ..\SampleAgentGatePlugin\bin\Release\net10.0-windows\SampleAgentGatePlugin.dll .\plugins\

# App.config is linked from Zeron.Demand/App.Sample.config at build output.

dotnet run -c Release --project Zeron.Edition.Sample.Demand.csproj -- --console
```

1. Log: `ZeronGateBootstrapServer ready` with `plugins=1` when sample DLL is in `plugins/`.
2. Drop `Evil.dll` in `plugins/` — log: `skipped … (load policy rejected)`.

Stock OSS agent: run `Zeron.Demand` (no edition hook unless you call `RegisterDemand()`).

See [Docs/plugins-runbook.md](../../Docs/plugins-runbook.md).