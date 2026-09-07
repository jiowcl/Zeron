// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Zeron.Server.Components.Shared
{
    /// <summary>
    /// Modal confirmation for destructive or irreversible actions.
    /// </summary>
    public partial class ConfirmDialog
    {
        // Focus targets.
        private ElementReference m_Panel;

        // Focus flags.
        private bool m_FocusPanel;

        // Open state.
        private bool m_WasOpen;

        /// <summary>
        /// Whether the dialog is visible.
        /// </summary>
        [Parameter]
        public bool IsOpen { get; set; }

        /// <summary>
        /// Dialog title.
        /// </summary>
        [Parameter]
        public string Title { get; set; } = "Confirm";

        /// <summary>
        /// Dialog message body.
        /// </summary>
        [Parameter]
        public string? Message { get; set; }

        /// <summary>
        /// Confirm button label.
        /// </summary>
        [Parameter]
        public string ConfirmText { get; set; } = "Confirm";

        /// <summary>
        /// Cancel button label.
        /// </summary>
        [Parameter]
        public string CancelText { get; set; } = "Cancel";

        /// <summary>
        /// Use danger styling on the confirm button.
        /// </summary>
        [Parameter]
        public bool IsDanger { get; set; } = true;

        /// <summary>
        /// Disable dismiss while the confirm action is running.
        /// </summary>
        [Parameter]
        public bool IsBusy { get; set; }

        /// <summary>
        /// Raised when the user confirms.
        /// </summary>
        [Parameter]
        public EventCallback OnConfirm { get; set; }

        /// <summary>
        /// Raised when the user cancels or dismisses.
        /// </summary>
        [Parameter]
        public EventCallback OnCancel { get; set; }

        /// <summary>
        /// Optional rich message content.
        /// </summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// OnParametersSet
        /// </summary>
        /// <returns>Returns void.</returns>
        protected override void OnParametersSet()
        {
            if (IsOpen && !m_WasOpen)
            {
                m_FocusPanel = true;
            }

            m_WasOpen = IsOpen;
        }

        /// <inheritdoc />
        /// <summary>
        /// OnAfterRenderAsync
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns>Returns Task.</returns>
        protected override async Task OnAfterRenderAsync(
            bool firstRender)
        {
            if (IsOpen && m_FocusPanel)
            {
                m_FocusPanel = false;

                try
                {
                    await m_Panel.FocusAsync();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// HandleConfirmAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task HandleConfirmAsync()
        {
            if (IsBusy)
            {
                return;
            }

            await OnConfirm.InvokeAsync();
        }

        /// <summary>
        /// HandleCancelAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task HandleCancelAsync()
        {
            if (IsBusy)
            {
                return;
            }

            await OnCancel.InvokeAsync();
        }

        /// <summary>
        /// OnKeyDown
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Returns Task.</returns>
        private async Task OnKeyDown(
            KeyboardEventArgs args)
        {
            if (args.Key == "Escape")
            {
                await HandleCancelAsync();
            }
        }
    }
}
