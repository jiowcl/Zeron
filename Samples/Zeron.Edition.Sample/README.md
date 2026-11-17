# Zeron.Edition.Sample

Minimal **commercial edition** pattern for Windows deployments:

- **Server:** `INavMenuContribution` extra nav link
- **Demand:** `DemandBoot.BeforeGateBootstrap` + `ZeronPluginLoader.CanLoadPluginAssemblyFile` allow-list

## Server host

```csharp
return ServerHost.BuildApplication(args, EditionSampleBootstrap.ConfigureServer);
```

Or from a forked `Program.cs` in your edition repo.

## Demand host

```csharp
EditionSampleBootstrap.RegisterDemand();
// DemandBoot.RegisterStandardServers();
DemandHost.Run(args);
```

## App.config allow-list (without edition code)

```xml
<add key="script_plugins_allowlist" value="SampleAgentGatePlugin.dll" />
<!-- or * for all non-reserved names -->
```

Edition hook runs **before** gate bootstrap; config allow-list is **merged** in `ZeronGateBootstrapServer` (both must allow).

See [Docs/editions.md](../../Docs/editions.md).