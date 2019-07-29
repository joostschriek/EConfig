using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

            Dictionary<string, object> config;
            using (StreamReader configStream = File.OpenText(ConfigFilename))
            {
                var dconfig = JSON.Deserialize<Dictionary<string, dynamic>>(configStream);
//                System.ComponentModel.TypeConverter tc = System.ComponentModel.TypeDescriptor.GetConverter(dconfig);

                // We assume there is a property in at the root of the json that is called public key and holds our public key for writing values.
                var publickKey = FindPublicKey(dconfig);
                if (publickKey == null)
                {
                    logger.Error($"Did not find a public key in {ConfigFilename}. Make sure we can find it in the root with the key publicKey");
                    return 0;
                }

                // Find every string value we can encrypt
                
            }

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

        private List<string> excludedKeys = new List<string> {"PublicKey"};
        private bool FindStringsAndEncrypt(Dictionary<string, dynamic> config)
        {
            foreach (var key in config.Keys)
            {
                if (excludedKeys.Contains(key))
                {
                    continue;
                }

                var v = config[key];
                TypeConverter c = TypeDescriptor.GetConverter(v);

                c.

                if (ShouldEncrypt(v, c))
                {
                    logger.Warn($"should encrypt {key}:{config[key]}");
                }

            }

            return true;
        }

        private bool ShouldEncrypt(dynamic value, TypeConverter c)
        {
            bool should = false;
            

            if (c.CanConvertTo(typeof(int)) || c.CanConvertTo(typeof(double)))
            {
                // no-op
            }
            else if (c.CanConvertToString())
            {
                should = true;
            }

            return should;
        }

        private bool 

        public virtual FileStream OpenFile() => File.Open(ConfigFilename, FileMode.Open, FileAccess.ReadWrite);
    }
}
