// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

namespace Zeron.ZCore.Utils
{
    /// <summary>
    /// PluginAssemblyPolicy - optional DLL allow-list for agent/server plugin directories.
    /// </summary>
    public static class PluginAssemblyPolicy
    {
        /// <summary>
        /// Parses allow-list config: empty or <c>*</c> = allow all; otherwise comma/semicolon-separated file names.
        /// </summary>
        /// <param name="allowListConfig">Configuration value.</param>
        /// <returns>Predicate on full assembly path (compares file name only).</returns>
        public static Func<string, bool> CreateFileNameAllowPredicate(
            string? allowListConfig)
        {
            if (string.IsNullOrWhiteSpace(allowListConfig)
                || string.Equals(allowListConfig.Trim(), "*", StringComparison.Ordinal))
            {
                return _ => true;
            }

            HashSet<string> allowed = new(StringComparer.OrdinalIgnoreCase);

            foreach (string part in allowListConfig.Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries))
            {
                string name = part.Trim();

                if (!string.IsNullOrEmpty(name))
                {
                    allowed.Add(name);
                }
            }

            if (allowed.Count == 0)
            {
                return _ => true;
            }

            return fullPath =>
            {
                string fileName = Path.GetFileName(fullPath);
                return allowed.Contains(fileName);
            };
        }
    }
}