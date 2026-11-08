// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Zeron.ZCore.Type;
using Zeron.ZCore.Utils.Engines;
using Zeron.ZInterfaces;

namespace Zeron.ZCore.Utils.Tests
{
    /// <summary>
    /// ScriptHostServerTests
    /// </summary>
    [TestClass()]
    public class ScriptHostServerTests
    {
        /// <summary>
        /// Cleanup test.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            ScriptHostServer.Clear();
        }

        /// <summary>
        /// ExecuteEmptyScriptSucceedsTest test.
        /// </summary>
        [TestMethod()]
        public void ExecuteEmptyScriptSucceedsTest()
        {
            ScriptHostServer.Clear();
            ScriptHostServer.Register(new PowerShellScriptEngine(enabled: true));

            ScriptResultType result = ScriptHostServer.Execute("powershell", "");

            Assert.IsTrue(result.Success);
            Assert.AreEqual("powershell", result.EngineId);
        }

        /// <summary>
        /// ExecuteUnknownEngineFailsTest test.
        /// </summary>
        [TestMethod()]
        public void ExecuteUnknownEngineFailsTest()
        {
            ScriptHostServer.Clear();
            ScriptHostServer.Register(new PowerShellScriptEngine(enabled: true));

            ScriptResultType result = ScriptHostServer.Execute("thinbasic", "MsgBox 1");

            Assert.IsFalse(result.Success);
            Assert.AreEqual("thinbasic", result.EngineId);
            Assert.IsTrue(result.ErrorMessage?.Contains("not registered", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// ListEnginesReportsAvailabilityTest test.
        /// </summary>
        [TestMethod()]
        public void ListEnginesReportsAvailabilityTest()
        {
            ScriptHostServer.Clear();
            ScriptHostServer.Register(new FakeScriptEngine("fake", available: true));
            ScriptHostServer.Register(new FakeScriptEngine("offline", available: false));

            List<ScriptEngineInfoType> engines = ScriptHostServer.ListEngines();
            List<ScriptEngineInfoType> available = ScriptHostServer.ListAvailable();

            Assert.AreEqual(2, engines.Count);
            Assert.AreEqual(1, available.Count);
            Assert.AreEqual("fake", available[0].Id);
        }

        /// <summary>
        /// ScriptExecutorFacadeUsesHostTest test.
        /// </summary>
        [TestMethod()]
        public void ScriptExecutorFacadeUsesHostTest()
        {
            ScriptHostServer.Clear();
            ScriptHostServer.Register(new FakeScriptEngine("powershell", available: true, succeed: true));

            Assert.IsTrue(ScriptExecutor.Execute("Write-Output ok"));
        }

        /// <summary>
        /// FakeScriptEngine test.
        /// </summary>
        private sealed class FakeScriptEngine : IScriptEngine
        {
            private readonly bool m_Available;
            private readonly bool m_Succeed;

            /// <summary>
            /// FakeScriptEngine constructor.
            /// </summary>
            /// <param name="id"></param>
            /// <param name="available"></param>
            /// <param name="succeed"></param>
            public FakeScriptEngine(
                string id,
                bool available,
                bool succeed = true)
            {
                Id = id;
                m_Available = available;
                m_Succeed = succeed;
            }

            public string Id { get; }

            public string DisplayName => Id;

            public IReadOnlyList<string> Platforms { get; } = ["windows"];

            public bool IsAvailable() => m_Available;

            /// <summary>
            /// Execute test.
            /// </summary>
            /// <param name="request"></param>
            /// <returns></returns>
            public ScriptResultType Execute(
                ScriptRequestType request)
            {
                return new ScriptResultType
                {
                    EngineId = Id,
                    Success = m_Succeed,
                    ExitCode = m_Succeed ? 0 : 1
                };
            }
        }
    }
}
