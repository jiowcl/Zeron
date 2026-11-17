// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace Zeron.ZCore.Utils
{
    /// <summary>
    /// Resolves secrets at rest in App.config: plaintext, Windows DPAPI, environment, or file.
    /// </summary>
    public static class SecureKeyStorage
    {
        // DPAPI prefix.
        private const string DpapiPrefix = "dpapi:";

        // Environment prefix.
        private const string EnvPrefix = "env:";

        // File prefix.
        private const string FilePrefix = "file:";

        /// <summary>
        /// Resolves a configured secret reference to plaintext.
        /// Supports <c>dpapi:</c> (Windows), <c>env:NAME</c>, <c>file:relativeOrAbsolute</c>, or raw value.
        /// </summary>
        /// <param name="configuredValue">Value from configuration.</param>
        /// <returns>Plain secret material.</returns>
        public static string ResolveConfiguredSecret(
            string configuredValue)
        {
            if (string.IsNullOrEmpty(configuredValue))
            {
                return configuredValue;
            }

            if (configuredValue.StartsWith(DpapiPrefix, StringComparison.OrdinalIgnoreCase))
            {
                if (!OperatingSystem.IsWindows())
                {
                    throw new PlatformNotSupportedException(
                        "dpapi: protected secrets require Windows. On Linux/macOS use env:VAR or file:path instead.");
                }

                return UnprotectDpapi(configuredValue[DpapiPrefix.Length..]);
            }

            if (configuredValue.StartsWith(EnvPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string name = configuredValue[EnvPrefix.Length..].Trim();

                if (string.IsNullOrEmpty(name))
                {
                    throw new InvalidOperationException("env: secret reference must include a variable name.");
                }

                string? value = Environment.GetEnvironmentVariable(name);

                if (string.IsNullOrEmpty(value))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            System.Globalization.CultureInfo.InvariantCulture,
                            "Environment variable '{0}' is not set.",
                            name));
                }

                return value;
            }

            if (configuredValue.StartsWith(FilePrefix, StringComparison.OrdinalIgnoreCase))
            {
                string path = configuredValue[FilePrefix.Length..].Trim();

                if (string.IsNullOrEmpty(path))
                {
                    throw new InvalidOperationException("file: secret reference must include a path.");
                }

                return ReadSecretFile(path);
            }

            return configuredValue;
        }

        /// <summary>
        /// Protect a plaintext secret using DPAPI (LocalMachine scope).
        /// Returns a Base64 string suitable for App.config with "dpapi:" prefix.
        /// </summary>
        /// <param name="plainText">Plain secret.</param>
        /// <returns>Base64 DPAPI blob.</returns>
        [SupportedOSPlatform("windows")]
        public static string Protect(
            string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] protectedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.LocalMachine);

            return Convert.ToBase64String(protectedBytes);
        }

        /// <summary>
        /// Unprotect a DPAPI-protected Base64 secret (Windows only).
        /// </summary>
        /// <param name="protectedBase64">Base64 DPAPI blob (without prefix).</param>
        /// <returns>Plain secret.</returns>
        [SupportedOSPlatform("windows")]
        public static string UnprotectDpapi(
            string protectedBase64)
        {
            byte[] protectedBytes = Convert.FromBase64String(protectedBase64);
            byte[] plainBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.LocalMachine);

            return Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>
        /// Reads a secret from disk (UTF-8); relative paths are under the app base directory.
        /// </summary>
        /// <param name="path">Relative or absolute file path.</param>
        /// <returns>Trimmed file contents.</returns>
        public static string ReadSecretFile(
            string path)
        {
            string fullPath = Path.IsPathRooted(path)
                ? path
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException(
                    string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "Secret file not found: {0}",
                        fullPath));
            }

            return File.ReadAllText(fullPath, Encoding.UTF8).Trim();
        }
    }
}