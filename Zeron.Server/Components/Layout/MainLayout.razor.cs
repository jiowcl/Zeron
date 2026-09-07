// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Zeron.Server.Components.Layout
{
    /// <summary>
    /// Main application shell with responsive sidebar.
    /// </summary>
    public partial class MainLayout : IDisposable
    {
        // NavigationManager is used to navigate to different pages
        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        // m_SidebarOpen is used to control the visibility of the sidebar
        private bool m_SidebarOpen;

        /// <summary>
        /// Toggle mobile sidebar visibility.
        /// </summary>
        /// <returns>void</returns>
        private void ToggleSidebar()
        {
            m_SidebarOpen = !m_SidebarOpen;
        }

        /// <summary>
        /// Close sidebar (mobile overlay dismiss).
        /// </summary>
        /// <returns>void</returns>
        private void CloseSidebar()
        {
            if (m_SidebarOpen)
            {
                m_SidebarOpen = false;
            }
        }

        /// <inheritdoc />
        /// <returns>void</returns>
        protected override void OnInitialized()
        {
            NavigationManager.LocationChanged += OnLocationChanged;
        }

        /// <summary>
        /// Handle location changed event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        /// <returns>void</returns>
        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            if (m_SidebarOpen)
            {
                m_SidebarOpen = false;
                InvokeAsync(StateHasChanged);
            }
        }

        /// <inheritdoc />
        /// <returns>void</returns>
        public void Dispose()
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
        }
    }
}
