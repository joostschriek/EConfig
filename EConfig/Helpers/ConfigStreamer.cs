using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EConfig.Helpers
{
    public class ConfigStreamer
    {
        public List<string> ExcludedKeys { get; } = new List<string> { "PublicKey" };

        public delegate bool CryptoAction(Dictionary<string, dynamic> currentTree, string key, dynamic value, TypeConverter converter);

        public CryptoAction Action { get; set; }

        public bool FindStringValueByKeys(List<string> keys, Dictionary<string, dynamic> currentTree)
        {
            bool didSomething = false;
            foreach (var key in keys)
            {
                if (ExcludedKeys.Contains(key))
                {
                    continue;
                }

                var value = currentTree[key];
                TypeConverter converter = TypeDescriptor.GetConverter(value);

                if (converter.CanConvertTo(typeof(IDictionary<string, dynamic>)))
                {
                    // This looks weird, but to edit the config object we need to not loop thru config itself (this would break the 
                    // loop when we edit something). So we keep track with a copy of the keys collection. But we also need to be able
                    // to set value in sub keys. hence we keep track of the current branch of the config tree we are in.
                    var treeToFollow = (Dictionary<string, dynamic>)converter.ConvertTo(currentTree[key], typeof(IDictionary<string, dynamic>));
                    if (FindStringValueByKeys(treeToFollow.Keys.ToList(), treeToFollow))
                    {
                        currentTree[key] = treeToFollow;
                        didSomething = true;
                    }

                    continue;
                }

                if (Action(currentTree, key, value, converter))
                {
                    didSomething = true;
                }
            }

            return didSomething;
        }
    }
}
