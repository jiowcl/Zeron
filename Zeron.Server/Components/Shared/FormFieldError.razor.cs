// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.AspNetCore.Components;

namespace Zeron.Server.Components.Shared
{
    /// <summary>
    /// Accessible inline validation message for a form field.
    /// </summary>
    public partial class FormFieldError
    {
        /// <summary>
        /// Element id referenced by the field's aria-describedby.
        /// </summary>
        [Parameter, EditorRequired]
        public string Id { get; set; } = "";

        /// <summary>
        /// Error text to announce. Hidden when empty.
        /// </summary>
        [Parameter]
        public string? Message { get; set; }
    }
}
