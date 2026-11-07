// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

namespace Zeron.Server.ZCore.Type
{
    /// <summary>
    /// CreateFormModelType
    /// </summary>
    /// <returns>Returns void.</returns>
    public sealed class CreateFormModelType
    {
        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; } = "";

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; } = "";

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; } = "";

        /// <summary>
        /// Role
        /// </summary>
        public string Role { get; set; } = ServerRoles.Viewer;
    }
}
