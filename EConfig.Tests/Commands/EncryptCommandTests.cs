using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EConfig.Commands;
using EConfig.Helpers;
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

        private Dictionary<string, object> basic = new Dictionary<string, object>
            {
                {
                    "PublicKey",
                    "305C300D06092A864886F70D0101010500034B003048024100B26316DB56856573156BC9DD1E93D99D5046ED4B63DAFB8AE3659D566425CF91F525480A633870BC0F7AF47ADB4061A215C95FC437636F9107CAFEB37F207CAD0203010001"
                },
                {"Name", "joost"}
            },
            withSubConfigs = new Dictionary<string, object>
            {
                {
                    "PublicKey",
                    "305C300D06092A864886F70D0101010500034B003048024100B26316DB56856573156BC9DD1E93D99D5046ED4B63DAFB8AE3659D566425CF91F525480A633870BC0F7AF47ADB4061A215C95FC437636F9107CAFEB37F207CAD0203010001"
                },
                {"Name", "joost"},
                {
                    "SubName", new Dictionary<string, dynamic>
                    {
                        {"AnotherName", "Hanan"}
                    }
                }
            },
            differentTypes = new Dictionary<string, object>
            {
                {"isSet", true},
                {"isList", new List<string> {"Hanan", "Mark", "Yas"}},
                {"name", "AndreiC"},
                {"isNubmer", 2},
                {
                    "PublicKey",
                    " 305C300D06092A864886F70D0101010500034B00304802410084C1909EA0B18BC61153D03141A16B935131492FED388F2BF2D612BF5A82BA661F02EC736EBB79115BBAF8742ACA982DC5AE182CEBB4AEDCD1F86B54E0E357810203010001"
                }
            };


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
            Assert.Equal(savedConfig["PublicKey"], "305C300D06092A864886F70D0101010500034B003048024100B26316DB56856573156BC9DD1E93D99D5046ED4B63DAFB8AE3659D566425CF91F525480A633870BC0F7AF47ADB4061A215C95FC437636F9107CAFEB37F207CAD0203010001");
            Assert.StartsWith("ENC[", savedConfig["Name"]);
            Assert.False(((string)savedConfig["Name"]).Contains("joost", StringComparison.InvariantCultureIgnoreCase), 
            "name should not contain the original value anymore");
        }

        [Fact]
        public void HappyPath_EncryptThenDecrypt()
        {

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
            
            Assert.StartsWith("ENC[", savedConfig["SubName"]["AnotherName"]);
        }

        // TODO only encrypt string

        private FileActions BuildFileActions(Dictionary<string, object> toReturn)
        {
            var file = Substitute.For<FileActions>();
            file.OpenFileFrom(Arg.Any<string>()).Returns(toReturn);
            file.SaveFileTo(Arg.Any<string>(), Arg.Do<Dictionary<string, dynamic>>(arg => savedConfig = arg));

            return file;
        }
    }
}