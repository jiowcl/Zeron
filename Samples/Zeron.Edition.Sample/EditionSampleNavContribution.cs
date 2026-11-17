// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Zeron.Server.Components.Shared;
using Zeron.Server.ZInterfaces;

namespace Zeron.Edition.Sample
{
    /// <summary>
    /// Adds a sample nav link demonstrating INavMenuContribution.
    /// </summary>
    public sealed class EditionSampleNavContribution : INavMenuContribution
    {
        /// <inheritdoc />
        public IReadOnlyList<NavMenuItem> GetItems()
        {
            return
            [
                new NavMenuItem
                {
                    Href = "https://github.com/inwazy/Zeron/blob/master/Docs/editions.md",
                    Text = "Edition sample"
                }
            ];
        }
    }
}