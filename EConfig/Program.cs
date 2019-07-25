using System;
using System.Linq;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Mono.Options;
using NLog;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;

namespace EConfig
{
    class Program
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        
        public static void Main(string[] args)
        {
            bool generateKeys = false;

            var cmds = new CommandSet("EConfig")
            {
                "usage: <command> [<args>]",
                

                new Command("generate", "Writes a new public and private key to the console")
                {
                    Options = new OptionSet
                    {
                        {"pem", "Outputs the private key to the console in pem format.", v => logger.Info("PEM")}
                    }
                },
                new Command("load", "Loads a new key from the given path"),
            };



            cmds.Run(args);
        }

        private static void GenerateAndWriteKeys()
        {
            var keypair = GenerateKeypair();

            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keypair.Private);
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keypair.Public);
            
            logger.Info($"public: {FromByteToHex(publicKeyInfo.ToAsn1Object().GetDerEncoded())}");
            logger.Info($"private: {FromByteToHex(privateKeyInfo.ToAsn1Object().GetDerEncoded())}");
        }

        private static byte[] FromHexToByte(string hextacular) => Enumerable
                .Range(0, hextacular.Length)
                .Where(i => i % 2 == 0)
                .Select(i => Convert.ToByte(hextacular.Substring(i, 2), 16))
                .ToArray();

        private static string FromByteToHex(byte[] biting) => BitConverter.ToString(biting).Replace("-", string.Empty);
        
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
