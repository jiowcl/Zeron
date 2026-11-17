// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Zeron.Demand;

namespace Zeron.Demand.Tests
{
    /// <summary>
    /// DemandCliTests
    /// </summary>
    [TestClass()]
    public class DemandCliTests
    {
        /// <summary>
        /// WantsForegroundHostRecognizesConsoleFlagTest
        /// </summary>
        [TestMethod()]
        public void WantsForegroundHostRecognizesConsoleFlagTest()
        {
            Assert.IsTrue(DemandCli.WantsForegroundHost(["--console"]));
            Assert.IsTrue(DemandCli.WantsForegroundHost(["-c"]));
            Assert.IsFalse(DemandCli.WantsForegroundHost([]));
        }

        /// <summary>
        /// IsTopshelfServiceCommandTest
        /// </summary>
        [TestMethod()]
        public void IsTopshelfServiceCommandTest()
        {
            Assert.IsTrue(DemandCli.IsTopshelfServiceCommand(["install"]));
            Assert.IsFalse(DemandCli.IsTopshelfServiceCommand(["--console"]));
        }
    }
}