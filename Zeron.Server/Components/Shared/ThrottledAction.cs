// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

namespace Zeron.Server.Components.Shared
{
    /// <summary>
    /// Coalesces rapid invocations so an async action runs at most once per interval,
    /// always ending with a trailing run for the latest request.
    /// </summary>
    public sealed class ThrottledAction : IAsyncDisposable
    {
        // Interval in milliseconds.
        private readonly int m_IntervalMs;

        // Sync object. 
        private readonly object m_Sync = new();

        // CancellationTokenSource.
        private CancellationTokenSource? m_DelayCts;

        // Action.
        private Func<Task>? m_Action;

        // Is queued.   
        private bool m_IsQueued;

        // Is pumping.
        private bool m_IsPumping;

        // Disposed.
        private bool m_Disposed;

        /// <summary>
        /// ThrottledAction
        /// </summary>
        /// <param name="intervalMs">Minimum spacing between action runs.</param>
        public ThrottledAction(
            int intervalMs = 750)
        {
            m_IntervalMs = Math.Max(0, intervalMs);
        }

        /// <summary>
        /// InvokeAsync
        /// </summary>
        /// <param name="action"></param>
        /// <returns>Returns Task.</returns>
        public Task InvokeAsync(
            Func<Task> action)
        {
            lock (m_Sync)
            {
                if (m_Disposed)
                {
                    return Task.CompletedTask;
                }

                m_Action = action;
                m_IsQueued = true;

                if (m_IsPumping)
                {
                    return Task.CompletedTask;
                }

                m_IsPumping = true;
            }

            return PumpAsync();
        }

        /// <summary>
        /// PumpAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task PumpAsync()
        {
            try
            {
                while (true)
                {
                    Func<Task>? action;

                    lock (m_Sync)
                    {
                        if (m_Disposed || !m_IsQueued)
                        {
                            m_IsPumping = false;
                            return;
                        }

                        m_IsQueued = false;
                        action = m_Action;
                    }

                    if (action != null)
                    {
                        await action();
                    }

                    if (m_IntervalMs <= 0)
                    {
                        continue;
                    }

                    CancellationToken delayToken;

                    lock (m_Sync)
                    {
                        if (m_Disposed)
                        {
                            m_IsPumping = false;
                            return;
                        }

                        m_DelayCts?.Cancel();
                        m_DelayCts?.Dispose();
                        m_DelayCts = new CancellationTokenSource();
                        delayToken = m_DelayCts.Token;
                    }

                    try
                    {
                        await Task.Delay(m_IntervalMs, delayToken);
                    }
                    catch (OperationCanceledException)
                    {
                        lock (m_Sync)
                        {
                            if (m_Disposed)
                            {
                                m_IsPumping = false;
                                return;
                            }
                        }
                    }
                }
            }
            catch
            {
                lock (m_Sync)
                {
                    m_IsPumping = false;
                }

                throw;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// DisposeAsync
        /// </summary>
        /// <returns>Returns ValueTask.</returns>
        public ValueTask DisposeAsync()
        {
            lock (m_Sync)
            {
                m_Disposed = true;
                m_IsQueued = false;
                m_Action = null;
                m_DelayCts?.Cancel();
                m_DelayCts?.Dispose();
                m_DelayCts = null;
            }

            return ValueTask.CompletedTask;
        }
    }
}
