// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Zeron.Server.Data.Entities;

namespace Zeron.Server.Components.Pages
{
    /// <summary>
    /// Tasks
    /// </summary>
    public partial class Tasks
    {
        // Tasks.
        private List<TaskEntity> m_Tasks = [];

        // Busy.
        private bool m_IsBusy;

        /// <summary>
        /// OnInitializedAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        protected override async Task OnInitializedAsync()
        {
            await ReloadAsync();
        }

        /// <summary>
        /// ReloadAsync
        /// </summary>
        /// <returns>Returns Task.</returns>
        private async Task ReloadAsync()
        {
            m_IsBusy = true;

            try
            {
                m_Tasks = await TaskDispatcher.GetTasksAsync();
            }
            finally
            {
                m_IsBusy = false;
            }
        }
    }
}
