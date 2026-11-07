// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Zeron.ZCore.Type;

namespace Zeron.ZCore.Common
{
    /// <summary>
    /// TaskPipelineDocument
    /// </summary>
    /// <returns>Returns list of SchedulerTaskDefinition.</returns>
    public sealed class TaskPipelineDocument
    {
        /// <summary>
        /// Tasks
        /// </summary>
        /// <returns>Returns list of SchedulerTaskDefinition.</returns>
        public List<SchedulerTaskDefinitionType>? Tasks
        {
            get;
            set;
        }
    }
}
