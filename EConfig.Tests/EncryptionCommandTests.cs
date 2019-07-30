using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using EConfig.Services;
using NSubstitute;
using Xunit;

namespace EConfig.Tests
{
    public class EncryptionCommandTests
    {
        public EncryptionCommand command = Substitute.ForPartsOf<EncryptionCommand>("encrypt", "");

        public EncryptionCommandTests()
        {
            command.WhenForAnyArgs(c => c.OpenConfig()).DoNotCallBase();
            command.WhenForAnyArgs(c => c.SaveConfig(Arg.Any<Dictionary<string, dynamic>>())).DoNotCallBase();
        }

        [Fact]
        public void HappyPath()
        {
            Dictionary<string, dynamic> savedConfig;
            command.OpenConfig().Returns(new Dictionary<string, dynamic>
            {
                {"PublicKey", "something"},
                {"Name", "joost"}
            });
            command.SaveConfig(Arg.Do<Dictionary<string, dynamic>>(arg => savedConfig = arg));

            var ret = command.Invoke(new string[] { });

            Assert.Equal(1, ret);
        }
    }
}