using System.IO;
using EConfig.Services;
using NSubstitute;
using Xunit;

namespace EConfig.Tests
{
    public class EncryptionCommandTests
    {
        public EncryptionCommand command = Substitute.ForPartsOf<EncryptionCommand>("encrypt", "");

        [Theory]
        [InlineFileStream(new [] { "appsettings.json" })]
        public void HappyPath(FileStream config)
        {
            command.OpenFile().Returns(config);

            var ret = command.Invoke(new string[] { });

            Assert.Equal(ret, 1);
        }
    }
}