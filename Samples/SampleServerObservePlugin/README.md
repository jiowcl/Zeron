# SampleServerObservePlugin

Server-side sample for **`IZeronServerPlugin`**. Subscribes to `agent.connected` and writes an info log line.

## Install

1. Build this project.
2. Copy `SampleServerObservePlugin.dll` to `plugins/` next to `Zeron.Server`.
3. Configure:

```json
"Zeron": {
  "PluginsEnabled": true,
  "PluginsDirectory": "plugins",
  "PluginsAllowList": "SampleServerObservePlugin.dll"
}
```

4. Restart Server. Check logs for `ZeronServerPluginHost ready` and `SampleServerObservePlugin loaded`.

See [Docs/editions.md](../../Docs/editions.md).