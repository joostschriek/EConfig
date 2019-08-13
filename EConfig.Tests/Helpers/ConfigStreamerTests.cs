using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using EConfig.Helpers;
using Jil;
using Xunit;

namespace EConfig.Tests.Helpers
{
    public class ConfigStreamerTests
    {
        [Fact]
        public void WhenStreamingSubSubTrees_ShouldIndicateThatSomethingChanged() 
        {
            var streamer = new ConfigStreamer();
            streamer.Action = (Dictionary<string, dynamic> currentTree, string key, dynamic value, TypeConverter converter) => {
                return key.Equals("bigsubtree");
            };
            var fullTree = JSON.Deserialize<Dictionary<string, dynamic>>("{ \"making\" : { \"a\": { \"bigsubtree\" : \"not ecrypted yet!\" } } }");
            var changed = streamer.FindStringValueByKeys(fullTree.Keys.ToList(), fullTree);

            Assert.True(changed);
        }
    }
}
