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

        public byte[] PublicKey { get; private set; }
        public int SymetricKeySize { get; set; } = 128 / 8;


        public Encrypt(byte[] publicKeyBytes)
        {
            PublicKey = publicKeyBytes;
        }

        public WrappedValue EncryptAndWrap(string value)
        {
            var valueAsBytes = Encoding.UTF8.GetBytes(value);

            // generate AES key and iv
            byte[] key = new byte[SymetricKeySize], iv = new byte[16];
            secureRandom.NextBytes(key);
            secureRandom.NextBytes(iv);

            // encrypt all the things
            var encryptedValue = SymEncryption.GetWith(key, iv).Encrypt(valueAsBytes);
            var encryptedKey = AsymEncryption.Encryptor(PublicKey).Encrypt(key);
            
            return new WrappedValue
            {
                EncryptedAESKey = encryptedKey,
                IV = iv,
                EncryptedValue = encryptedValue
            };
        }

        public string UnwrapAndDecrypt(WrappedValue wrap)
        {
            return string.Empty;
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
                    byte[] paddedData = PadRandom(data, aes.BlockSize);
                    logger.Trace("encrypted padded data length=" + paddedData.Length);
                    logger.Trace("encrypted padded data=" + BitConverter.ToString(paddedData));

                    byte[] encryptedData = aes.CreateEncryptor().TransformFinalBlock(paddedData, 0, paddedData.Length);
                    logger.Trace("encrypted data length=" + encryptedData.Length);
                    logger.Trace("encrypted data=" +  BitConverter.ToString(encryptedData));

                    return encryptedData;
                }
            }

            public byte[] Decrypt(byte[] encyptedData)
            {
                byte[] paddedData = null;
                using (var aes = GetAESManaged())
                {
                    paddedData = aes.CreateDecryptor().TransformFinalBlock(encyptedData, 0, encyptedData.Length);
                }

                logger.Trace("Sym decrypted content=" + paddedData);
                byte[] data = null;
                using (Asn1InputStream asn1InputStream = new Asn1InputStream(paddedData))
                {
                    Asn1Object ob = asn1InputStream.ReadObject();
                    if (ob is Asn1OctetString)
                    {
                        data = ((Asn1OctetString)ob).GetOctets();
                    }
                    else
                    {
                        throw new IOException(string.Format("Unexpected data structure:{0}", ob.GetType()));
                    }
                }

                logger.Trace("Unpadded content=" + data);
                return data;
            }

            private AesManaged GetAESManaged()
            {
                var a = new AesManaged();
                a.Key = symKey;
                a.Mode = CipherMode.CBC;
                a.IV = iv;
                a.Padding = PaddingMode.None;

                return a;
            }

            private byte[] PadRandom(byte[] data, int blocksize)
            {
                int bytesToPad = CalculatePaddingSize(data.Length, blocksize, false);
                if (bytesToPad <= 0)
                {
                    return (byte[])data.Clone();
                }
                byte[] rndIntAsBytes = new byte[1];
                byte[] paddedData = new byte[data.Length + bytesToPad];
                Buffer.BlockCopy(data, 0, paddedData, 0, data.Length);
                for (int i = 0; i < bytesToPad; i++)
                {
                    secureRandom.NextBytes(rndIntAsBytes);
                    paddedData[paddedData.Length - 1 - i] = rndIntAsBytes[0];
                }
                return paddedData;
            }

            private int CalculatePaddingSize(int dataLength, int blocksize, bool manditory)
            {
                if (dataLength == 0)
                {
                    return blocksize;
                }
                int times = (int)Math.Ceiling(dataLength / (blocksize * 1.0));
                int neededPadding = blocksize * times - dataLength;
                return manditory && neededPadding > 0 ? blocksize : neededPadding;
            }
        }
    }
}
