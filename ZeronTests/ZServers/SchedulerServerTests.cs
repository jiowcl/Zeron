// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using System.Reflection;
using Zeron.ZCore.Type;
using Zeron.ZServers;

namespace ZeronTests;

[TestClass]
public class SchedulerServerTests
{
    private const BindingFlags StaticNonPublic = BindingFlags.NonPublic | BindingFlags.Static;

    /// <summary>
    /// Tests that GetTasks returns the current list of tasks.
    /// </summary>
    [TestMethod]
    public void GetTasksReturnsCurrentTaskList()
    {
        var field = typeof(SchedulerServer).GetField("s_Tasks", StaticNonPublic);
        Assert.IsNotNull(field);

        var originalTasks = (List<SchedulerTaskDefinitionType>)field.GetValue(null)!;
        var expectedTasks = new List<SchedulerTaskDefinitionType>
        {
            new() { Name = "DailyCleanup", Cron = "0 0 * * *", Enabled = true },
            new() { Name = "NightSync", Cron = "30 2 * * *", Enabled = false }
        };

        try
        {
            field.SetValue(null, expectedTasks);

            var actual = SchedulerServer.GetTasks();

            Assert.AreSame(expectedTasks, actual);
            Assert.AreEqual(2, actual.Count);
            Assert.AreEqual("DailyCleanup", actual[0].Name);
            Assert.AreEqual("NightSync", actual[1].Name);
            Assert.IsTrue(actual[0].Enabled);
            Assert.IsFalse(actual[1].Enabled);
        }
        finally
        {
            field.SetValue(null, originalTasks);
        }
    }

    /// <summary>
    /// GetTasksReturnsEmptyListWhenNoTasksExist
    /// </summary>
    [TestMethod]
    public void GetTasksReturnsEmptyListWhenNoTasksExist()
    {
        var field = typeof(SchedulerServer).GetField("s_Tasks", StaticNonPublic);
        Assert.IsNotNull(field);

        var originalTasks = (List<SchedulerTaskDefinitionType>)field.GetValue(null)!;
        var expectedTasks = new List<SchedulerTaskDefinitionType>();

        try
        {
            field.SetValue(null, expectedTasks);

            var actual = SchedulerServer.GetTasks();

            Assert.AreSame(expectedTasks, actual);
            Assert.AreEqual(0, actual.Count);
        }
        finally
        {
            field.SetValue(null, originalTasks);
        }
    }
}