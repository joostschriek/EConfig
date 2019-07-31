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
        public ConfigWalker Walker { get; set; } = new ConfigWalker();

        public string configFilename = "appsettings.json";
        private Dictionary<string, dynamic> config;
        private byte[] privateKey;
        private Encrypt encrypt;

        public DecryptCommand() : base("decrypt", "decrypt a json file")
        {
            Options = new OptionSet
            {
                {"file|f", "The file to load and encrypt (defaults to appsettings.json)", s => configFilename = s}
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

            privateKey = FindPrivateKey();
            if (privateKey == null)
            {
                logger.Error($"Did not find a public key in {configFilename}. Make sure we can find it in the root with the key publicKey");
                return 0;
            }
            
            encrypt = new Encrypt(privateKeyBytes: privateKey);
            Walker.Action = DecryptIf;
            Walker.FindStringValueByKeys(config.Keys.ToList(), config);

            FileActions.SaveFileTo(configFilename, config);

            return 1;
        }

        private bool DecryptIf(Dictionary<string, object> currenttree, string key, object value, TypeConverter converter)
        {
            throw new System.NotImplementedException();
        }

        private byte[] FindPrivateKey()
        {

            return new byte[16];
        }
    }
}
