using System;
using System.Collections.Generic;
using System.Text;
using EConfig.Helpers;
using Org.BouncyCastle.Security;
using Xunit;

namespace EConfig.Tests.Helpers
{
    public class WrappedValueTests
    {
        [Fact]
        public void HappyPath()
        {
            byte[] key = new byte[16], iv = new byte[16], value = new byte[16];
            SecureRandom random = new SecureRandom();
            random.NextBytes(key);
            random.NextBytes(iv);
            random.NextBytes(value);

            var wrappedString = new WrappedValue { EncryptedAESKey = key, IV = iv, EncryptedValue = value }.ToString();
            Assert.StartsWith("ENC[", wrappedString);

            var wrap = new WrappedValue(wrappedString);
            Assert.Equal(key, wrap.EncryptedAESKey);
            Assert.Equal(iv, wrap.IV);
            Assert.Equal(value, wrap.EncryptedValue);
        }
    }
}
