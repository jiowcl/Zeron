// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using System.Collections.Specialized;
using Zeron.ZCore;
using Zeron.ZCore.Type;
using Zeron.ZCore.Utils;
using Zeron.ZInterfaces;

namespace Zeron.ZServers.Tests
{
    /// <summary>
    /// InstallScriptHostTests
    /// </summary>
    [TestClass()]
    public class InstallScriptHostTests
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
            ScriptHostServer.Clear();
            ZeronGateServer.Current.Clear();
        }

        /// <summary>
        /// Install before script executes through ScriptHostServer using ScriptEngine.
        /// </summary>
        [TestMethod()]
        public void ExecuteInstallQueuesUsesScriptEngineTest()
        {
            if (InstallServer.Current != null)
            {
                Assert.Inconclusive("InstallServer.Current already set by another test.");
            }

            ScriptHostServer.Clear();
            ScriptHostServer.Register(new RecordingEngine("mytool"));

            m_Install = new InstallServer();
            m_Install.LoadConfig(new NameValueCollection
            {
                ["install_timer_queue_trigger_interval"] = "50000"
            });
            m_Install.Initialize();

            bool ok = InstallServer.ExecuteInstallQueues(new InstallQueuesType
            {
                PackageName = "pkg",
                Operation = "install",
                ScriptBefore = "before",
                ScriptAfter = "after",
                ScriptEngine = "mytool",
                FilePath = Path.Combine(Path.GetTempPath(), "zeron-missing-installer.exe"),
                RepoUrl = ""
            });

            Assert.IsFalse(ok);
            Assert.AreEqual(1, RecordingEngine.Scripts.Count);
            Assert.AreEqual("before", RecordingEngine.Scripts[0]);
        }

        /// <summary>
        /// Install gate Cancel skips script execution.
        /// </summary>
        [TestMethod()]
        public void ExecuteInstallQueuesGateCancelSkipsScriptTest()
        {
            if (InstallServer.Current != null)
            {
                Assert.Inconclusive("InstallServer.Current already set by another test.");
            }

            ScriptHostServer.Clear();
            ScriptHostServer.Register(new RecordingEngine("mytool"));
            ZeronGateServer.Current.Clear();
            ZeronGateServer.Current.Register(new CancelInstallGate());

            m_Install = new InstallServer();
            m_Install.LoadConfig(new NameValueCollection
            {
                ["install_timer_queue_trigger_interval"] = "50000"
            });
            m_Install.Initialize();

            bool ok = InstallServer.ExecuteInstallQueues(new InstallQueuesType
            {
                PackageName = "pkg",
                Operation = "install",
                ScriptBefore = "before",
                ScriptEngine = "mytool",
                AssignmentId = "assign-gate-1",
                FilePath = Path.Combine(Path.GetTempPath(), "zeron-missing-installer.exe"),
                RepoUrl = ""
            });

            Assert.IsFalse(ok);
            Assert.AreEqual(0, RecordingEngine.Scripts.Count);
        }

        /// <summary>
        /// CancelInstallGate test.
        /// </summary>
        private sealed class CancelInstallGate : IGateHandler
        {
            /// <summary>
            /// Handle test.
            /// </summary>
            /// <param name="context"></param>
            public void Handle(
                GateContextType context)
            {
                if (context.Topic == ZeronEventTopics.GateInstall)
                {
                    context.Decision = GateDecisionType.Cancel;
                    context.Reason = "test";
                }
            }
        }

        /// <summary>
        /// RecordingEngine test.
        /// </summary>
        private sealed class RecordingEngine : IScriptEngine
        {
            public static List<string> Scripts { get; } = [];

            /// <summary>
            /// RecordingEngine constructor.
            /// </summary>
            /// <param name="id"></param>
            public RecordingEngine(
                string id)
            {
                Id = id;
                Scripts.Clear();
            }

            public string Id { get; }

            public string DisplayName => Id;

            public IReadOnlyList<string> Platforms { get; } = ["windows"];

            public bool IsAvailable() => true;

            /// <summary>
            /// Execute test.
            /// </summary>
            /// <param name="request"></param>
            /// <returns></returns>
            public ScriptResultType Execute(
                ScriptRequestType request)
            {
                Scripts.Add(request.Script ?? "");

                return new ScriptResultType
                {
                    EngineId = Id,
                    Success = true,
                    ExitCode = 0
                };
            }
        }
    }
}
