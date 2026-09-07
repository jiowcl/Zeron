// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Zeron.Server.Components.Shared;
using Zeron.Server.ZCore.Type;
using Zeron.Server.ZServers;
using Zeron.ZCore.Type;

namespace Zeron.Server.Components.Pages
{
    /// <summary>
    /// ScheduleCreate
    /// </summary>
    public partial class ScheduleCreate
    {
        // Schedule breadcrumbs.
        private static readonly IReadOnlyList<BreadcrumbItem> c_ScheduleBreadcrumbs =
        [
            new() { Label = "Schedules", Href = "/schedules" },
        ];

        // Model.
        private readonly ScheduleFormModelType m_Model = new();

        // Page-level error.
        private string? m_Error;

        // Field errors.
        private string? m_NameError;
        private string? m_CronError;
        private string? m_TargetApiError;
        private string? m_CommandError;
        private string? m_AgentIdError;
        private string? m_HostnameError;

        // Is submitting.
        private bool m_IsSubmitting;

        /// <summary>
        /// HandleCreateAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task HandleCreateAsync()
        {
            m_Error = null;

            if (!ValidateForm())
            {
                return;
            }

            m_IsSubmitting = true;

            try
            {
                (TaskScheduleInfoType? schedule, string? error) = await TaskScheduleServer.CreateScheduleAsync(
                    new TaskScheduleCreateRequestType
                    {
                        Name = m_Model.Name,
                        Description = m_Model.Description,
                        Cron = m_Model.Cron,
                        Enabled = m_Model.Enabled,
                        TargetApi = m_Model.TargetApi,
                        Command = m_Model.Command,
                        TargetType = m_Model.TargetType,
                        HostnamePattern = m_Model.HostnamePattern,
                        AgentIds = string.IsNullOrWhiteSpace(m_Model.AgentId) ? null : [m_Model.AgentId]
                    });

                if (error != null)
                {
                    MapServerError(error);
                    return;
                }

                Navigation.NavigateTo($"/schedules/{schedule!.Id}");
            }
            catch (Exception ex)
            {
                m_Error = ex.Message;
            }
            finally
            {
                m_IsSubmitting = false;
            }
        }

        /// <summary>
        /// ValidateForm
        /// </summary>
        /// <returns>Returns true when valid.</returns>
        private bool ValidateForm()
        {
            ClearFieldErrors();

            m_NameError = FormFieldValidation.Required(m_Model.Name, "Name");
            m_TargetApiError = FormFieldValidation.Required(m_Model.TargetApi, "Target API");
            m_CommandError = FormFieldValidation.Required(m_Model.Command, "Command");

            if (!TaskScheduleServer.TryParseCron(m_Model.Cron, out _, out string? cronError))
            {
                m_CronError = cronError;
            }

            FormFieldValidation.ValidateTargetSelection(
                m_Model.TargetType,
                m_Model.AgentId,
                m_Model.HostnamePattern,
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

            m_Error = error;
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
    }
}
