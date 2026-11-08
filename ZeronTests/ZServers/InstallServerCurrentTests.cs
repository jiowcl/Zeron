// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using System.Collections.Specialized;
using Zeron.ZCore.Type;

namespace Zeron.ZServers.Tests
{
    /// <summary>
    /// InstallServerCurrentTests
    /// </summary>
    [TestClass()]
    public class InstallServerCurrentTests
    {
        private InstallServer? m_Install;

        /// <summary>
        /// Cleanup test.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            if (m_Install != null && ReferenceEquals(InstallServer.Current, m_Install))
            {
                m_Install.Stop();
            }

            m_Install = null;
        }

        /// <summary>
        /// FacadesAreSafeWhenCurrentIsNullTest test.
        /// </summary>
        [TestMethod()]
        public void FacadesAreSafeWhenCurrentIsNullTest()
        {
            if (InstallServer.Current != null)
            {
                Assert.Inconclusive("InstallServer.Current already set by another test.");
            }

            Assert.AreEqual(0, InstallServer.GetQueueCount());
            Assert.AreEqual(0, InstallServer.AddQueues("install", new InstallQueuesType { PackageName = "x" }));
            Assert.IsFalse(InstallServer.ExecuteQueues("install", new InstallQueuesType()));
        }

        /// <summary>
        /// InitializeSetsCurrentAndStopClearsTest test.
        /// </summary>
        [TestMethod()]
        public void InitializeSetsCurrentAndStopClearsTest()
        {
            m_Install = new InstallServer();
            m_Install.LoadConfig(new NameValueCollection
            {
                ["install_timer_queue_trigger_interval"] = "50000"
            });
            m_Install.Initialize();

            Assert.AreSame(m_Install, InstallServer.Current);
            Assert.AreEqual(1, InstallServer.AddQueues("install", new InstallQueuesType
            {
                PackageName = "pkg",
                FilePath = "C:\\missing\\installer.exe",
                RepoUrl = ""
            }));
            Assert.AreEqual(1, InstallServer.GetQueueCount());

            m_Install.Stop();
            Assert.IsNull(InstallServer.Current);
            Assert.AreEqual(0, InstallServer.GetQueueCount());
            m_Install = null;
        }
    }
}
