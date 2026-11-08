// Zeron - Scheduled Task Application for Windows OS
// Copyright (c) 2019 Jiowcl. All rights reserved.

using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Zeron.ZCore.Utils.Tests
{
    /// <summary>
    /// EncryptionProviderTests
    /// </summary>
    [TestClass()]
    public class EncryptionProviderTests
    {
        /// <summary>
        /// Cleanup test.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            EncryptionProvider.ResetToDefaults();
        }

        /// <summary>
        /// Encrypt test.
        /// </summary>
        [TestMethod()]
        public void EncryptTest()
        {
            string userNamePayload = EncryptionProvider.Encrypt("Ji-Feng Tsai");

            Assert.IsNotNull(userNamePayload);
            Assert.IsTrue(userNamePayload.Length > 0);
        }

        /// <summary>
        /// EncryptDecryptRoundTripTest test.
        /// </summary>
        [TestMethod()]
        public void EncryptDecryptRoundTripTest()
        {
            const string plainText = "Ji-Feng Tsai";

            string cipherText = EncryptionProvider.Encrypt(plainText);
            bool decrypted = EncryptionProvider.TryDecrypt(cipherText, out string? result);

            Assert.IsTrue(decrypted);
            Assert.AreEqual(plainText, result);
        }

        /// <summary>
        /// TryDecryptInvalidCipherTest test.
        /// </summary>
        [TestMethod()]
        public void TryDecryptInvalidCipherTest()
        {
            bool decrypted = EncryptionProvider.TryDecrypt("not-valid-base64!!!", out string? result);

            Assert.IsFalse(decrypted);
            Assert.IsNull(result);
        }

        /// <summary>
        /// ConfigureCustomKeysRoundTripTest test.
        /// </summary>
        [TestMethod()]
        public void ConfigureCustomKeysRoundTripTest()
        {
            EncryptionProvider.Configure("prod-salt-key-A", "prod-iv-key-ABCDEF");
            const string plainText = "zeron.prod.key";

            string cipherText = EncryptionProvider.Encrypt(plainText);
            bool decrypted = EncryptionProvider.TryDecrypt(cipherText, out string? result);

            Assert.AreEqual("prod-salt-key-A", EncryptionProvider.SaltKey);
            Assert.AreEqual("prod-iv-key-ABCDEF", EncryptionProvider.IvKey);
            Assert.IsTrue(decrypted);
            Assert.AreEqual(plainText, result);
        }

        /// <summary>
        /// ConfigureKeysMismatchCannotDecryptTest test.
        /// </summary>
        [TestMethod()]
        public void ConfigureKeysMismatchCannotDecryptTest()
        {
            EncryptionProvider.Configure("salt-one", "iv-source-one!");
            string cipherText = EncryptionProvider.Encrypt("secret-payload");

            EncryptionProvider.Configure("salt-two", "iv-source-two!");
            bool decrypted = EncryptionProvider.TryDecrypt(cipherText, out string? result);

            Assert.IsFalse(decrypted);
            Assert.IsNull(result);
        }

        /// <summary>
        /// ConfigureIgnoresEmptyValuesTest test.
        /// </summary>
        [TestMethod()]
        public void ConfigureIgnoresEmptyValuesTest()
        {
            EncryptionProvider.Configure("keep-salt", "keep-iv-value!!");
            EncryptionProvider.Configure("", "   ");

            Assert.AreEqual("keep-salt", EncryptionProvider.SaltKey);
            Assert.AreEqual("keep-iv-value!!", EncryptionProvider.IvKey);
        }

        /// <summary>
        /// ConfigureFromEnvironmentOverridesTest test.
        /// </summary>
        [TestMethod()]
        public void ConfigureFromEnvironmentOverridesTest()
        {
            string? previousSalt = Environment.GetEnvironmentVariable(EncryptionProvider.EnvSaltKey);
            string? previousIv = Environment.GetEnvironmentVariable(EncryptionProvider.EnvIvKey);

            try
            {
                Environment.SetEnvironmentVariable(EncryptionProvider.EnvSaltKey, "env-salt-material");
                Environment.SetEnvironmentVariable(EncryptionProvider.EnvIvKey, "env-iv-material!!");

                EncryptionProvider.ResetToDefaults();
                EncryptionProvider.ConfigureFromEnvironment();

                Assert.AreEqual("env-salt-material", EncryptionProvider.SaltKey);
                Assert.AreEqual("env-iv-material!!", EncryptionProvider.IvKey);

                string cipher = EncryptionProvider.Encrypt("env-roundtrip");
                Assert.IsTrue(EncryptionProvider.TryDecrypt(cipher, out string? plain));
                Assert.AreEqual("env-roundtrip", plain);
            }
            finally
            {
                Environment.SetEnvironmentVariable(EncryptionProvider.EnvSaltKey, previousSalt);
                Environment.SetEnvironmentVariable(EncryptionProvider.EnvIvKey, previousIv);
            }
        }

        /// <summary>
        /// ResetToDefaultsRestoresLegacyKeysTest test.
        /// </summary>
        [TestMethod()]
        public void ResetToDefaultsRestoresLegacyKeysTest()
        {
            EncryptionProvider.Configure("custom", "custom-iv-value");
            EncryptionProvider.ResetToDefaults();

            Assert.AreEqual(EncryptionProvider.DefaultSaltKey, EncryptionProvider.SaltKey);
            Assert.AreEqual(EncryptionProvider.DefaultIvKey, EncryptionProvider.IvKey);
        }

        /// <summary>
        /// InvokeDeriveKeyBytes test.
        /// </summary>
        private static byte[] InvokeDeriveKeyBytes(string source)
        {
            MethodInfo? method = typeof(EncryptionProvider)
                .GetMethod("DeriveKeyBytes", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(method);

            object? result = method.Invoke(null, [source]);
            Assert.IsNotNull(result);

            return (byte[])result;
        }

        /// <summary>
        /// DeriveKeyBytes_WhenSourceIsExactly32Bytes_ReturnsOriginalUtf8Bytes test.
        /// </summary>
        [TestMethod]
        public void DeriveKeyBytes_WhenSourceIsExactly32Bytes_ReturnsOriginalUtf8Bytes()
        {
            const string source = "12345678901234567890123456789012";
            byte[] expected = Encoding.UTF8.GetBytes(source);

            byte[] actual = InvokeDeriveKeyBytes(source);

            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        /// DeriveKeyBytes_WhenSourceIsNot32Bytes_ReturnsSha256Hash test.
        /// </summary>
        [TestMethod]
        public void DeriveKeyBytes_WhenSourceIsNot32Bytes_ReturnsSha256Hash()
        {
            const string source = "hello-world";
            byte[] expected = SHA256.HashData(Encoding.UTF8.GetBytes(source));

            byte[] actual = InvokeDeriveKeyBytes(source);

            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        /// DeriveKeyBytes_WhenSourceIsEmpty_ReturnsSha256HashOfEmptyBytes test.
        /// </summary>
        [TestMethod]
        public void DeriveKeyBytes_WhenSourceIsEmpty_ReturnsSha256HashOfEmptyBytes()
        {
            byte[] expected = SHA256.HashData(Encoding.UTF8.GetBytes(string.Empty));

            byte[] actual = InvokeDeriveKeyBytes(string.Empty);

            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        /// InvokeDeriveIvBytes test.
        /// </summary>
        private static byte[] InvokeDeriveIvBytes(string source)
        {
            MethodInfo? method = typeof(EncryptionProvider)
                .GetMethod("DeriveIvBytes", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(method);

            object? result = method.Invoke(null, [source]);
            Assert.IsNotNull(result);

            return (byte[])result;
        }

        /// <summary>
        /// DeriveIvBytes_WhenSourceIsExactly16Bytes_ReturnsOriginalUtf8Bytes test.
        /// </summary>
        [TestMethod]
        public void DeriveIvBytes_WhenSourceIsExactly16Bytes_ReturnsOriginalUtf8Bytes()
        {
            const string source = "1234567890123456";
            byte[] expected = Encoding.UTF8.GetBytes(source);

            byte[] actual = InvokeDeriveIvBytes(source);

            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        /// DeriveIvBytes_WhenSourceIsNot16Bytes_ReturnsFirst16BytesOfSha256Hash test.
        /// </summary>
        [TestMethod]
        public void DeriveIvBytes_WhenSourceIsNot16Bytes_ReturnsFirst16BytesOfSha256Hash()
        {
            const string source = "hello-world";
            byte[] expected = new byte[16];
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(source));

            Array.Copy(hash, expected, 16);

            byte[] actual = InvokeDeriveIvBytes(source);

            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        /// DeriveIvBytes_WhenSourceIsEmpty_ReturnsFirst16BytesOfSha256Hash test.
        /// </summary>
        [TestMethod]
        public void DeriveIvBytes_WhenSourceIsEmpty_ReturnsFirst16BytesOfSha256Hash()
        {
            byte[] expected = new byte[16];
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(string.Empty));

            Array.Copy(hash, expected, 16);

            byte[] actual = InvokeDeriveIvBytes(string.Empty);

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
