using System.Collections.Generic;
using System.Security.Cryptography;
using Mono.Options;
using NLog;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using static EConfig.Helpers;

namespace EConfig.Services
{
    public class KeyCommand : Command
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private bool load = false;

        public KeyCommand() : base("key", "Key generation and loading. Will wirte a new key to the output ")
        {
            Options = new OptionSet
            {
                {"load", "Verifies and makes the key findable for the later operations.", s => load = true }
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            var opts = Options.Parse(arguments);

            GenerateAndWriteKeys();

            return 1;
        }

        private static void GenerateAndWriteKeys()
        {
            var keypair = GenerateKeypair();

            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keypair.Private);
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keypair.Public);

            logger.Info($"public: {FromByteToHex(publicKeyInfo.ToAsn1Object().GetDerEncoded())}");
            logger.Info($"private: {FromByteToHex(privateKeyInfo.ToAsn1Object().GetDerEncoded())}");
        }

        private static AsymmetricCipherKeyPair GenerateKeypair()
        {
            byte[] seed = new byte[16];
            RNGCryptoServiceProvider a = new RNGCryptoServiceProvider();
            a.GetNonZeroBytes(seed);

            var secureRandom = new SecureRandom();
            secureRandom.SetSeed(seed);

            RsaKeyPairGenerator rsaGenny = new RsaKeyPairGenerator();
            rsaGenny.Init(new KeyGenerationParameters(secureRandom, 32));

            return rsaGenny.GenerateKeyPair();
        }

    }
}
