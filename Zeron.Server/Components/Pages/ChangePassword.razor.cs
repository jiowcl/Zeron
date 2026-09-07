// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.AspNetCore.Components;

namespace Zeron.Server.Components.Pages
{
    /// <summary>
    /// ChangePassword
    /// </summary>
    public partial class ChangePassword
    {
        // Required.
        [SupplyParameterFromQuery(Name = "required")]
        public string? Required { get; set; }

        // Error code.
        [SupplyParameterFromQuery(Name = "error")]
        public string? ErrorCode { get; set; }

        // Is required.
        private bool m_IsRequired;

        // Error.
        private string? m_Error;

        // Field-level errors.
        private string? m_CurrentPasswordError;
        private string? m_NewPasswordError;
        private string? m_ConfirmPasswordError;

        // Focus targets.
        private ElementReference m_CurrentPasswordInput;
        private ElementReference m_NewPasswordInput;
        private ElementReference m_ConfirmPasswordInput;
        private bool m_FocusCurrentPassword;
        private bool m_FocusNewPassword;
        private bool m_FocusConfirmPassword;

        /// <summary>
        /// OnInitialized
        /// </summary>
        /// <returns>Returns void.</returns>
        protected override void OnInitialized()
        {
            m_IsRequired = Required == "1";
            m_Error = ErrorCode switch
            {
                "mismatch" => "New password and confirmation do not match.",
                "current" => "Current password is incorrect.",
                "same" => "New password must be different from the current password.",
                "invalid" => "Unable to update password. Use at least 6 characters.",
                _ => null
            };

            // Map error code to field-level hints.
            // Note: HTML inputs are not bound (plain form post), so we only display errors from query code.
            m_CurrentPasswordError = null;
            m_NewPasswordError = null;
            m_ConfirmPasswordError = null;
            m_FocusCurrentPassword = false;
            m_FocusNewPassword = false;
            m_FocusConfirmPassword = false;

            switch (ErrorCode)
            {
                case "current":
                    m_CurrentPasswordError = m_Error;
                    m_FocusCurrentPassword = true;
                    break;

                case "same":
                case "invalid":
                    m_NewPasswordError = m_Error;
                    m_FocusNewPassword = true;
                    break;

                case "mismatch":
                    m_NewPasswordError = m_Error;
                    m_ConfirmPasswordError = m_Error;
                    m_FocusConfirmPassword = true;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// OnAfterRenderAsync
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns>Returns Task.</returns>
        protected override async Task OnAfterRenderAsync(
            bool firstRender)
        {
            if (!firstRender)
            {
                return;
            }

            try
            {
                if (m_FocusCurrentPassword)
                {
                    await m_CurrentPasswordInput.FocusAsync();
                }
                else if (m_FocusNewPassword)
                {
                    await m_NewPasswordInput.FocusAsync();
                }
                else if (m_FocusConfirmPassword)
                {
                    await m_ConfirmPasswordInput.FocusAsync();
                }
            }
            catch
            {
            }
        }
    }
}
