// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using System.Globalization;
using Zeron.ZCore;
using Zeron.ZCore.Utils;
using Zeron.ZInterfaces;

namespace Zeron.Server.ZCore
{
    /// <summary>
    /// Loads <see cref="IZeronServerPlugin"/> from a configured directory at Server startup.
    /// </summary>
    public static class ZeronServerPluginHost
    {
        // Plugins loaded by ZeronServerPluginHost
        private static readonly List<IZeronServerPlugin> s_Plugins = [];

        /// <summary>
        /// Loaded plugin count (after <see cref="Start"/>).
        /// </summary>
        public static int LoadedCount => s_Plugins.Count;

        /// <summary>
        /// Start
        /// </summary>
        /// <param name="settings">Server settings.</param>
        public static void Start(
            ServerSettings settings)
        {
            Stop();

            if (!settings.PluginsEnabled)
            {
                return;
            }

            string directory = Path.IsPathRooted(settings.PluginsDirectory)
                ? settings.PluginsDirectory
                : Path.Combine(AppContext.BaseDirectory, settings.PluginsDirectory);

            ApplyPluginLoadPolicy(settings.PluginsAllowList);

            if (!Directory.Exists(directory))
            {
                ZNLogger.Common.Info(string.Format(CultureInfo.InvariantCulture,
                    "ZeronServerPluginHost plugins directory missing: {0}", directory));
                return;
            }

            s_Plugins.AddRange(ZeronPluginLoader.LoadServerPlugins(
                directory,
                ZeronEventBus.Current,
                ZeronGateServer.Current));

            ZNLogger.Common.Info(string.Format(CultureInfo.InvariantCulture,
                "ZeronServerPluginHost ready. plugins={0} dir={1}",
                s_Plugins.Count,
                directory));
        }

        /// <summary>
        /// Stop
        /// </summary>
        public static void Stop()
        {
            foreach (IZeronServerPlugin plugin in s_Plugins)
            {
                try
                {
                    plugin.Stop();
                }
                catch (Exception e)
                {
                    ZNLogger.Common.Warn(string.Format(CultureInfo.InvariantCulture,
                        "ZeronServerPluginHost plugin stop: {0}", e.Message));
                }
            }

            s_Plugins.Clear();
        }

        /// <summary>
        /// ApplyPluginLoadPolicy
        /// </summary>
        /// <param name="allowList">Allow list.</param>
        /// <returns>Returns void.</returns>
        private static void ApplyPluginLoadPolicy(
            string? allowList)
        {
            Func<string, bool> configAllow = PluginAssemblyPolicy.CreateFileNameAllowPredicate(allowList);
            Func<string, bool>? editionHook = ZeronPluginLoader.CanLoadPluginAssemblyFile;

            ZeronPluginLoader.CanLoadPluginAssemblyFile = path =>
            {
                if (!configAllow(path))
                {
                    return false;
                }

                return editionHook == null || editionHook(path);
            };
        }
    }
}