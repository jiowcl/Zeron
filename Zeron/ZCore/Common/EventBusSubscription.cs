// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Zeron.ZCore.Type;
using Zeron.ZCore.Utils;

namespace Zeron.ZCore.Common
{
    /// <summary>
    /// EventBusSubscription
    /// </summary>
    public sealed class EventBusSubscription : IDisposable
    {
        // Bus instance.
        private readonly ZeronEventBus m_Bus;

        // Subscription ID.
        private readonly Guid m_Id;

        // Disposed flag.
        private bool m_Disposed;

        /// <summary>
        /// Subscription
        /// </summary>
        /// <param name="id"></param>
        /// <param name="topicFilter"></param>
        /// <param name="handler"></param>
        /// <param name="bus"></param>
        /// <returns>Returns void.</returns>
        public EventBusSubscription(
            Guid id,
            string topicFilter,
            Action<ZeronEventType> handler,
            ZeronEventBus bus)
        {
            m_Id = id;
            TopicFilter = topicFilter;
            Handler = handler;
            m_Bus = bus;
        }

        /// <summary>
        /// TopicFilter
        /// </summary>
        public string TopicFilter
        {
            get;
        }

        /// <summary>
        /// Handler
        /// </summary>
        public Action<ZeronEventType> Handler
        {
            get;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <returns>Returns void.</returns>
        public void Dispose()
        {
            if (m_Disposed)
            {
                return;
            }

            m_Disposed = true;
            m_Bus.Unsubscribe(m_Id);
        }
    }
}
