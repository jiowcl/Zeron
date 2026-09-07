// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

namespace Zeron.Server.Components.Shared
{
    /// <summary>
    /// Runs an async action after a quiet period; newer calls cancel pending ones.
    /// </summary>
    public sealed class DebouncedAction : IAsyncDisposable
    {
        // Delay in milliseconds.
        private readonly int m_DelayMs;

        // CancellationTokenSource.
        private CancellationTokenSource? m_Cts;

        /// <summary>
        /// DebouncedAction
        /// </summary>
        /// <param name="delayMs"></param>
        public DebouncedAction(
            int delayMs = 350)
        {
            m_DelayMs = Math.Max(0, delayMs);
        }

        /// <summary>
        /// InvokeAsync
        /// </summary>
        /// <param name="action"></param>
        /// <returns>Returns Task.</returns>
        public async Task InvokeAsync(
            Func<Task> action)
        {
            m_Cts?.Cancel();
            m_Cts?.Dispose();
            m_Cts = new CancellationTokenSource();
            CancellationToken token = m_Cts.Token;

            try
            {
                if (m_DelayMs > 0)
                {
                    await Task.Delay(m_DelayMs, token);
                }

                await action();
            }
            catch (OperationCanceledException)
            {
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// DisposeAsync
        /// </summary>
        /// <returns>Returns ValueTask.</returns>
        public ValueTask DisposeAsync()
        {
            m_Cts?.Cancel();
            m_Cts?.Dispose();
            m_Cts = null;
            return ValueTask.CompletedTask;
        }
    }
}
