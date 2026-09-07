// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.AspNetCore.Components;

namespace Zeron.Server.Components.Pages
{
    /// <summary>
    /// Login
    /// </summary>
    public partial class Login
    {
        /// <summary>
        /// Failed
        /// </summary>
        [SupplyParameterFromQuery(Name = "failed")]
        public string? Failed { get; set; }

        /// <summary>
        /// Username preserved after a failed login attempt.
        /// </summary>
        [SupplyParameterFromQuery(Name = "username")]
        public string? UsernameQuery { get; set; }

        // Error message.
        private string? m_Error;

        // Field-level errors.
        private string? m_UsernameError;
        private string? m_PasswordError;

        // Preserved username.
        private string m_Username = "";

        // Focus targets.
        private ElementReference m_UsernameInput;
        private ElementReference m_PasswordInput;
        private bool m_FocusUsername;
        private bool m_FocusPassword;

        /// <summary>
        /// OnInitialized
        /// </summary>
        /// <returns>Returns void.</returns>
        protected override void OnInitialized()
        {
            m_Username = UsernameQuery?.Trim() ?? "";

            m_Error = Failed switch
            {
                "username" => "Username is required.",
                "password" => "Password is required.",
                "credentials" or "1" => "Invalid username or password.",
                _ => null
            };

            m_UsernameError = null;
            m_PasswordError = null;
            m_FocusUsername = false;
            m_FocusPassword = false;

            switch (Failed)
            {
                case "username":
                    m_UsernameError = m_Error;
                    m_FocusUsername = true;
                    break;
                case "password":
                    m_PasswordError = m_Error;
                    m_FocusPassword = true;
                    break;
                case "credentials":
                case "1":
                    m_UsernameError = m_Error;
                    m_PasswordError = m_Error;
                    // Username is usually preserved; put caret on password for the next attempt.
                    m_FocusPassword = true;
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
                if (m_FocusUsername)
                {
                    await m_UsernameInput.FocusAsync();
                }
                else if (m_FocusPassword)
                {
                    await m_PasswordInput.FocusAsync();
                }
            }
            catch
            {
            }
        }
    }
}
