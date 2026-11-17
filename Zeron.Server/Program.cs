// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.AspNetCore.Builder;

namespace Zeron.Server
{
    /// <summary>
    /// Program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Returns void.</returns>
        public static void Main(
            string[] args)
        {
            // Ensure relative paths (Data/, NLog.config) resolve next to the binary when run as a service.
            Environment.CurrentDirectory = AppContext.BaseDirectory;
            CreateApp(args).Run();
        }

        /// <summary>
        /// CreateApp
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Returns WebApplication.</returns>
        public static WebApplication CreateApp(
            string[] args)
        {
            return CreateApp(args, configureBuilder: null);
        }

        /// <summary>
        /// CreateApp - optional edition hook for <see cref="ServerHost.BuildApplication"/>.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <param name="configureBuilder">Edition DI / options registration.</param>
        /// <returns>Returns WebApplication.</returns>
        public static WebApplication CreateApp(
            string[] args,
            Action<WebApplicationBuilder>? configureBuilder)
        {
            return ServerHost.BuildApplication(args, configureBuilder);
        }
    }
}
