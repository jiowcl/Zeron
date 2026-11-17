// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Zeron.Demand;
using Zeron.Server.ZInterfaces;
using Zeron.ZCore.Utils;

namespace Zeron.Edition.Sample
{
    /// <summary>
    /// Sample wiring for a commercial edition (nav + stricter agent plugin policy).
    /// </summary>
    public static class EditionSampleBootstrap
    {
        /// <summary>
        /// Default agent plugin allow-list for the sample edition (file names under script_plugins_dir).
        /// </summary>
        public const string DefaultAgentPluginAllowList = "SampleAgentGatePlugin.dll";

        /// <summary>
        /// Default Server plugin allow-list file name.
        /// </summary>
        public const string DefaultServerPluginAllowList = "SampleServerObservePlugin.dll";

        /// <summary>
        /// Registers Demand <see cref="DemandBoot.BeforeGateBootstrap"/> plugin policy.
        /// Call from Demand <c>Program.Main</c> before <see cref="DemandHost.Run"/>.
        /// </summary>
        public static void RegisterDemand()
        {
            DemandBoot.BeforeGateBootstrap = () =>
            {
                Func<string, bool> editionAllow = PluginAssemblyPolicy.CreateFileNameAllowPredicate(
                    DefaultAgentPluginAllowList);
                Func<string, bool>? existing = ZeronPluginLoader.CanLoadPluginAssemblyFile;

                ZeronPluginLoader.CanLoadPluginAssemblyFile = path =>
                {
                    if (!editionAllow(path))
                    {
                        return false;
                    }

                    return existing == null || existing(path);
                };
            };
        }

        /// <summary>
        /// Optional Server plugin file policy before <see cref="Zeron.Server.ZCore.ZeronServerPluginHost"/> loads DLLs.
        /// Call from edition Server <c>Program.Main</c> before <c>CreateApp</c>.
        /// </summary>
        public static void ApplyServerPluginPolicy()
        {
            Func<string, bool> editionAllow = PluginAssemblyPolicy.CreateFileNameAllowPredicate(
                DefaultServerPluginAllowList);
            Func<string, bool>? existing = ZeronPluginLoader.CanLoadPluginAssemblyFile;

            ZeronPluginLoader.CanLoadPluginAssemblyFile = path =>
            {
                if (!editionAllow(path))
                {
                    return false;
                }

                return existing == null || existing(path);
            };
        }

        /// <summary>
        /// Registers Server edition services (nav item). Pass to <c>ServerHost.BuildApplication</c> configureBuilder.
        /// </summary>
        /// <param name="builder">Web application builder.</param>
        public static void ConfigureServer(
            WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<INavMenuContribution, EditionSampleNavContribution>();
            builder.Services.AddSingleton<IAlertRowActionContribution, EditionSampleAlertRowActionContribution>();
        }
    }
}