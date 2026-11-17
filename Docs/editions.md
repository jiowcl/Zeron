# Editions & plugin trust (Windows / commercial)

Core **Zeron.Server** and **Zeron.Demand** ship a stable OSS baseline. **Editions** (e.g. ZeronOps Pro) add services, UI, and policy without forking the control plane.

## Extension hooks

| Hook | Location | Use |
|------|----------|-----|
| `ServerHost.BuildApplication(…, configureBuilder, …)` | Server | Register DI, extra endpoints, Blazor contributions |
| `INavMenuContribution` | Server Dashboard | Extra nav links |
| `IAlertRowActionContribution` | Alerts page | Row actions |
| `IRazorComponentAssemblyProvider` | Server | Edition Razor components |
| `DemandBoot.BeforeGateBootstrap` | Demand | Policy before agent plugins load |
| `ZeronPluginLoader.CanLoadPluginAssemblyFile` | Shared | Reject plugin DLL paths |
| `script_plugins_allowlist` | Demand `App.config` | Config-level DLL name allow-list |

Sample implementation: [`Samples/Zeron.Edition.Sample`](../Samples/Zeron.Edition.Sample/).

## Plugin trust model

Agent `.NET` gate plugins are **in-process code**. Before exposing `plugins/` to customers:

1. **Built-in skips** — `Zeron.*`, `System.*`, `Microsoft.*`, `netstandard*` DLLs are never loaded.
2. **Allow-list (recommended)** — `script_plugins_allowlist` in `App.config`:
   - `*` or empty — any non-reserved DLL name (development)
   - `SampleAgentGatePlugin.dll;YourCorp.Gate.dll` — production
3. **Edition hook** — stricter list in `BeforeGateBootstrap` (see `EditionSampleBootstrap`).
4. **Merged policy** — config allow-list **and** edition hook must both pass (`ZeronGateBootstrapServer`).

Scripts via Script Host remain **observe-only** (no Pause/Cancel on install/sync); see [event-bus.md](./event-bus.md).

## Wiring an edition Server

```csharp
// Program.cs in your edition host project
public static WebApplication CreateApp(string[] args) =>
    ServerHost.BuildApplication(args, EditionSampleBootstrap.ConfigureServer);
```

## Wiring an edition Demand

```csharp
EditionSampleBootstrap.RegisterDemand();
if (!DemandBoot.RegisterStandardServers()) return;
DemandHost.Run(args);
```

Publish as your own executables (`YourProduct.Server.exe`, `YourProduct.Demand.exe`) referencing `Zeron.*` assemblies.

## Related docs

- [Event Bus & Gate sample plugin](../Samples/SampleAgentGatePlugin/README.md)
- [Configuration](./configuration.md) — `script_plugins_dir`, allow-list
