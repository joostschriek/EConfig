using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using EConfig.Helpers;
using Mono.Options;
using NLog;

namespace EConfig.Commands
{
    public class DecryptCommand : Command
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public FileActions FileActions { get; set; } = new FileActions();
        public ConfigStreamer Walker { get; set; } = new ConfigStreamer();

        public string configFilename = "appsettings.json";
        private Dictionary<string, dynamic> config;
        private byte[] privateKey, publicKey;
        private Encrypt encrypt;

        public DecryptCommand() : base("decrypt", "decrypt a json file")
        {
            Options = new OptionSet
            {
                {"file=|f=", "The file to load and encrypt (defaults to appsettings.json)", s => configFilename = s}
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

            publicKey = FindPublicKey(config);
            privateKey = FindPrivateKey();
            if (privateKey == null || publicKey == null)
            {
                logger.Error($"Did not find a public key or a private key in {configFilename} or the key dir. Make sure we have the public key in the config file, and the private key as a PEM in the key dir.");
                return 0;
            }

            using (new Stopwatch("Decrypting config"))
            {
                encrypt = new Encrypt { PublicKey = publicKey, PrivateKey = privateKey };
                Walker.Action = DecryptIf;
                Walker.FindStringValueByKeys(config.Keys.ToList(), config);
            }

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

        private byte[] FindPrivateKey() => FileActions.ReadPem();

        private bool DecryptIf(Dictionary<string, object> currenttree, string key, dynamic value, TypeConverter converter)
        {
            var didSomething = false;
            if (ShouldTryDecryption(value, converter))
            {
                // TODO properly some try-catch-all error handling to prevent nasty blowups. Nothing fancy. 
                var wrap = new WrappedValue((string)value);
                var unwrappedValue = this.encrypt.UnwrapAndDecrypt(wrap);
                currenttree[key] = unwrappedValue;
                didSomething = true;
            }

            return didSomething;
        }

        private bool ShouldTryDecryption(dynamic value, TypeConverter converter)
        {
            bool should = false;
            if (converter.CanConvertTo(typeof(int)) || converter.CanConvertTo(typeof(double)) || converter.CanConvertTo(typeof(bool)))
            {
                // no-op, but we want to say no to these types
            }
            else if (converter.CanConvertTo(typeof(string)))
            {
                // Don't re-encrypt a setting
                var valueAsString = (string)converter.ConvertTo(value, typeof(string));
                should = valueAsString.StartsWith("enc", StringComparison.InvariantCultureIgnoreCase);
            }

            return should;
        }
    }
}
