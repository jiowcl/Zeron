// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Zeron.ZCore.Utils;

namespace Zeron.ZCore.Utils.Tests
{
    /// <summary>
    /// PluginAssemblyPolicyTests
    /// </summary>
    [TestClass()]
    public class PluginAssemblyPolicyTests
    {
        /// <summary>
        /// AllowAllWhenEmptyOrStarTest
        /// </summary>
        /// <returns>Returns void.</returns>
        [TestMethod()]
        public void AllowAllWhenEmptyOrStarTest()
        {
            Func<string, bool> all = PluginAssemblyPolicy.CreateFileNameAllowPredicate(null);
            Assert.IsTrue(all(@"C:\plugins\Any.dll"));
            Assert.IsTrue(PluginAssemblyPolicy.CreateFileNameAllowPredicate("*")(@"x.dll"));
        }

        /// <summary>
        /// AllowListMatchesFileNameOnlyTest
        /// </summary>
        /// <returns>Returns void.</returns>
        [TestMethod()]
        public void AllowListMatchesFileNameOnlyTest()
        {
            Func<string, bool> pred = PluginAssemblyPolicy.CreateFileNameAllowPredicate(
                "SampleAgentGatePlugin.dll;Other.dll");

            Assert.IsTrue(pred(@"/opt/zeron/plugins/SampleAgentGatePlugin.dll"));
            Assert.IsFalse(pred(@"/opt/zeron/plugins/Evil.dll"));
        }
    }
}