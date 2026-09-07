// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.AspNetCore.Components.Authorization;
using Zeron.Server.Components.Shared;
using Zeron.Server.ZCore;
using Zeron.ZCore.Type;

namespace Zeron.Server.Components.Pages
{
    /// <summary>
    /// Account - self-service email and password.
    /// </summary>
    public partial class Account
    {
        // Profile.
        private UserInfoType? m_Profile;

        // Email draft.
        private string m_Email = "";

        // Password fields.
        private string m_CurrentPassword = "";

        // New password.
        private string m_NewPassword = "";

        // Confirm password.
        private string m_ConfirmPassword = "";

        // Email message.
        private string? m_EmailMessage;

        // Password message.
        private string? m_PasswordMessage;

        // Field errors.
        private string? m_EmailError;
        private string? m_CurrentPasswordError;
        private string? m_NewPasswordError;
        private string? m_ConfirmPasswordError;

        // Email succeeded.
        private bool m_EmailSucceeded;

        // Password succeeded.
        private bool m_PasswordSucceeded;

        // Busy.
        private bool m_IsBusy;

        // Home path for Back link.
        private string m_HomePath = "/";

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
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            m_Profile = await AuthServer.GetUserFromPrincipalAsync(authState.User);
            m_Email = m_Profile?.Email ?? "";
            m_HomePath = string.Equals(m_Profile?.Role, ServerRoles.DeviceOwner, StringComparison.OrdinalIgnoreCase)
                ? "/my-devices"
                : "/";
        }

        /// <summary>
        /// SaveEmailAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task SaveEmailAsync()
        {
            if (m_Profile == null || !Guid.TryParse(m_Profile.Id, out Guid userId))
            {
                return;
            }

            m_EmailMessage = null;
            m_EmailError = FormFieldValidation.OptionalEmail(m_Email);

            if (m_EmailError != null)
            {
                return;
            }

            m_IsBusy = true;

            try
            {
                (UserInfoType? user, string? error) = await AuthServer.UpdateEmailAsync(userId, m_Email);

                if (error != null)
                {
                    m_EmailSucceeded = false;

                    if (error.Contains("email", StringComparison.OrdinalIgnoreCase)
                        || error.Contains("Email", StringComparison.OrdinalIgnoreCase))
                    {
                        m_EmailError = error;
                    }
                    else
                    {
                        m_EmailMessage = error;
                    }

                    return;
                }

                m_Profile = user;
                m_Email = user?.Email ?? "";
                m_EmailSucceeded = true;
                m_EmailMessage = string.IsNullOrWhiteSpace(m_Email)
                    ? "Email cleared."
                    : "Email saved.";
            }
            finally
            {
                m_IsBusy = false;
            }
        }

        /// <summary>
        /// SavePasswordAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task SavePasswordAsync()
        {
            if (m_Profile == null || !Guid.TryParse(m_Profile.Id, out Guid userId))
            {
                return;
            }

            m_PasswordMessage = null;
            ClearPasswordFieldErrors();

            m_CurrentPasswordError = FormFieldValidation.Required(m_CurrentPassword, "Current password");
            m_NewPasswordError = FormFieldValidation.MinLength(m_NewPassword, 6, "New password");

            if (!string.Equals(m_NewPassword, m_ConfirmPassword, StringComparison.Ordinal))
            {
                m_ConfirmPasswordError = "New password and confirmation do not match.";
            }

            if (m_CurrentPasswordError != null
                || m_NewPasswordError != null
                || m_ConfirmPasswordError != null)
            {
                return;
            }

            m_IsBusy = true;

            try
            {
                (UserInfoType? user, string? error) = await AuthServer.ChangePasswordAsync(
                    userId,
                    m_CurrentPassword,
                    m_NewPassword);

                if (error != null)
                {
                    m_PasswordSucceeded = false;
                    MapPasswordServerError(error);
                    return;
                }

                m_Profile = user;
                m_CurrentPassword = "";
                m_NewPassword = "";
                m_ConfirmPassword = "";
                m_PasswordSucceeded = true;
                m_PasswordMessage = "Password updated.";
            }
            finally
            {
                m_IsBusy = false;
            }
        }

        /// <summary>
        /// MapPasswordServerError
        /// </summary>
        /// <param name="error"></param>
        /// <returns>Returns void.</returns>
        private void MapPasswordServerError(
            string error)
        {
            if (error.Contains("Current password", StringComparison.OrdinalIgnoreCase)
                || error.Contains("incorrect", StringComparison.OrdinalIgnoreCase))
            {
                m_CurrentPasswordError = error;
                return;
            }

            if (error.Contains("different", StringComparison.OrdinalIgnoreCase)
                || error.Contains("at least", StringComparison.OrdinalIgnoreCase)
                || error.Contains("New password", StringComparison.OrdinalIgnoreCase))
            {
                m_NewPasswordError = error;
                return;
            }

            m_PasswordMessage = error;
        }

        /// <summary>
        /// ClearPasswordFieldErrors
        /// </summary>
        /// <returns>Returns void.</returns> 
        private void ClearPasswordFieldErrors()
        {
            m_CurrentPasswordError = null;
            m_NewPasswordError = null;
            m_ConfirmPasswordError = null;
        }
    }
}
