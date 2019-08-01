using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using EConfig.Helpers;
using Mono.Options;
using NLog;
using Hex = EConfig.Helpers.Hex;

namespace EConfig.Commands
{
    public class EncryptCommand : Command
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public FileActions FileActions { get; set; } = new FileActions();
        public ConfigWalker Walker { get; set; } = new ConfigWalker();
        
        private string configFilename = "appsettings.json";
        private Dictionary<string, dynamic> config;
        private byte[] publicKey;
        private Encrypt encrypt;

        public EncryptCommand() : base("encrypt", "encrypts a json file")
        {
            Options = new OptionSet
            {
                {"file=|f=", "The file to load and encrypt (defaults to appsettings.json)", s => this.configFilename = s }
                // TODO Add exclusion keys as options
                // TODO Add ability to rename the "PublickKey" key
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            Options.Parse(arguments);

            config = FileActions.OpenFileFrom(configFilename);
            if (config == null)
            {
                logger.Error($"Did not find or cold not access the file \"{configFilename}\".");
                return 0;
            }

            // We assume there is a property in at the root of the json that is called public key and holds our public key for writing values.
            publicKey = FindPublicKey(config);
            if (publicKey == null)
            {
                logger.Error($"Did not find a public key in {configFilename}. Make sure we can find it in the root with the key publicKey");
                return 0;
            }

            encrypt = new Encrypt { PublicKey = publicKey };
            Walker.Action = EncryptIf;
            Walker.FindStringValueByKeys(config.Keys.ToList(), config);

            FileActions.SaveFileTo(configFilename, config);

            return 1;
        }
        
        private byte[] FindPublicKey(Dictionary<string, dynamic> config)
        {
            byte[] publikcKey = null;
            if (config.ContainsKey("PublicKey"))
            {
                publikcKey = Hex.ToByte((string)config["PublicKey"]);
            }

            return publikcKey;
        }

        private bool EncryptIf(Dictionary<string, dynamic> currentTree, string key, dynamic value, TypeConverter converter)
        {
            var didSomething = false;
            if (ShouldEncrypt(value, converter))
            {
                // TODO properly some try-catch-all error handling to prevent nasty blowups. Nothing fancy. 
                logger.Warn($"Encrypting {key}:{value}");
                var wrap = this.encrypt.EncryptAndWrap((string) value);
                currentTree[key] = wrap.ToString();
                logger.Warn($"{key} is now {currentTree[key]}");

                didSomething = true;
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
                // Don't re-encrypt a setting
                var valueAsString = (string) c.ConvertTo(value, typeof(string));
                should = !valueAsString.StartsWith("enc", StringComparison.InvariantCultureIgnoreCase);
            }

            return should;
        }
    }
}
