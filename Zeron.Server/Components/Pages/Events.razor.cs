// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Zeron.Server.Components.Shared;
using Zeron.Server.Data.Entities;
using Zeron.Server.ZServers;

namespace Zeron.Server.Components.Pages
{
    /// <summary>
    /// Events
    /// </summary>
    public partial class Events : IAsyncDisposable
    {
        // Events.
        private List<EventEntity> m_Events = [];

        // Current page rows.
        private List<EventEntity> m_PageRows = [];

        // Agent key.
        private string? m_AgentKey;

        // Topic.
        private string? m_Topic;

        // Hub connection.
        private HubConnection? m_HubConnection;

        // Busy.
        private bool m_IsBusy;

        // Filter debounce.
        private readonly DebouncedAction m_FilterDebounce = new(350);

        // Hub reload throttle.
        private readonly ThrottledAction m_HubReloadThrottle = new(750);

        // Pagination.
        private const int c_PageSize = 50;

        // Page index.
        private int m_PageIndex;

        // Has next page.
        private bool m_HasNextPage;

        // Topic query.
        [SupplyParameterFromQuery(Name = "topic")]
        public string? TopicQuery { get; set; }

        // Agent key query.
        [SupplyParameterFromQuery(Name = "agentKey")]
        public string? AgentKeyQuery { get; set; }

        /// <summary>
        /// OnInitializedAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        protected override async Task OnInitializedAsync()
        {
            if (!string.IsNullOrWhiteSpace(TopicQuery))
            {
                m_Topic = TopicQuery;
            }

            if (!string.IsNullOrWhiteSpace(AgentKeyQuery))
            {
                m_AgentKey = AgentKeyQuery;
            }

            await ReloadAsync(showBusy: true);
            await ConnectHubAsync();
        }

        /// <summary>
        /// ManualRefreshAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private Task ManualRefreshAsync()
        {
            return ReloadAsync(showBusy: true);
        }

        /// <summary>
        /// ReloadAsync
        /// </summary>
        /// <param name="showBusy"></param>
        /// <returns>Returns Task.</returns>
        private async Task ReloadAsync(
            bool showBusy = true)
        {
            if (showBusy)
            {
                m_IsBusy = true;
            }

            try
            {
                int offset = m_PageIndex * c_PageSize;
                m_Events = await EventIngestor.GetEventsAsync(
                    m_AgentKey,
                    m_Topic,
                    limit: c_PageSize + 1,
                    offset: offset);

                m_HasNextPage = m_Events.Count > c_PageSize;
                m_PageRows = m_Events.Take(c_PageSize).ToList();
            }
            finally
            {
                if (showBusy)
                {
                    m_IsBusy = false;
                }
            }
        }

        /// <summary>
        /// ScheduleHubReloadAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private Task ScheduleHubReloadAsync()
        {
            return m_HubReloadThrottle.InvokeAsync(async () =>
            {
                await ReloadAsync(showBusy: false);
                await InvokeAsync(StateHasChanged);
            });
        }

        /// <summary>
        /// ConnectHubAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task ConnectHubAsync()
        {
            m_HubConnection = DashboardHubClient.Create(Navigation, HttpContextAccessor);

            m_HubConnection.On<long, string?, string, string, DateTime>("EventReceived", async (_, agentKey, topic, payload, receivedAt) =>
            {
                if (!string.IsNullOrWhiteSpace(m_AgentKey) && m_AgentKey != agentKey)
                {
                    return;
                }

                if (!string.IsNullOrWhiteSpace(m_Topic) && !topic.Contains(m_Topic, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                await ScheduleHubReloadAsync();
            });

            await DashboardHubClient.TryStartAsync(m_HubConnection);
        }

        /// <summary>
        /// DisposeAsync
        /// </summary>
        /// <returns>Returns ValueTask.</returns>
        public async ValueTask DisposeAsync()
        {
            await m_FilterDebounce.DisposeAsync();
            await m_HubReloadThrottle.DisposeAsync();

            if (m_HubConnection != null)
            {
                await m_HubConnection.DisposeAsync();
            }
        }

        /// <summary>
        /// ScheduleFilterAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private Task ScheduleFilterAsync()
        {
            return m_FilterDebounce.InvokeAsync(async () =>
            {
                m_PageIndex = 0;
                
                await ReloadAsync(showBusy: true);
                await InvokeAsync(StateHasChanged);
            });
        }

        /// <summary>
        /// ApplyFiltersAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task ApplyFiltersAsync()
        {
            m_PageIndex = 0;

            await ReloadAsync(showBusy: true);
        }

        /// <summary>
        /// GoPrevPageAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task GoPrevPageAsync()
        {
            if (m_PageIndex <= 0)
            {
                return;
            }

            m_PageIndex--;

            await ReloadAsync(showBusy: true);
        }

        /// <summary>
        /// GoNextPageAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task GoNextPageAsync()
        {
            if (!m_HasNextPage)
            {
                return;
            }

            m_PageIndex++;

            await ReloadAsync(showBusy: true);
        }

        /// <summary>
        /// PageSummary
        /// </summary>
        private string PageSummary =>
            UiFormatServer.FormatPageRange(m_PageIndex, c_PageSize, m_PageRows.Count, "event");
    }
}
