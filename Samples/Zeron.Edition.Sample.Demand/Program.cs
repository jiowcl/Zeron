// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using NLog;
using Zeron.Demand;
using Zeron.Edition.Sample;

namespace Zeron.Edition.Sample.Demand
{
    /// <summary>
    /// Edition sample Demand entry (stricter agent plugin allow-list).
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
            LogManager.Setup().LoadConfigurationFromFile("NLog.config");
            EditionSampleBootstrap.RegisterDemand();

            if (!DemandBoot.RegisterStandardServers())
            {
                return;
            }

            DemandHost.Run(args);
        }
    }
}