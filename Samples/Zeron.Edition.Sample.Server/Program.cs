// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Zeron.Edition.Sample;

namespace Zeron.Edition.Sample.Server
{
    /// <summary>
    /// Edition sample Server entry (replaces default Zeron.Server host wiring only).
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        public static void Main(
            string[] args)
        {
            EditionSampleBootstrap.ApplyServerPluginPolicy();
            Environment.CurrentDirectory = AppContext.BaseDirectory;
            Zeron.Server.Program.CreateApp(args, EditionSampleBootstrap.ConfigureServer).Run();
        }
    }
}