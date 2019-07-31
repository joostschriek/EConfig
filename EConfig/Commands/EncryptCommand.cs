using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using EConfig.Helpers;
using Jil;
using Mono.Options;
using NLog;
using Hex = EConfig.Helpers.Hex;

namespace EConfig.Commands
{
    public class EncryptCommand : Command
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public List<string> ExcludedKeys { get; } = new List<string> {"PublicKey"};
        public string ConfigFilename { get; set; } = "appsettings.json";

        public FileActions FileActions { get; set; } = new FileActions();

        private Dictionary<string, dynamic> config;
        private byte[] publicKey;
        private Encrypt encrypt;

        public EncryptCommand() : base("encrypt", "encrypts a json file")
        {
            Options = new OptionSet
            {
                {"file", "The file to load and encrypt (defaults to appsettings.json)", s => this.ConfigFilename = s }
                // TODO Add exclusion keys as options
                // TODO Add ability to rename the "PublickKey" key
            };

            JSON.SetDefaultOptions(Jil.Options.CamelCase);
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            Options.Parse(arguments);

            return Encrypt();
        }

        private int Encrypt()
        {
            config = FileActions.OpenFileFrom(ConfigFilename);

            // We assume there is a property in at the root of the json that is called public key and holds our public key for writing values.
            publicKey = FindPublicKey(config);
            if (publicKey == null)
            {
                logger.Error($"Did not find a public key in {ConfigFilename}. Make sure we can find it in the root with the key publicKey");
                return 0;
            }
            encrypt = new Encrypt(publicKey);
            FindStringsAndEncryptByKeys(config.Keys.ToList(), config);
            
            FileActions.SaveFileTo(ConfigFilename, config);

            return 1;
        }

        private byte[] FindPublicKey(Dictionary<string, dynamic> config)
        {
            byte[] publikcKey = null;
            if (config.ContainsKey("PublicKey"))
            {
                publikcKey = Hex.ToByte((string) config["PublicKey"]);
            }

            return publikcKey;
        }

        private bool FindStringsAndEncryptByKeys(List<string> keys, Dictionary<string, dynamic> currentTree)
        {
            bool didSomething = false;
            foreach (var key in keys)
            {
                if (ExcludedKeys.Contains(key))
                {
                    continue;
                }

                var v = currentTree[key];
                TypeConverter c = TypeDescriptor.GetConverter(v);

                if (c.CanConvertTo(typeof(IDictionary<string, dynamic>)))
                {
                    // This looks weird, but to edit the config object we need to not loop thru config itself (this would break the 
                    // loop when we edit something). So we keep track with a copy of the keys collection. But we also need to be able
                    // to set value in sub keys. hence we keep track of the current branch of the config tree we are in.
                    var treeToFollow = (Dictionary<string, dynamic>) c.ConvertTo(currentTree[key], typeof(IDictionary<string, dynamic>));
                    if (FindStringsAndEncryptByKeys(treeToFollow.Keys.ToList(), treeToFollow))
                    {
                        currentTree[key] = treeToFollow;
                    }

                    continue;
                }

                if (ShouldEncrypt(v, c))
                {
                    logger.Warn($"Encrypting {key}:{v}");
                    var wrap = this.encrypt.EncryptAndWrap((string) v);
                    currentTree[key] = wrap.ToString();
                    logger.Warn($"{key} is now {currentTree[key]}");

                    didSomething = true;
                }
            }

            return didSomething;
        }

        private bool ShouldEncrypt(dynamic value, TypeConverter c)
        {
            bool should = false;
            if (c.CanConvertTo(typeof(int)) || c.CanConvertTo(typeof(double)) || c.CanConvertTo(typeof(bool)))
            {
                // no-op, but we want to say no to these types
            }
            else if (c.CanConvertTo(typeof(string)))
            {
                var valueAsString = (string) c.ConvertTo(value, typeof(string));
                should = !valueAsString.StartsWith("enc", StringComparison.InvariantCultureIgnoreCase);
            }

            return should;
        }
    }
}
