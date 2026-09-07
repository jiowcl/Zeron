// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

namespace Zeron.Server.Components.Shared
{
    /// <summary>
    /// Breadcrumb trail item for page headers.
    /// </summary>
    public sealed class BreadcrumbItem
    {
        /// <summary>
        /// Display label.
        /// </summary>
        public string Label { get; set; } = "";

        /// <summary>
        /// Optional link target. Omit for the current page segment.
        /// </summary>
        public string? Href { get; set; }
    }
}
