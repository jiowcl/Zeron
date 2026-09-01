// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.AspNetCore.Components;

namespace Zeron.Server.Components.Shared
{
    /// <summary>
    /// Unified page title area with optional breadcrumbs and actions.
    /// </summary>
    public partial class PageHeader
    {
        /// <summary>
        /// Page title.
        /// </summary>
        [Parameter]
        public string Title { get; set; } = "";

        /// <summary>
        /// Optional subtitle shown below the title.
        /// </summary>
        [Parameter]
        public string? Subtitle { get; set; }

        /// <summary>
        /// Optional breadcrumb trail (parent segments only; title remains in h1).
        /// </summary>
        [Parameter]
        public IReadOnlyList<BreadcrumbItem>? Breadcrumbs { get; set; }

        /// <summary>
        /// Optional action buttons or controls aligned to the right.
        /// </summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }
    }
}
