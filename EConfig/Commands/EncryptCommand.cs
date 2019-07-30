using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Jil;
using Mono.Options;
using NLog;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using EConfig.Helpers;

using Hex = EConfig.Helpers.Hex;

namespace EConfig.Services
{
    public class EncryptCommand : Command
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public string ConfigFilename { get; set; } = "appsettings.json";
        
        private List<string> excludedKeys = new List<string> { "PublicKey" };
        private byte[] publicKey;
        private Dictionary<string, dynamic> config;
        private Encrypt encrypt;


        public EncryptCommand() : base("encrypt", "encrypts a json file")
        {
            Options = new OptionSet
            {
                {"file", "The file to load and encrypt (defaults to appsettings.json)", s => this.ConfigFilename = s }
                // TODO Add exclusion keys as options
            };

            JSON.SetDefaultOptions(Jil.Options.CamelCase);
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            var opts = Options.Parse(arguments);

            return Encrypt();
        }

        private int Encrypt()
        {
            this.config = OpenConfig();

            // We assume there is a property in at the root of the json that is called public key and holds our public key for writing values.
            publicKey = FindPublicKey(config);
            if (publicKey == null)
            {
                logger.Error($"Did not find a public key in {ConfigFilename}. Make sure we can find it in the root with the key publicKey");
                return 0;
            }
            this.encrypt = new Encrypt(publicKey);
            FindStringsAndEncryptByKeys(config.Keys.ToList());
            
            SaveConfig(config);

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

        private void FindStringsAndEncryptByKeys(List<string> keys)
        {
            foreach (var key in keys)
            {
                if (excludedKeys.Contains(key))
                {
                    continue;
                }

                var v = config[key];
                TypeConverter c = TypeDescriptor.GetConverter(v);

                if (c.CanConvertTo(typeof(IDictionary<string, dynamic>)))
                {
                    var subConfig = (Dictionary<string, dynamic>) c.ConvertTo(v, typeof(IDictionary<string, dynamic>));
                    FindStringsAndEncryptByKeys(subConfig.Keys.ToList());
                }

                if (ShouldEncrypt(v, c))
                {
                    logger.Warn($"Encrypting {key}:{v}");
                    var wrap = this.encrypt.EncryptAndWrap((string) v);
                    config[key] = wrap.ToString();
                    logger.Warn($"{key} is now {config[key]}");
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

        public virtual Dictionary<string, object> OpenConfig()
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
