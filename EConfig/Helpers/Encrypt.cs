using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NLog;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

namespace EConfig.Helpers
{
    public class Encrypt
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private SecureRandom secureRandom = new SecureRandom();

        public byte[] PublicKey { get; set; }
        public byte[] PrivateKey { get; set; }
        public int SymetricKeySize { get; set; } = 128 / 8;

        public WrappedValue EncryptAndWrap(string value)
        {
            var valueAsBytes = Encoding.UTF8.GetBytes(value);

            // generate AES key and iv
            byte[] key = new byte[SymetricKeySize], iv = new byte[16];
            secureRandom.NextBytes(key);
            secureRandom.NextBytes(iv);

            // encrypt all the things
            var encryptedKey = AsymEncryption.Encryptor(PublicKey).Encrypt(key);
            var encryptedValue = SymEncryption.GetWith(key, iv).Encrypt(valueAsBytes);

            return new WrappedValue
            {
                EncryptedAESKey = encryptedKey,
                IV = iv,
                EncryptedValue = encryptedValue
            };
        }

        public string UnwrapAndDecrypt(WrappedValue wrap)
        {
            var key = AsymEncryption.Decryptor(PrivateKey).Decrypt(wrap.EncryptedAESKey);
            var value = SymEncryption.GetWith(key, wrap.IV).Decrypt(wrap.EncryptedValue);

            return Encoding.UTF8.GetString(value);
        }

        // RSA
        public class AsymEncryption
        {
            public ICipherParameters PublicKey { get; set; }
            public ICipherParameters PrivateKey { get; set; }


            public static AsymEncryption Encryptor(byte[] publicKey)
            {
                return new AsymEncryption
                {
                    PublicKey = PublicKeyFactory.CreateKey(publicKey)
                };
            }

            public static AsymEncryption Decryptor(byte[] privateKey)
            {
                return new AsymEncryption
                {
                    PrivateKey = PrivateKeyFactory.CreateKey(privateKey)
                };
            }

            public byte[] Encrypt(byte[] data)
            {
                Pkcs1Encoding rsa_bc = new Pkcs1Encoding(new RsaEngine());
                rsa_bc.Init(true, PublicKey);

                return rsa_bc.ProcessBlock(data, 0, data.Length);
            }

            public byte[] Decrypt(byte[] encryptedData)
            {
                Pkcs1Encoding rsa_bc = new Pkcs1Encoding(new RsaEngine());
                rsa_bc.Init(false, PrivateKey);

                return rsa_bc.ProcessBlock(encryptedData, 0, encryptedData.Length);
            }
        }

        public class SymEncryption
        {
            private SecureRandom secureRandom = new SecureRandom();

            private byte[] symKey;
            private byte[] iv;
            
            private SymEncryption(byte[] symmetricKey, byte[] iv)
            {
                symKey = symmetricKey;
                this.iv = iv;
            }

            public static SymEncryption GetWith(byte[] key, byte[] iv)
            {
                return new SymEncryption(key, iv);
            }

            public byte[] Encrypt(byte[] data)
            {
                using (var aes = GetAESManaged())
                {
                    return aes.CreateEncryptor().TransformFinalBlock(data, 0, data.Length);
                }
            }

            public byte[] Decrypt(byte[] encyptedData)
            {
                using (var aes = GetAESManaged())
                {
                    return aes.CreateDecryptor().TransformFinalBlock(encyptedData, 0, encyptedData.Length);
                }
            }

            private AesManaged GetAESManaged()
            {
                var a = new AesManaged();
                a.Key = symKey;
                a.Mode = CipherMode.CBC;
                a.IV = iv;
                a.Padding = PaddingMode.PKCS7;

                return a;
            }
        }
    }
}
