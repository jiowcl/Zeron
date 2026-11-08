// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

namespace Zeron.ZCore.Utils.Tests
{
    /// <summary>
    /// AgentIdProviderTests
    /// </summary>
    [TestClass()]
    public class AgentIdProviderTests
    {
        /// <summary>
        /// LoadOrCreateUsesConfiguredValue test.
        /// </summary>
        [TestMethod()]
        public void LoadOrCreateUsesConfiguredValueTest()
        {
            string agentId = AgentIdProvider.LoadOrCreate("configured-id", Path.GetTempFileName());

            Assert.AreEqual("configured-id", agentId);
        }

        /// <summary>
        /// LoadOrCreatePersistsGeneratedId test.
        /// </summary>
        [TestMethod()]
        public void LoadOrCreatePersistsGeneratedIdTest()
        {
            string identityFile = Path.Combine(Path.GetTempPath(), "zeron-test-" + Guid.NewGuid().ToString("N") + ".id");

            try
            {
                string first = AgentIdProvider.LoadOrCreate("", identityFile);
                string second = AgentIdProvider.LoadOrCreate("", identityFile);

                Assert.IsFalse(string.IsNullOrWhiteSpace(first));
                Assert.AreEqual(first, second);
            }
            finally
            {
                if (File.Exists(identityFile))
                {
                    File.Delete(identityFile);
                }
            }
        }
    }
}
