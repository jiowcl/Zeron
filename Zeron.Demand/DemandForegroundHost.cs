// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zeron.ZCore;
using Zeron.ZCore.Container;

namespace Zeron.Demand
{
    /// <summary>
    /// Cross-platform host (console / systemd) — servers start in <see cref="DemandBoot"/> before this runs.
    /// </summary>
    public static class DemandForegroundHost
    {
        /// <summary>
        /// Blocks until shutdown, then stops all Demand servers.
        /// </summary>
        public static void Run()
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder();

#if !ZERON_WINDOWS
            builder.Services.AddSystemd();
#endif

            builder.Services.AddHostedService<DemandHostedService>();

            using IHost host = builder.Build();
            host.Run();
        }

        /// <summary>
        /// Hosted service hook for graceful shutdown (SIGTERM / Ctrl+C / systemd stop).
        /// </summary>
        private sealed class DemandHostedService : IHostedService
        {
            /// <inheritdoc />
            public Task StartAsync(
                CancellationToken cancellationToken)
            {
                ZNLogger.Common.Info("Zeron.Demand foreground host started.");
                return Task.CompletedTask;
            }

            /// <inheritdoc />
            public Task StopAsync(
                CancellationToken cancellationToken)
            {
                ZNLogger.Common.Info("Zeron.Demand foreground host stopping...");
                ServerIntegrate.StopAll();
                ZNLogger.Common.Info("Zeron.Demand foreground host stopped.");
                return Task.CompletedTask;
            }
        }
    }
}