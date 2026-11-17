// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using System.Globalization;
using Zeron.Demand.ZCore;
using Zeron.ZServers;

#if ZERON_WINDOWS
using Topshelf;
#endif

namespace Zeron.Demand
{
    /// <summary>
    /// Demand process host: Windows Service (Topshelf) or cross-platform foreground / systemd.
    /// </summary>
    public static class DemandHost
    {
        /// <summary>
        /// Runs the agent host chosen from CLI flags and OS.
        /// </summary>
        /// <param name="args">Process arguments.</param>
        public static void Run(
            string[] args)
        {
            if (DemandCli.ShouldUseForegroundHost(args))
            {
                DemandForegroundHost.Run();
                return;
            }

#if ZERON_WINDOWS
            RunWindowsService();
#else
            DemandForegroundHost.Run();
#endif
        }

#if ZERON_WINDOWS
        /// <summary>
        /// Runs the Demand agent as a Windows Service (Topshelf).
        /// </summary>
        public static void RunWindowsService()
        {
            SCHostLoader hostLoader = new();

            TopshelfExitCode rc = HostFactory.Run(x =>
            {
                x.Service<SCLoader>(sc =>
                {
                    sc.ConstructUsing(_ => new SCLoader());

                    sc.WhenStarted(tc => tc.WhenStart());
                    sc.WhenStopped(tc => tc.WhenStop());
                    sc.WhenPaused(tc => tc.WhenPause());
                    sc.WhenContinued(tc => tc.WhenContinue());
                    sc.WhenShutdown(tc => tc.WhenShutdown());
                });

                x.RunAsLocalSystem();

                x.SetServiceName(DeployServer.ServiceName);
                x.SetDisplayName(DeployServer.ServiceDisplayName);
                x.SetInstanceName(DeployServer.ServiceInstanceName);
                x.SetDescription(DeployServer.ServiceDescription);

                x.BeforeInstall(_ => hostLoader.BeforeInstall());
                x.BeforeUninstall(() => hostLoader.BeforeUninstall());

                x.AfterInstall(_ => hostLoader.AfterInstall());
                x.AfterUninstall(() => hostLoader.AfterUninstall());
            });

            int exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode(), CultureInfo.InvariantCulture);
            Environment.ExitCode = exitCode;
        }
#endif
    }
}