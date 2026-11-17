// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.AspNetCore.Components;
using Zeron.Server.Components.Shared;
using Zeron.Server.ZCore.Type;
using Zeron.Server.ZServers;
using Zeron.ZCore.Type;

namespace Zeron.Server.Components.Pages
{
    /// <summary>
    /// ScheduleDetail
    /// </summary>
    public partial class ScheduleDetail
    {
        // Schedule breadcrumbs.
        private static readonly IReadOnlyList<BreadcrumbItem> c_ScheduleBreadcrumbs =
        [
            new() { Label = "Schedules", Href = "/schedules" },
        ];

        // Schedule ID.
        [Parameter]
        public Guid ScheduleId { get; set; }

        // Schedule.
        private TaskScheduleInfoType? m_Schedule;

        // Edit form model.
        private readonly EditFormModelType m_Edit = new();

        // Message.
        private string? m_Message;

        // Is succeeded.
        private bool m_Succeeded;

        // Is busy.
        private bool m_IsBusy;

        // Run now confirmation.
        private bool m_ShowRunNowConfirm;

        // Delete confirmation.
        private bool m_ShowDeleteConfirm;

        // Field Name errors.
        private string? m_NameError;

        // Field Cron errors.
        private string? m_CronError;

        // Field Target API errors. 
        private string? m_TargetApiError;
        
        // Field Command errors.
        private string? m_CommandError;

        // Field Agent ID errors.
        private string? m_AgentIdError;

        // Field Hostname errors.
        private string? m_HostnameError;

        /// <summary>
        /// OnParametersSetAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        protected override async Task OnParametersSetAsync()
        {
            await ReloadAsync();
        }

        /// <summary>
        /// ReloadAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task ReloadAsync()
        {
            m_Schedule = await TaskScheduleServer.GetScheduleAsync(ScheduleId);

            if (m_Schedule == null)
            {
                return;
            }

            m_Edit.Name = m_Schedule.Name;
            m_Edit.Description = m_Schedule.Description ?? "";
            m_Edit.Cron = m_Schedule.Cron;
            m_Edit.TargetApi = m_Schedule.TargetApi;
            m_Edit.Command = m_Schedule.Command;
            m_Edit.TargetType = m_Schedule.TargetType;
            m_Edit.AgentId = m_Schedule.AgentIds?.FirstOrDefault() ?? "";
            m_Edit.HostnamePattern = m_Schedule.HostnamePattern ?? "";
        }

        /// <summary>
        /// SaveAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task SaveAsync()
        {
            m_Message = null;

            if (!ValidateEditForm())
            {
                return;
            }

            m_IsBusy = true;

            try
            {
                (TaskScheduleInfoType? updated, string? error) = await TaskScheduleServer.UpdateScheduleAsync(
                    ScheduleId,
                    new TaskScheduleUpdateRequestType
                    {
                        Name = m_Edit.Name,
                        Description = m_Edit.Description,
                        Cron = m_Edit.Cron,
                        TargetApi = m_Edit.TargetApi,
                        Command = m_Edit.Command,
                        TargetType = m_Edit.TargetType,
                        AgentIds = string.IsNullOrWhiteSpace(m_Edit.AgentId) ? null : [m_Edit.AgentId],
                        HostnamePattern = string.IsNullOrWhiteSpace(m_Edit.HostnamePattern) ? null : m_Edit.HostnamePattern
                    });

                if (error != null)
                {
                    m_Succeeded = false;
                    MapServerError(error);
                    return;
                }

                m_Schedule = updated;
                m_Succeeded = true;
                m_Message = "Schedule updated.";
                ClearFieldErrors();

                await ReloadAsync();
            }
            finally
            {
                m_IsBusy = false;
            }
        }

        /// <summary>
        /// ValidateEditForm
        /// </summary>
        /// <returns>Returns true when valid.</returns>
        private bool ValidateEditForm()
        {
            ClearFieldErrors();

            m_NameError = FormFieldValidation.Required(m_Edit.Name, "Name");
            m_TargetApiError = FormFieldValidation.Required(m_Edit.TargetApi, "Target API");
            m_CommandError = FormFieldValidation.Required(m_Edit.Command, "Command");

            if (!TaskScheduleServer.TryParseCron(m_Edit.Cron, out _, out string? cronError))
            {
                m_CronError = cronError;
            }

            FormFieldValidation.ValidateTargetSelection(
                m_Edit.TargetType,
                m_Edit.AgentId,
                m_Edit.HostnamePattern,
                out m_AgentIdError,
                out m_HostnameError);

            return m_NameError == null
                && m_CronError == null
                && m_TargetApiError == null
                && m_CommandError == null
                && m_AgentIdError == null
                && m_HostnameError == null;
        }

        /// <summary>
        /// MapServerError
        /// </summary>
        /// <param name="error"></param>
        /// <returns>Returns void.</returns>
        private void MapServerError(
            string error)
        {
            if (error.Contains("cron", StringComparison.OrdinalIgnoreCase))
            {
                m_CronError = error;
                return;
            }

            if (error.Contains("Name", StringComparison.OrdinalIgnoreCase))
            {
                m_NameError = error;
                return;
            }

            if (error.Contains("Target API", StringComparison.OrdinalIgnoreCase))
            {
                m_TargetApiError = error;
                return;
            }

            if (error.Contains("Command", StringComparison.OrdinalIgnoreCase))
            {
                m_CommandError = error;
                return;
            }

            m_Message = error;
        }

        /// <summary>
        /// ClearFieldErrors
        /// </summary>
        /// <returns>Returns void.</returns>
        private void ClearFieldErrors()
        {
            m_NameError = null;
            m_CronError = null;
            m_TargetApiError = null;
            m_CommandError = null;
            m_AgentIdError = null;
            m_HostnameError = null;
        }

        /// <summary>
        /// SetEnabledAsync
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns>Returns Task.</returns>
        private async Task SetEnabledAsync(
            bool enabled)
        {
            m_Message = null;
            m_IsBusy = true;

            try
            {
                (TaskScheduleInfoType? updated, string? error) = await TaskScheduleServer.SetEnabledAsync(ScheduleId, enabled);

                if (error != null)
                {
                    m_Succeeded = false;
                    m_Message = error;

                    return;
                }

                m_Schedule = updated;
                m_Succeeded = true;
                m_Message = enabled ? "Schedule enabled." : "Schedule disabled.";
            }
            finally
            {
                m_IsBusy = false;
            }
        }

        /// <summary>
        /// RequestRunNow
        /// </summary>
        /// <returns>Returns void.</returns>
        private void RequestRunNow()
        {
            m_ShowRunNowConfirm = true;
        }

        /// <summary>
        /// CloseRunNowConfirm
        /// </summary>
        /// <returns>Returns void.</returns>
        private void CloseRunNowConfirm()
        {
            m_ShowRunNowConfirm = false;
        }

        /// <summary>
        /// ConfirmRunNowAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task ConfirmRunNowAsync()
        {
            await RunNowAsync();
            m_ShowRunNowConfirm = false;
        }

        /// <summary>
        /// RunNowAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task RunNowAsync()
        {
            m_Message = null;
            m_IsBusy = true;

            try
            {
                (Guid? taskId, string? error) = await TaskScheduleServer.TriggerNowAsync(ScheduleId);

                if (error != null || taskId == null)
                {
                    m_Succeeded = false;
                    m_Message = error ?? "Failed to run schedule.";

                    return;
                }

                Navigation.NavigateTo($"/tasks/{taskId}");
            }
            finally
            {
                m_IsBusy = false;
            }
        }

        /// <summary>
        /// RequestDelete
        /// </summary>
        /// <returns>Returns void.</returns>
        private void RequestDelete()
        {
            m_ShowDeleteConfirm = true;
        }

        /// <summary>
        /// CloseDeleteConfirm
        /// </summary>
        /// <returns>Returns void.</returns>
        private void CloseDeleteConfirm()
        {
            m_ShowDeleteConfirm = false;
        }

        /// <summary>
        /// ConfirmDeleteAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task ConfirmDeleteAsync()
        {
            await DeleteAsync();
            m_ShowDeleteConfirm = false;
        }

        /// <summary>
        /// DeleteAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task DeleteAsync()
        {
            m_IsBusy = true;

            try
            {
                bool deleted = await TaskScheduleServer.DeleteScheduleAsync(ScheduleId);

                if (!deleted)
                {
                    m_Succeeded = false;
                    m_Message = "Schedule not found.";

                    return;
                }

                Navigation.NavigateTo("/schedules");
            }
            finally
            {
                m_IsBusy = false;
            }
        }
    }
}
