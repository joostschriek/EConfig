using System.Collections.Generic;
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


namespace EConfig.Services
{
    public class KeyCommand : Command
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private bool load = false;
        private int keySize = 128;

        public KeyCommand() : base("key", "Key generation and loading. Will wirte a new key to the output ")
        {
            Options = new OptionSet
            {
                {"load", "Verifies and makes the key findable for the later operations.", s => load = true },
                {"size|s=", "Allows you to specify the key size (deafult to 128)", (int v) => keySize = v }
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            var opts = Options.Parse(arguments);

            GenerateAndWriteKeys();

            return 1;
        }

        private void GenerateAndWriteKeys()
        {
            var keypair = GenerateKeypair();

            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keypair.Private);
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keypair.Public);

            logger.Info($"public: {Hex.FromByte(publicKeyInfo.ToAsn1Object().GetDerEncoded())}");
            logger.Info($"private: {Hex.FromByte(privateKeyInfo.ToAsn1Object().GetDerEncoded())}");
        }

        private AsymmetricCipherKeyPair GenerateKeypair()
        {
            byte[] seed = new byte[16];
            RNGCryptoServiceProvider a = new RNGCryptoServiceProvider();
            a.GetNonZeroBytes(seed);

            var secureRandom = new SecureRandom();
            secureRandom.SetSeed(seed);

            RsaKeyPairGenerator rsaGenny = new RsaKeyPairGenerator();
            rsaGenny.Init(new KeyGenerationParameters(secureRandom, this.keySize));

            return rsaGenny.GenerateKeyPair();
        }

    }
}
