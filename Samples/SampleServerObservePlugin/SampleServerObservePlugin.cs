// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using System.Globalization;
using Zeron.ZCore;
using Zeron.ZCore.Type;
using Zeron.ZInterfaces;

namespace SampleServerObservePlugin
{
    /// <summary>
    /// SampleServerObservePlugin - logs agent.connected on the in-process Server bus.
    /// Copy DLL to Zeron.Server plugins/ and set Zeron:PluginsEnabled=true.
    /// </summary>
    public sealed class SampleServerObservePlugin : IZeronServerPlugin
    {
        // Subscription to the agent.connected event.
        private IDisposable? m_Subscription;

        /// <inheritdoc />
        public string Id => "sample-server-observe";

        /// <inheritdoc />
        public void Initialize(
            IZeronEventBus bus,
            IGateController gate)
        {
            m_Subscription = bus.Subscribe(
                ZeronEventTopics.AgentConnected,
                evt => ZNLogger.Common.Info(string.Format(
                    CultureInfo.InvariantCulture,
                    "SampleServerObservePlugin observed {0} correlation={1}",
                    evt.Topic,
                    evt.CorrelationId ?? "-")));
        }

        /// <inheritdoc />
        public void Stop()
        {
            m_Subscription?.Dispose();
            m_Subscription = null;
        }
    }
}