using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using EConfig.Services;
using NSubstitute;
using NSubstitute.Extensions;
using Xunit;
using Enumerable = System.Linq.Enumerable;

namespace EConfig.Tests
{
    public class EncryptCommandTests
    {
        public EncryptCommand command = Substitute.ForPartsOf<EncryptCommand>();

        public EncryptCommandTests()
        {
            command.WhenForAnyArgs(c => c.OpenConfig()).DoNotCallBase();
            command.WhenForAnyArgs(c => c.SaveConfig(Arg.Any<Dictionary<string, dynamic>>())).DoNotCallBase();
        }

        [Fact]
        public void HappyPath()
        {
            Dictionary<string, dynamic> savedConfig = null;
            command.OpenConfig().ReturnsForAnyArgs(new Dictionary<string, object>
            {
                {"PublicKey", "305C300D06092A864886F70D0101010500034B003048024100B26316DB56856573156BC9DD1E93D99D5046ED4B63DAFB8AE3659D566425CF91F525480A633870BC0F7AF47ADB4061A215C95FC437636F9107CAFEB37F207CAD0203010001"},
                {"Name", "joost"}
            });
            command.SaveConfig(Arg.Do<Dictionary<string, dynamic>>(arg => savedConfig = arg));

            var ret = command.Invoke(new string[] { });
                
            Assert.Equal(1, ret);
            Assert.NotNull(savedConfig);
            Assert.Contains("Name", savedConfig.Keys.ToList());
            Assert.Contains("PublicKey", savedConfig.Keys.ToList());
            Assert.Equal(savedConfig["PublicKey"], "305C300D06092A864886F70D0101010500034B003048024100B26316DB56856573156BC9DD1E93D99D5046ED4B63DAFB8AE3659D566425CF91F525480A633870BC0F7AF47ADB4061A215C95FC437636F9107CAFEB37F207CAD0203010001");
            Assert.StartsWith("ENC[", savedConfig["Name"]);
            Assert.False(((string)savedConfig["Name"]).Contains("joost", StringComparison.InvariantCultureIgnoreCase), 
            "name should not contain the original value anymore");
        }
    }
}