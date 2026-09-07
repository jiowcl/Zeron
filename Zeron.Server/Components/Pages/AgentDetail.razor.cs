// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.AspNetCore.Components;
using System.Text.Json;
using Zeron.Server.Components.Shared;
using Zeron.Server.Data.Entities;
using Zeron.Server.ZServers;
using Zeron.ZCore.Type;

namespace Zeron.Server.Components.Pages
{
    /// <summary>
    /// AgentDetail
    /// </summary>
    public partial class AgentDetail
    {
        // Agent breadcrumbs.
        private static readonly IReadOnlyList<BreadcrumbItem> c_AgentBreadcrumbs =
        [
            new() { Label = "Agents", Href = "/agents" },
        ];

        // Agent key.
        [Parameter]
        public string AgentKey { get; set; } = "";

        // Agent.
        private AgentEntity? m_Agent;

        // Diagnostic.
        private AgentDiagnosticType? m_Diagnostic;

        // Events.
        private List<EventEntity> m_Events = [];

        // Heartbeats.
        private List<AgentHeartbeatEntity> m_Heartbeats = [];

        // Script engines from last heartbeat.
        private List<ScriptEngineInfoType> m_ScriptEngines = [];

        // Disable confirmation.
        private bool m_ShowDisableConfirm;

        // Busy.
        private bool m_IsBusy;

        /// <summary>
        /// OnParametersSetAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        protected override async Task OnParametersSetAsync()
        {
            m_Agent = await AgentManager.GetAgentByKeyAsync(AgentKey);
            m_Diagnostic = await AgentDiagnosticServer.GetDiagnosticAsync(AgentKey);
            m_Events = await EventIngestor.GetEventsAsync(AgentKey, null, 20);
            m_Heartbeats = await AgentManager.GetAgentHeartbeatsAsync(AgentKey, 30);
            m_ScriptEngines = ParseScriptEngines(m_Agent?.SupportedEnginesJson);
        }

        /// <summary>
        /// ParseScriptEngines
        /// </summary>
        /// <param name="json"></param>
        /// <returns>Returns engine list.</returns>
        private static List<ScriptEngineInfoType> ParseScriptEngines(
            string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<ScriptEngineInfoType>>(json) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }

        /// <summary>
        /// RequestDisableAgent
        /// </summary>
        /// <returns>Returns void.</returns>
        private void RequestDisableAgent()
        {
            m_ShowDisableConfirm = true;
        }

        /// <summary>
        /// CloseDisableConfirm
        /// </summary>
        /// <returns>Returns void.</returns>
        private void CloseDisableConfirm()
        {
            m_ShowDisableConfirm = false;
        }

        /// <summary>
        /// ConfirmDisableAgentAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task ConfirmDisableAgentAsync()
        {
            await DisableAgentAsync();
            m_ShowDisableConfirm = false;
        }

        /// <summary>
        /// DisableAgentAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task DisableAgentAsync()
        {
            m_IsBusy = true;

            try
            {
                m_Agent = await AgentManager.UpdateAgentAsync(AgentKey, new AgentUpdateRequestType { Status = "disabled" });
            }
            finally
            {
                m_IsBusy = false;
            }
        }

        /// <summary>
        /// EnableAgentAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task EnableAgentAsync()
        {
            m_Agent = await AgentManager.UpdateAgentAsync(AgentKey, new AgentUpdateRequestType { Status = "online" });
        }
    }
}
