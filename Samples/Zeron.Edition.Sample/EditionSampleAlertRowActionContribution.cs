// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Zeron.Server.Components.Shared;
using Zeron.Server.Data.Entities;
using Zeron.Server.ZCore.Type;
using Zeron.Server.ZInterfaces;

namespace Zeron.Edition.Sample
{
    /// <summary>
    /// Sample alert row link for open alerts with an agent key.
    /// </summary>
    public sealed class EditionSampleAlertRowActionContribution : IAlertRowActionContribution
    {
        /// <inheritdoc />
        public IReadOnlyList<AlertRowAction> GetActions(
            AlertEntity alert)
        {
            if (!string.Equals(alert.Status, AlertStatusesType.Open, StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(alert.AgentKey))
            {
                return [];
            }

            return
            [
                new AlertRowAction
                {
                    Href = $"https://github.com/inwazy/Zeron/blob/master/Docs/editions.md",
                    Text = "Edition help"
                }
            ];
        }
    }
}