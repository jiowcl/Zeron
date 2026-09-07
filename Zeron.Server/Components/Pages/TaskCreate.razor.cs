// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Zeron.Server.Components.Shared;
using Zeron.Server.Data.Entities;
using Zeron.Server.ZCore.Type;
using Zeron.ZCore.Type;

namespace Zeron.Server.Components.Pages
{
    /// <summary>
    /// TaskCreate
    /// </summary>
    public partial class TaskCreate
    {
        // Task breadcrumbs.
        private static readonly IReadOnlyList<BreadcrumbItem> c_TaskBreadcrumbs =
        [
            new() { Label = "Tasks", Href = "/tasks" },
        ];

        // Model.
        private readonly TaskFormModelType m_Model = new();

        // Page-level error.
        private string? m_Error;

        // Field errors.
        private string? m_NameError;
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
                TaskCreateRequestType request = new()
                {
                    Name = m_Model.Name,
                    Description = m_Model.Description,
                    TargetApi = m_Model.TargetApi,
                    Command = m_Model.Command,
                    TargetType = m_Model.TargetType,
                    HostnamePattern = m_Model.HostnamePattern,
                    AgentIds = string.IsNullOrWhiteSpace(m_Model.AgentId) ? null : [m_Model.AgentId]
                };

                TaskEntity task = await TaskDispatcher.CreateTaskAsync(request);

                Navigation.NavigateTo($"/tasks/{task.Id}");
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

            FormFieldValidation.ValidateTargetSelection(
                m_Model.TargetType,
                m_Model.AgentId,
                m_Model.HostnamePattern,
                out m_AgentIdError,
                out m_HostnameError);

            return m_NameError == null
                && m_TargetApiError == null
                && m_CommandError == null
                && m_AgentIdError == null
                && m_HostnameError == null;
        }

        /// <summary>
        /// ClearFieldErrors
        /// </summary>
        /// <returns>Returns void.</returns>
        private void ClearFieldErrors()
        {
            m_NameError = null;
            m_TargetApiError = null;
            m_CommandError = null;
            m_AgentIdError = null;
            m_HostnameError = null;
        }
    }
}
