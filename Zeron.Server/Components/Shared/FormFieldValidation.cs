// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

namespace Zeron.Server.Components.Shared
{
    /// <summary>
    /// Shared client-side field validation helpers for dashboard forms.
    /// </summary>
    public static class FormFieldValidation
    {
        /// <summary>
        /// Required
        /// </summary>
        /// <param name="value"></param>
        /// <param name="label"></param>
        /// <returns>Returns error or null.</returns>
        public static string? Required(
            string? value,
            string label)
        {
            return string.IsNullOrWhiteSpace(value)
                ? $"{label} is required."
                : null;
        }

        /// <summary>
        /// MinLength
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minLength"></param>
        /// <param name="label"></param>
        /// <returns>Returns error or null.</returns>
        public static string? MinLength(
            string? value,
            int minLength,
            string label)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return $"{label} is required.";
            }

            return value.Length < minLength
                ? $"{label} must be at least {minLength} characters."
                : null;
        }

        /// <summary>
        /// OptionalEmail
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns error or null.</returns>
        public static string? OptionalEmail(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string email = value.Trim();

            if (email.Length > 254 || email.Contains(' ', StringComparison.Ordinal))
            {
                return "Enter a valid email address.";
            }

            int at = email.IndexOf('@');

            if (at <= 0
                || at >= email.Length - 1
                || email.LastIndexOf('@') != at
                || email.IndexOf('.', at + 1) <= at + 1)
            {
                return "Enter a valid email address.";
            }

            return null;
        }

        /// <summary>
        /// ValidateTargetSelection
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="agentId"></param>
        /// <param name="hostnamePattern"></param>
        /// <param name="agentError"></param>
        /// <param name="hostnameError"></param>
        /// <returns>Returns true when valid.</returns>
        public static bool ValidateTargetSelection(
            string? targetType,
            string? agentId,
            string? hostnamePattern,
            out string? agentError,
            out string? hostnameError)
        {
            agentError = null;
            hostnameError = null;

            if (string.Equals(targetType, "agent", StringComparison.OrdinalIgnoreCase))
            {
                agentError = Required(agentId, "Agent Id");
            }
            else if (string.Equals(targetType, "filter", StringComparison.OrdinalIgnoreCase))
            {
                hostnameError = Required(hostnamePattern, "Hostname Pattern");
            }

            return agentError == null && hostnameError == null;
        }

        /// <summary>
        /// DescribedBy
        /// </summary>
        /// <param name="id"></param>
        /// <param name="error"></param>
        /// <returns>Returns id when error is present.</returns>
        public static string? DescribedBy(
            string id,
            string? error)
        {
            return string.IsNullOrWhiteSpace(error) ? null : id;
        }
    }
}
