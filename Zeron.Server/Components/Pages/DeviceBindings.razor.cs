// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.AspNetCore.Components.Authorization;
using Zeron.Server.Components.Shared;
using Zeron.Server.Data.Entities;
using Zeron.Server.ZCore.Type;
using Zeron.Server.ZServers;
using Zeron.ZCore.Type;

namespace Zeron.Server.Components.Pages
{
    /// <summary>
    /// DeviceBindings
    /// </summary>
    public partial class DeviceBindings
    {
        // Bindings.
        private List<UserAgentBindingInfoType> m_Bindings = [];

        // Users.
        private List<UserInfoType> m_Users = [];

        // Agents.
        private List<AgentEntity> m_Agents = [];

        // Form.
        private readonly BindFormModelType m_Form = new();

        // Message.
        private string? m_Message;

        // Succeeded.
        private bool m_Succeeded;

        // Busy.
        private bool m_IsBusy;

        // Pending unbind.
        private UserAgentBindingInfoType? m_PendingUnbind;

        // Field errors (bind form).
        private string? m_UserError;
        private string? m_AgentError;

        /// <summary>
        /// OnInitializedAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        protected override async Task OnInitializedAsync()
        {
            await ReloadAsync();
        }

        /// <summary>
        /// ReloadAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task ReloadAsync()
        {
            m_IsBusy = true;

            try
            {
                m_Bindings = await BindingServer.GetBindingsAsync();
                m_Users = await UserManager.GetUsersAsync();
                m_Agents = await AgentManager.GetAgentsAsync();
            }
            finally
            {
                m_IsBusy = false;
            }
        }

        /// <summary>
        /// BindAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task BindAsync()
        {
            m_Message = null;

            if (!ValidateBindForm())
            {
                return;
            }

            m_IsBusy = true;

            try
            {
                AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                (UserAgentBindingInfoType? binding, string? error) = await BindingServer.CreateBindingAsync(
                    new UserAgentBindingRequestType
                    {
                        UserId = m_Form.UserId,
                        AgentKey = m_Form.AgentKey
                    },
                    actor: AuditLogServer.FromPrincipal(authState.User));

                if (error != null)
                {
                    m_Succeeded = false;
                    MapServerError(error);
                    return;
                }

                m_Succeeded = true;
                m_Message = $"Bound '{binding!.Username}' to '{binding.AgentKey}'.";
                m_Form.UserId = "";
                m_Form.AgentKey = "";
                ClearFieldErrors();
                await ReloadAsync();
            }
            finally
            {
                m_IsBusy = false;
            }
        }

        /// <summary>
        /// ValidateBindForm
        /// </summary>
        /// <returns>Returns true when valid.</returns>
        private bool ValidateBindForm()
        {
            ClearFieldErrors();

            m_UserError = FormFieldValidation.Required(m_Form.UserId, "User");
            m_AgentError = FormFieldValidation.Required(m_Form.AgentKey, "Agent");

            return m_UserError == null && m_AgentError == null;
        }

        /// <summary>
        /// MapServerError
        /// </summary>
        /// <param name="error"></param>
        /// <returns>Returns void.</returns>
        private void MapServerError(
            string error)
        {
            if (error.Contains("User", StringComparison.OrdinalIgnoreCase))
            {
                m_UserError = error;
                return;
            }

            if (error.Contains("Agent", StringComparison.OrdinalIgnoreCase))
            {
                m_AgentError = error;
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
            m_UserError = null;
            m_AgentError = null;
        }

        /// <summary>
        /// RequestUnbind
        /// </summary>
        /// <param name="binding"></param>
        /// <returns>Returns void.</returns>
        private void RequestUnbind(
            UserAgentBindingInfoType binding)
        {
            m_PendingUnbind = binding;
        }

        /// <summary>
        /// CloseUnbindConfirm
        /// </summary>
        /// <returns>Returns void.</returns>
        private void CloseUnbindConfirm()
        {
            m_PendingUnbind = null;
        }

        /// <summary>
        /// ConfirmUnbindAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task ConfirmUnbindAsync()
        {
            if (m_PendingUnbind == null)
            {
                return;
            }

            UserAgentBindingInfoType binding = m_PendingUnbind;
            await UnbindAsync(binding);
            m_PendingUnbind = null;
        }

        /// <summary>
        /// UnbindAsync
        /// </summary>
        /// <param name="binding"></param>
        /// <returns>Returns Task.</returns>
        private async Task UnbindAsync(
            UserAgentBindingInfoType binding)
        {
            if (!Guid.TryParse(binding.Id, out Guid bindingId))
            {
                return;
            }

            m_IsBusy = true;
            m_Message = null;

            try
            {
                AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                string? error = await BindingServer.UnbindAsync(
                    bindingId,
                    actor: AuditLogServer.FromPrincipal(authState.User));

                if (error != null)
                {
                    m_Succeeded = false;
                    m_Message = error;
                    return;
                }

                m_Succeeded = true;
                m_Message = "Binding removed.";
                await ReloadAsync();
            }
            finally
            {
                m_IsBusy = false;
            }
        }
    }
}
