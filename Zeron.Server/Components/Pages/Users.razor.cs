// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.AspNetCore.Components.Authorization;
using Zeron.Server.Components.Shared;
using Zeron.Server.ZCore;
using Zeron.Server.ZCore.Type;
using Zeron.ZCore.Type;

namespace Zeron.Server.Components.Pages
{
    /// <summary>
    /// Users
    /// </summary>
    public partial class Users
    {
        // Rows.
        private List<UserEditRowType> m_Rows = [];

        // Create model.
        private readonly CreateFormModelType m_CreateModel = new();

        // Current user ID.
        private string? m_CurrentUserId;

        // Create message.
        private string? m_CreateMessage;

        // Update message.
        private string? m_UpdateMessage;

        // Create succeeded.
        private bool m_CreateSucceeded;

        // Update succeeded.
        private bool m_UpdateSucceeded;

        // Is busy.
        private bool m_IsBusy;

        // Pending deactivate.
        private UserEditRowType? m_PendingDeactivate;

        // Create field errors.
        private string? m_UsernameError;
        private string? m_PasswordError;
        private string? m_EmailError;

        /// <summary>
        /// OnInitializedAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        protected override async Task OnInitializedAsync()
        {
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            m_CurrentUserId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await ReloadAsync();
        }

        /// <summary>
        /// ReloadAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task ReloadAsync()
        {
            List<UserInfoType> users = await UserManager.GetUsersAsync();

            m_Rows = [.. users
                .Where(user => !string.IsNullOrWhiteSpace(user.Id))
                .Select(user => new UserEditRowType
                {
                    Id = user.Id!,
                    Username = user.Username ?? "",
                    Role = user.Role ?? ServerRoles.Viewer,
                    Email = user.Email ?? "",
                    IsActive = user.IsActive,
                    MustChangePassword = user.MustChangePassword,
                    CreatedAt = user.CreatedAt,
                    NewPassword = ""
                })];
        }

        /// <summary>
        /// CreateUserAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task CreateUserAsync()
        {
            m_CreateMessage = null;

            if (!ValidateCreateForm())
            {
                return;
            }

            m_IsBusy = true;

            try
            {
                (UserInfoType? user, string? error) = await UserManager.CreateUserAsync(new UserCreateRequestType
                {
                    Username = m_CreateModel.Username,
                    Password = m_CreateModel.Password,
                    Role = m_CreateModel.Role,
                    Email = m_CreateModel.Email
                });

                if (error != null)
                {
                    m_CreateSucceeded = false;
                    MapCreateServerError(error);
                    return;
                }

                m_CreateSucceeded = true;
                m_CreateMessage = $"Created user '{user!.Username}'.";
                m_CreateModel.Username = "";
                m_CreateModel.Password = "";
                m_CreateModel.Email = "";
                m_CreateModel.Role = ServerRoles.Viewer;
                ClearCreateFieldErrors();

                await ReloadAsync();
            }
            finally
            {
                m_IsBusy = false;
            }
        }

        /// <summary>
        /// ValidateCreateForm
        /// </summary>
        /// <returns>Returns true when valid.</returns>
        private bool ValidateCreateForm()
        {
            ClearCreateFieldErrors();

            m_UsernameError = FormFieldValidation.Required(m_CreateModel.Username, "Username");
            m_PasswordError = FormFieldValidation.MinLength(m_CreateModel.Password, 6, "Password");
            m_EmailError = FormFieldValidation.OptionalEmail(m_CreateModel.Email);

            return m_UsernameError == null
                && m_PasswordError == null
                && m_EmailError == null;
        }

        /// <summary>
        /// MapCreateServerError
        /// </summary>
        /// <param name="error"></param>
        /// <returns>Returns void.</returns>
        private void MapCreateServerError(
            string error)
        {
            if (error.Contains("Username", StringComparison.OrdinalIgnoreCase))
            {
                m_UsernameError = error;
                return;
            }

            if (error.Contains("Password", StringComparison.OrdinalIgnoreCase))
            {
                m_PasswordError = error;
                return;
            }

            if (error.Contains("Email", StringComparison.OrdinalIgnoreCase)
                || error.Contains("email", StringComparison.OrdinalIgnoreCase))
            {
                m_EmailError = error;
                return;
            }

            m_CreateMessage = error;
        }

        /// <summary>
        /// ClearCreateFieldErrors
        /// </summary>
        /// <returns>Returns void.</returns>
        private void ClearCreateFieldErrors()
        {
            m_UsernameError = null;
            m_PasswordError = null;
            m_EmailError = null;
        }

        /// <summary>
        /// SaveUserAsync
        /// </summary>
        /// <param name="row"></param>
        /// <returns>Returns Task.</returns>
        private async Task SaveUserAsync(
            UserEditRowType row)
        {
            if (!Guid.TryParse(row.Id, out Guid userId))
            {
                return;
            }

            m_UpdateMessage = null;
            m_IsBusy = true;

            try
            {
                (UserInfoType? updated, string? error) = await UserManager.UpdateUserAsync(
                    userId,
                    new UserUpdateRequestType
                    {
                        Role = row.Role,
                        Password = string.IsNullOrWhiteSpace(row.NewPassword) ? null : row.NewPassword,
                        Email = row.Email,
                        UpdateEmail = true
                    },
                    ParseCurrentUserId());

                if (error != null)
                {
                    m_UpdateSucceeded = false;
                    m_UpdateMessage = error;

                    return;
                }

                m_UpdateSucceeded = true;
                m_UpdateMessage = $"Updated user '{updated!.Username}'.";

                await ReloadAsync();
            }
            finally
            {
                m_IsBusy = false;
            }
        }

        /// <summary>
        /// RequestDeactivate
        /// </summary>
        /// <param name="row"></param>
        /// <returns>Returns void.</returns>
        private void RequestDeactivate(
            UserEditRowType row)
        {
            m_PendingDeactivate = row;
        }

        /// <summary>
        /// CloseDeactivateConfirm
        /// </summary>
        /// <returns>Returns void.</returns>
        private void CloseDeactivateConfirm()
        {
            m_PendingDeactivate = null;
        }

        /// <summary>
        /// ConfirmDeactivateAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task ConfirmDeactivateAsync()
        {
            if (m_PendingDeactivate == null)
            {
                return;
            }

            UserEditRowType row = m_PendingDeactivate;
            await SetActiveAsync(row, false);
            m_PendingDeactivate = null;
        }

        /// <summary>
        /// SetActiveAsync
        /// </summary>
        /// <param name="row"></param>
        /// <param name="isActive"></param>
        /// <returns>Returns Task.</returns>
        private async Task SetActiveAsync(
            UserEditRowType row, 
            bool isActive)
        {
            if (!Guid.TryParse(row.Id, out Guid userId))
            {
                return;
            }

            m_UpdateMessage = null;
            m_IsBusy = true;

            try
            {
                (UserInfoType? updated, string? error) = await UserManager.UpdateUserAsync(
                    userId,
                    new UserUpdateRequestType { IsActive = isActive },
                    ParseCurrentUserId());

                if (error != null)
                {
                    m_UpdateSucceeded = false;
                    m_UpdateMessage = error;

                    return;
                }

                m_UpdateSucceeded = true;
                m_UpdateMessage = isActive
                    ? $"Activated user '{updated!.Username}'."
                    : $"Deactivated user '{updated!.Username}'.";

                await ReloadAsync();
            }
            finally
            {
                m_IsBusy = false;
            }
        }

        /// <summary>
        /// ParseCurrentUserId
        /// </summary>
        /// <returns>Returns Guid?</returns>
        private Guid? ParseCurrentUserId()
        {
            return Guid.TryParse(m_CurrentUserId, out Guid userId) ? userId : null;
        }
    }
}
