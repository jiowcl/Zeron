// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.AspNetCore.Components;
using Zeron.Server.Components.Shared;
using Zeron.Server.Data.Entities;

namespace Zeron.Server.Components.Pages
{
    /// <summary>
    /// TaskDetail
    /// </summary>
    public partial class TaskDetail
    {
        // Task breadcrumbs.
        private static readonly IReadOnlyList<BreadcrumbItem> c_TaskBreadcrumbs =
        [
            new() { Label = "Tasks", Href = "/tasks" },
        ];

        // Task ID.
        [Parameter]
        public Guid TaskId { get; set; }

        // Task.
        private TaskEntity? m_Task;

        // Cancel confirmation.
        private bool m_ShowCancelConfirm;

        // Busy.
        private bool m_IsBusy;

        /// <summary>
        /// OnParametersSetAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        protected override async Task OnParametersSetAsync()
        {
            m_Task = await TaskDispatcher.GetTaskAsync(TaskId);
        }

        /// <summary>
        /// RequestCancelTask
        /// </summary>
        /// <returns>Returns void.</returns>
        private void RequestCancelTask()
        {
            m_ShowCancelConfirm = true;
        }

        /// <summary>
        /// CloseCancelConfirm
        /// </summary>
        /// <returns>Returns void.</returns>
        private void CloseCancelConfirm()
        {
            m_ShowCancelConfirm = false;
        }

        /// <summary>
        /// ConfirmCancelTaskAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task ConfirmCancelTaskAsync()
        {
            await CancelTaskAsync();
            m_ShowCancelConfirm = false;
        }

        /// <summary>
        /// CancelTaskAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task CancelTaskAsync()
        {
            m_IsBusy = true;

            try
            {
                await TaskDispatcher.CancelTaskAsync(TaskId);
                m_Task = await TaskDispatcher.GetTaskAsync(TaskId);
            }
            finally
            {
                m_IsBusy = false;
            }
        }
    }
}
