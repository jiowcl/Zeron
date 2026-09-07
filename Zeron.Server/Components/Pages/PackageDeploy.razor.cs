// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.AspNetCore.Components.Authorization;
using Zeron.Server.Components.Shared;
using Zeron.Server.ZCore.Type;
using Zeron.Server.ZServers;
using Zeron.ZCore.Type;

namespace Zeron.Server.Components.Pages
{
    /// <summary>
    /// PackageDeploy
    /// </summary>
    public partial class PackageDeploy
    {
        // Package breadcrumbs.
        private static readonly IReadOnlyList<BreadcrumbItem> c_PackageBreadcrumbs =
        [
            new() { Label = "Packages", Href = "/packages" },
        ];

        // Model.
        private readonly DeployFormModelType m_Model = new();

        // Catalog packages.
        private List<ManagedPackageInfoType> m_Packages = [];

        // Page-level error.
        private string? m_Error;

        // Field errors.
        private string? m_PackageError;
        private string? m_AgentIdError;
        private string? m_HostnameError;

        // Is submitting.
        private bool m_IsSubmitting;

        // Command preview.
        private string m_CommandPreview =>
            string.IsNullOrWhiteSpace(m_Model.PackageName)
                ? ""
                : PackageDeployServer.BuildCommand(
                    m_Model.Operation,
                    m_Model.PackageName.Trim(),
                    m_Model.ExtraArgs);

        /// <summary>
        /// OnInitializedAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        protected override async Task OnInitializedAsync()
        {
            m_Packages = await CatalogServer.GetPackagesAsync(enabledOnly: true);
        }

        /// <summary>
        /// HandleDeployAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task HandleDeployAsync()
        {
            m_Error = null;

            if (!ValidateForm())
            {
                return;
            }

            m_IsSubmitting = true;

            try
            {
                AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                PackageDeployResponseType response = await PackageDeployServer.DeployAsync(
                    new PackageDeployRequestType
                    {
                        Operation = m_Model.Operation,
                        PackageName = m_Model.PackageName,
                        ExtraArgs = m_Model.ExtraArgs,
                        Name = string.IsNullOrWhiteSpace(m_Model.Name) ? null : m_Model.Name,
                        TargetType = m_Model.TargetType,
                        AgentIds = string.IsNullOrWhiteSpace(m_Model.AgentId) ? null : [m_Model.AgentId],
                        HostnamePattern = m_Model.HostnamePattern
                    },
                    actor: AuditLogServer.FromPrincipal(authState.User));

                if (!response.Success || response.TaskId == null)
                {
                    MapServerError(response.Message ?? "Deploy failed.");
                    return;
                }

                Navigation.NavigateTo($"/tasks/{response.TaskId}");
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

            m_PackageError = FormFieldValidation.Required(m_Model.PackageName, "Package");

            FormFieldValidation.ValidateTargetSelection(
                m_Model.TargetType,
                m_Model.AgentId,
                m_Model.HostnamePattern,
                out m_AgentIdError,
                out m_HostnameError);

            return m_PackageError == null
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
            if (error.Contains("package", StringComparison.OrdinalIgnoreCase))
            {
                m_PackageError = error;
                return;
            }

            if (error.Contains("agent", StringComparison.OrdinalIgnoreCase))
            {
                m_AgentIdError = error;
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
            m_PackageError = null;
            m_AgentIdError = null;
            m_HostnameError = null;
        }
    }
}
