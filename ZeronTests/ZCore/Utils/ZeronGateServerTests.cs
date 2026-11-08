// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Zeron.ZCore.Type;
using Zeron.ZInterfaces;

namespace Zeron.ZCore.Utils.Tests
{
    /// <summary>
    /// ZeronGateServerTests
    /// </summary>
    [TestClass()]
    public class ZeronGateServerTests
    {
        /// <summary>
        /// Cleanup test.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            ZeronGateServer.Current.Clear();
        }

        /// <summary>
        /// Evaluate without handlers proceeds test.
        /// </summary>
        [TestMethod()]
        public void EvaluateWithoutHandlersProceedsTest()
        {
            GateDecisionType decision = ZeronGateServer.Evaluate("gate.command", "{}", "c1");

            Assert.AreEqual(GateDecisionType.Proceed, decision);
        }

        /// <summary>
        /// Evaluate cancel stops immediately test.
        /// </summary>
        [TestMethod()]
        public void EvaluateCancelStopsImmediatelyTest()
        {
            ZeronGateServer.Current.Register(new DelegateGateHandler(context =>
            {
                context.Decision = GateDecisionType.Cancel;
                context.Reason = "blocked";
            }));

            GateDecisionType decision = ZeronGateServer.Evaluate(ZeronEventTopics.GateCommand, "{}", "c-cancel");

            Assert.AreEqual(GateDecisionType.Cancel, decision);
        }

        /// <summary>
        /// Pause then resume proceeds test.
        /// </summary>
        [TestMethod()]
        public void PauseThenResumeProceedsTest()
        {
            ZeronGateServer.Current.Register(new DelegateGateHandler(context =>
            {
                context.Decision = GateDecisionType.Pause;
            }));

            Task<GateDecisionType> evaluate = Task.Run(() =>
                ZeronGateServer.Evaluate(ZeronEventTopics.GateCommand, "{}", "pause-1", timeoutMs: 5000));

            Assert.IsTrue(SpinWait.SpinUntil(() => ZeronGateServer.Current.Resume("pause-1"), 2000));
            Assert.AreEqual(GateDecisionType.Proceed, evaluate.Result);
        }

        /// <summary>
        /// Pause then cancel aborts test.
        /// </summary>
        [TestMethod()]
        public void PauseThenCancelAbortsTest()
        {
            ZeronGateServer.Current.Register(new DelegateGateHandler(context =>
            {
                context.Decision = GateDecisionType.Pause;
            }));

            Task<GateDecisionType> evaluate = Task.Run(() =>
                ZeronGateServer.Evaluate(ZeronEventTopics.GateInstall, "{}", "pause-2", timeoutMs: 5000));

            Assert.IsTrue(SpinWait.SpinUntil(() => ZeronGateServer.Current.Cancel("pause-2", "nope"), 2000));
            Assert.AreEqual(GateDecisionType.Cancel, evaluate.Result);
        }

        /// <summary>
        /// Pause then cancel aborts test.
        /// </summary>
        [TestMethod()]
        public void PauseTimeoutCancelsTest()
        {
            ZeronGateServer.Current.Register(new DelegateGateHandler(context =>
            {
                context.Decision = GateDecisionType.Pause;
            }));

            GateDecisionType decision = ZeronGateServer.Evaluate(
                ZeronEventTopics.GateDispatch,
                "{}",
                "pause-timeout",
                timeoutMs: 50);

            Assert.AreEqual(GateDecisionType.Cancel, decision);
        }

        /// <summary>
        /// Pause without correlation id cancels test.
        /// </summary>
        [TestMethod()]
        public void PauseWithoutCorrelationIdCancelsTest()
        {
            ZeronGateServer.Current.Register(new DelegateGateHandler(context =>
            {
                context.Decision = GateDecisionType.Pause;
            }));

            GateDecisionType decision = ZeronGateServer.Current.Evaluate(new GateContextType
            {
                Topic = ZeronEventTopics.GateCommand,
                PayloadJson = "{}"
            });

            Assert.AreEqual(GateDecisionType.Cancel, decision);
        }

        /// <summary>
        /// DelegateGateHandler test.
        /// </summary>
        private sealed class DelegateGateHandler : IGateHandler
        {
            private readonly Action<GateContextType> m_Handle;

            public DelegateGateHandler(
                Action<GateContextType> handle)
            {
                m_Handle = handle;
            }

            public void Handle(
                GateContextType context)
            {
                m_Handle(context);
            }
        }
    }
}
