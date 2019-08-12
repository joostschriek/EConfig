using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using EConfig.Commands;
using EConfig.Helpers;
using Jil;
using Mono.Options;
using NLog;
using NLog.Config;
using NLog.Targets;
using NSubstitute;
using Xunit;

namespace EConfig.Tests.Commands
{
    public class EncryptCommandTests
    {
        public EncryptCommand command = Substitute.ForPartsOf<EncryptCommand>();

        private Dictionary<string, dynamic> savedConfig;

        private static string publicKey = "\"PublicKey\": \"305C300D06092A864886F70D0101010500034B003048024100B26316DB56856573156BC9DD1E93D99D5046ED4B63DAFB8AE3659D566425CF91F525480A633870BC0F7AF47ADB4061A215C95FC437636F9107CAFEB37F207CAD0203010001\",";

        private Dictionary<string, dynamic> basic = JSON.Deserialize<Dictionary<string, dynamic>>("{ " + publicKey + " \"Name\": \"joost\" }"),
            withSubConfigs = JSON.Deserialize<Dictionary<string, dynamic>>("{ " + publicKey + " \"SubName\": { \"AnotherName\": \"Hanan\", \"Name\": \"Joost\" } }"),
            differentTypes = JSON.Deserialize<Dictionary<string, dynamic>>("{ " + publicKey + " \"isBool\": true, \"isList\": [ \"Hanan\", \"Mark\", \"Yas\"], \"isNumber\": 2 }");

        public EncryptCommandTests()
        {
            command.FileActions = BuildFileActions(basic);
        }

        [Fact]
        public void HappyPath_Encrypt()
        {
            var ret = command.Invoke(new string[] { });
                
            Assert.Equal(1, ret);
            Assert.NotNull(savedConfig);
            Assert.Contains("Name", savedConfig.Keys.ToList());
            Assert.Contains("PublicKey", savedConfig.Keys.ToList());
            Assert.Equal("305C300D06092A864886F70D0101010500034B003048024100B26316DB56856573156BC9DD1E93D99D5046ED4B63DAFB8AE3659D566425CF91F525480A633870BC0F7AF47ADB4061A215C95FC437636F9107CAFEB37F207CAD0203010001", (string)savedConfig["PublicKey"]);
            Assert.StartsWith("ENC[", (string) savedConfig["Name"]);
            Assert.False(((string) savedConfig["Name"]).Contains("joost", StringComparison.InvariantCultureIgnoreCase), 
            "name should not contain the original value anymore");
        }

        [Fact]
        public void HappyPath_ShouldNotTryToReencrypt()
        {
            basic["Name"] = "ENC[something";

            command.Invoke(new string[] { });

            Assert.NotNull(savedConfig);
            Assert.Equal("ENC[something", savedConfig["Name"]);
        }

        [Fact]
        public void HappyPath_ShouldWalkConfigTree()
        {

            command.FileActions = BuildFileActions(withSubConfigs);
            command.Invoke(new string[] { });
            
            Assert.NotNull(savedConfig["SubName"]["AnotherName"]);
            Assert.StartsWith("ENC[", (string) savedConfig["SubName"]["AnotherName"]);
        }

        [Fact]
        public void HappyPath_ExcludesKeys()
        {
            command.FileActions = BuildFileActions(withSubConfigs);
            command.KeysToExclude = new[] { "Name" };
            command.Invoke(new string[] { });

            Assert.Equal("Joost", (string) savedConfig["SubName"]["Name"]);
        }

        // TODO only encrypt string

        private FileActions BuildFileActions(Dictionary<string, dynamic> toReturn)
        {
            var file = Substitute.For<FileActions>();
            file.OpenFileFrom(Arg.Any<string>()).Returns(toReturn);
            file.SaveFileTo(Arg.Any<string>(), Arg.Do<Dictionary<string, dynamic>>(arg => savedConfig = arg));

            return file;
        }
    }
}