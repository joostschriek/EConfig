using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Jil;
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

        private static string configFilename = "appsettings.json";

        public static void Main(string[] args)
        {
            bool load = false;

            var keyOptions = new OptionSet
            {
                {"load", "Verifies and makes the key findable for the later operations.", s => load = true}

            };
            var encryptOptions = new OptionSet
            {
                {"file", "The file to load and encrypt (defaults to appsettings.json)", s => configFilename = s }
            };

            var cmds = new CommandSet("EncryptedConfig")
            {
                new Command("key", "Key generation and loading. Will wirte a new key to the output ")
                {  
                    Options = keyOptions,
                    Run = pargs => { GenerateAndWriteKeys() }
                },
                new Command("encrypt")
                {
                    Options = encryptOptions,
                    Run = Encrypt
                }
            };
            
            cmds.Run(args);
        }
        

        private static void Encrypt(IEnumerable<string> pargs)
        {
            // We assume there is a property in at the root of the json that is called publickKey and holds our public key for writing values.
            var publickKey = FindPublicKey(configFilename);
        }


        private static AsymmetricKeyParameter FindPublicKey(string filename)
        {
            AsymmetricKeyParameter publikcKey = null;

            Dictionary<string, object> config;
            using (var configStream = File.OpenText(filename))
            {
                config = JSON.Deserialize<Dictionary<string, object>>(configStream);
            }

            string hexedPublicKey;
            if (config.ContainsKey("publicKey"))
            {
                var bytesPublickey = FromHexToByte(config["publicKey"] as string);
                publikcKey = PublicKeyFactory.CreateKey(bytesPublickey);
            }

            return publikcKey;
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
