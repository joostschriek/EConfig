using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Jil;
using Mono.Options;
using NLog;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using static EConfig.Helpers;

namespace EConfig.Services
{
    public class EncryptionCommand : Command
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private List<string> excludedKeys = new List<string> { "PublicKey" };

        public string ConfigFilename { get; set; } = "appsettings.json";


        public EncryptionCommand(string name, string help = null) : base(name, help)
        {
            Options = new OptionSet
            {
                {"file", "The file to load and encrypt (defaults to appsettings.json)", s => this.ConfigFilename = s }
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            var opts = Options.Parse(arguments);

            return Encrypt();
        }

        private int Encrypt()
        {
            Dictionary<string, dynamic> config = OpenConfig();

            // We assume there is a property in at the root of the json that is called public key and holds our public key for writing values.
            var publicKey = FindPublicKey(config);
            if (publicKey == null)
            {
                logger.Error($"Did not find a public key in {ConfigFilename}. Make sure we can find it in the root with the key publicKey");
                return 0;
            }
            
            FindStringsAndEncrypt(config);
            
            SaveConfig(config);

            return 1;
        }

        private AsymmetricKeyParameter FindPublicKey(Dictionary<string, dynamic> config)
        {
            AsymmetricKeyParameter publikcKey = null;
            if (config.ContainsKey("PublicKey"))
            {
                var bytesPublickey = FromHexToByte((string) config["PublicKey"]);
                publikcKey = PublicKeyFactory.CreateKey(bytesPublickey);
            }

            return publikcKey;
        }

        private void FindStringsAndEncrypt(Dictionary<string, dynamic> config)
        {
            foreach (var key in config.Keys)
            {
                if (excludedKeys.Contains(key))
                {
                    continue;
                }

                var v = config[key];
                TypeConverter c = TypeDescriptor.GetConverter(v);

                if (c.CanConvertTo(typeof(IDictionary<string, dynamic>)))
                {
                    FindStringsAndEncrypt((Dictionary<string, dynamic>) c.ConvertTo(v, typeof(IDictionary<string, dynamic>)));
                }

                if (ShouldEncrypt(v, c))
                {
                    logger.Warn($"should encrypt {key}:{config[key]}");
                }
            }
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
                should = true;
            }

            return should;
        }

        public virtual Dictionary<string, dynamic> OpenConfig()
        {
            using (StreamReader configStream = new StreamReader(File.OpenRead(ConfigFilename)))
            {
                return JSON.Deserialize<Dictionary<string, dynamic>>(configStream);
            }
        }

        public virtual void SaveConfig(Dictionary<string, dynamic> config)
        {
            using (var writer = new StreamWriter(ConfigFilename))
            {
                JSON.Serialize(config, writer);
            }
        }
    }
}
