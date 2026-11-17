// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using Zeron.ZCore.Utils;

namespace Zeron.ZCore.Utils.Tests
{
    /// <summary>
    /// SecureKeyStorageTests
    /// </summary>
    [TestClass()]
    public class SecureKeyStorageTests
    {
        /// <summary>
        /// ResolveConfiguredSecretPlaintextPassthroughTest test.
        /// </summary>
        [TestMethod()]
        public void ResolveConfiguredSecretPlaintextPassthroughTest()
        {
            Assert.AreEqual("zeron.testkey", SecureKeyStorage.ResolveConfiguredSecret("zeron.testkey"));
        }

        /// <summary>
        /// ResolveConfiguredSecretEnvPrefixTest test.
        /// </summary>
        [TestMethod()]
        public void ResolveConfiguredSecretEnvPrefixTest()
        {
            const string varName = "ZERON_TEST_SECRET_RESOLVE";
            string? previous = Environment.GetEnvironmentVariable(varName);

            try
            {
                Environment.SetEnvironmentVariable(varName, "from-env");
                Assert.AreEqual("from-env", SecureKeyStorage.ResolveConfiguredSecret("env:" + varName));
            }
            finally
            {
                Environment.SetEnvironmentVariable(varName, previous);
            }
        }

        /// <summary>
        /// ResolveConfiguredSecretFilePrefixTest test.
        /// </summary>
        [TestMethod()]
        public void ResolveConfiguredSecretFilePrefixTest()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), "zeron-secret-" + Guid.NewGuid().ToString("N") + ".key");

            try
            {
                File.WriteAllText(tempFile, "  file-secret  \n");
                Assert.AreEqual("file-secret", SecureKeyStorage.ResolveConfiguredSecret("file:" + tempFile));
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        /// <summary>
        /// ResolveConfiguredSecretRelativeFileUnderBaseDirectoryTest test.
        /// </summary>
        [TestMethod()]
        public void ResolveConfiguredSecretRelativeFileUnderBaseDirectoryTest()
        {
            string fileName = "zeron-test-secret-" + Guid.NewGuid().ToString("N") + ".key";
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

            try
            {
                File.WriteAllText(fullPath, "rel-secret");
                Assert.AreEqual("rel-secret", SecureKeyStorage.ResolveConfiguredSecret("file:" + fileName));
            }
            finally
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
        }

        /// <summary>
        /// ResolveConfiguredSecretDpapiThrowsOnNonWindowsTest test.
        /// </summary>
        [TestMethod()]
        public void ResolveConfiguredSecretDpapiThrowsOnNonWindowsTest()
        {
            if (OperatingSystem.IsWindows())
            {
                Assert.Inconclusive("dpapi: resolution is Windows-specific.");
            }

            Assert.ThrowsExactly<PlatformNotSupportedException>(() =>
                SecureKeyStorage.ResolveConfiguredSecret("dpapi:AAAA"));
        }
    }
}