// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

namespace Zeron.ZCore.Utils.Tests
{
    /// <summary>
    /// ApiScopeValidatorTests
    /// </summary>
    [TestClass()]
    public class ApiScopeValidatorTests
    {
        /// <summary>
        /// AllowAllWhenScopeIsStar test.
        /// </summary>
        [TestMethod()]
        public void AllowAllWhenScopeIsStarTest()
        {
            Assert.IsTrue(ApiScopeValidator.IsAllowed("*", "PowerShell", "write"));
        }

        /// <summary>
        /// AllowMatchingApiAndActionTest test.
        /// </summary>
        [TestMethod()]
        public void AllowMatchingApiAndActionTest()
        {
            string scopes = "HealthCheck:read,ManagedPackage:install";

            Assert.IsTrue(ApiScopeValidator.IsAllowed(scopes, "HealthCheck", "read"));
            Assert.IsTrue(ApiScopeValidator.IsAllowed(scopes, "ManagedPackage", "install"));
        }

        /// <summary>
        /// DenyMissingScopeTest test.
        /// </summary>
        [TestMethod()]
        public void DenyMissingScopeTest()
        {
            string scopes = "HealthCheck:read";

            Assert.IsFalse(ApiScopeValidator.IsAllowed(scopes, "PowerShell", "write"));
        }

        /// <summary>
        /// AllowApiWildcardScopeTest test.
        /// </summary>
        [TestMethod()]
        public void AllowApiWildcardScopeTest()
        {
            Assert.IsTrue(ApiScopeValidator.IsAllowed("FileSystem:*", "FileSystem", "write"));
        }
    }
}
