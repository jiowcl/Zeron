// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

namespace Zeron.Demand
{
    /// <summary>
    /// Parses Demand host CLI flags (foreground vs service host).
    /// </summary>
    public static class DemandCli
    {
        // Foreground flags.
        private static readonly HashSet<string> ForegroundFlags = new(StringComparer.OrdinalIgnoreCase)
        {
            "--console",
            "-c",
            "--foreground",
            "--fg"
        };

        // Topshelf service management verbs (Windows only).
        private static readonly HashSet<string> TopshelfServiceVerbs = new(StringComparer.OrdinalIgnoreCase)
        {
            "install",
            "uninstall",
            "start",
            "stop",
            "pause",
            "resume"
        };

        /// <summary>
        /// True when the user asked for foreground / console host (not Topshelf service runner).
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool WantsForegroundHost(
            string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return false;
            }

            foreach (string arg in args)
            {
                if (ForegroundFlags.Contains(arg))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// True when args look like Topshelf install/start/stop (Windows service management).
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool IsTopshelfServiceCommand(
            string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return false;
            }

            return TopshelfServiceVerbs.Contains(args[0]);
        }

        /// <summary>
        /// Chooses foreground host when explicitly requested or when not on Windows.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool ShouldUseForegroundHost(
            string[] args)
        {
            if (!OperatingSystem.IsWindows())
            {
                return true;
            }

            if (WantsForegroundHost(args))
            {
                return true;
            }

            if (IsTopshelfServiceCommand(args))
            {
                return false;
            }

            string? env = Environment.GetEnvironmentVariable("ZERON_DEMAND_FOREGROUND");
            if (string.Equals(env, "1", StringComparison.Ordinal)
                || string.Equals(env, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}