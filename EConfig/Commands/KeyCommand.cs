using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using EConfig.Helpers;
using Mono.Options;
using NLog;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace EConfig.Commands
{
    public class KeyCommand : Command
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public FileActions File { get; set; } = new FileActions();

        private bool load, pem;
        private int keySize = 512;
        private string hexedPrivateKey;

        public KeyCommand() : base("key", "Key generation and loading. Will write a new key to the output ")
        {
            Options = new OptionSet
            {
                {"load=", "Verifies and makes the key findable for the later operations.", key => { load = true; hexedPrivateKey = key; } },
                {"size=|s=", "Allows you to specify the key size (defaults to 512)", (int v) => keySize = v },
                {"pem", "Write the file to disk as id_econfig.pem", _ => pem = true }
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            Options.Parse(arguments);

            if (load)
            {
                LoadPrivateKeyFromHexed(hexedPrivateKey);
            }
            else
            {
                GenerateAndWriteKeys();
            }

            return 1;
        }

        public  void GenerateAndWriteKeys()
        {
            var keypair = GenerateKeypair();

            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keypair.Private);
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keypair.Public);

            string publicKey = Hex.FromByte(publicKeyInfo.ToAsn1Object().GetDerEncoded()),
                privateKey = Hex.FromByte(privateKeyInfo.ToAsn1Object().GetDerEncoded());

            logger.Info($"public: {publicKey}");
            logger.Info($"private: {privateKey}");

            if (pem)
            {
                File.WritePem(privateKeyInfo);
                logger.Info("Wrote private key to disk.");
            }
        }

        public void LoadPrivateKeyFromHexed(string privateKeyAsHex)
        {
            var privateKey = PrivateKeyFactory.CreateKey(Hex.ToByte(privateKeyAsHex));
            var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKey);

            File.WritePem(privateKeyInfo);
            logger.Info($"Loaded key to {Directory.GetCurrentDirectory()}\\id_econfig.pem.");
        }

        private AsymmetricCipherKeyPair GenerateKeypair()
        {
            byte[] seed = new byte[16];
            RNGCryptoServiceProvider a = new RNGCryptoServiceProvider();
            a.GetNonZeroBytes(seed);

            var secureRandom = new SecureRandom();
            secureRandom.SetSeed(seed);

            RsaKeyPairGenerator rsaGenny = new RsaKeyPairGenerator();
            rsaGenny.Init(new KeyGenerationParameters(secureRandom, keySize));

            return rsaGenny.GenerateKeyPair();
        }
    }
}
