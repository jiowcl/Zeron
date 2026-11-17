// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Zeron.ZCore.Utils;
using ServerObservePlugin = SampleServerObservePlugin.SampleServerObservePlugin;

namespace Zeron.ZCore.Tests
{
    /// <summary>
    /// ZeronServerPluginLoaderTests
    /// </summary>
    [TestClass()]
    public class ZeronServerPluginLoaderTests
    {
        /// <summary>
        /// Cleanup
        /// </summary>
        /// <returns>Returns void.</returns>
        [TestCleanup]
        public void Cleanup()
        {
            ZeronPluginLoader.CanLoadPluginAssemblyFile = null;
            ZeronEventBus.Current.Clear();
            ZeronGateServer.Current.Clear();
        }

        /// <summary>
        /// LoadServerPluginsFindsSampleAssemblyTest
        /// </summary>
        /// <returns>Returns void.</returns>
        [TestMethod()]
        public void LoadServerPluginsFindsSampleAssemblyTest()
        {
            string dir = Path.Combine(Path.GetTempPath(), "zeron-srv-plugins-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);

            try
            {
                string source = typeof(ServerObservePlugin).Assembly.Location;
                string dest = Path.Combine(dir, "SampleServerObservePlugin.dll");
                File.Copy(source, dest, overwrite: true);

                ZeronPluginLoader.CanLoadPluginAssemblyFile = PluginAssemblyPolicy.CreateFileNameAllowPredicate(
                    "SampleServerObservePlugin.dll");

                List<Zeron.ZInterfaces.IZeronServerPlugin> plugins = ZeronPluginLoader.LoadServerPlugins(
                    dir,
                    ZeronEventBus.Current,
                    ZeronGateServer.Current);

                Assert.AreEqual(1, plugins.Count);
                Assert.AreEqual("sample-server-observe", plugins[0].Id);
            }
            finally
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, recursive: true);
                }
            }
        }
    }
}