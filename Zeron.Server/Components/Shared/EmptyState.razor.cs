// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.AspNetCore.Components;

namespace Zeron.Server.Components.Shared
{
    /// <summary>
    /// Consistent empty-list placeholder with optional call to action.
    /// </summary>
    public partial class EmptyState
    {
        /// <summary>
        /// Short title shown prominently.
        /// </summary>
        [Parameter]
        public string Title { get; set; } = "Nothing here yet";

        /// <summary>
        /// Supporting description.
        /// </summary>
        [Parameter]
        public string? Message { get; set; }

        /// <summary>
        /// Optional primary action label (used with ActionHref).
        /// </summary>
        [Parameter]
        public string? ActionText { get; set; }

        /// <summary>
        /// Optional primary action link.
        /// </summary>
        [Parameter]
        public string? ActionHref { get; set; }

        /// <summary>
        /// Dense layout for dashboard panels and nested sections.
        /// </summary>
        [Parameter]
        public bool Compact { get; set; }

        /// <summary>
        /// Optional rich message body.
        /// </summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Optional custom action buttons/links (e.g. AuthorizeView-wrapped CTAs).
        /// </summary>
        [Parameter]
        public RenderFragment? Actions { get; set; }
    }
}
