﻿using System;
using System.Linq;
using System.Text;
using EConfig.Helpers;
using NLog.LayoutRenderers.Wrappers;
using Xunit;

namespace EConfig.Tests.Helpers
{
    public class EncryptTests
    {
        // 512 byte key
        private byte[] publicKey = Hex.ToByte("305C300D06092A864886F70D0101010500034B003048024100B26316DB56856573156BC9DD1E93D99D5046ED4B63DAFB8AE3659D566425CF91F525480A633870BC0F7AF47ADB4061A215C95FC437636F9107CAFEB37F207CAD0203010001");

        private byte[] privateKey = Hex.ToByte("30820153020100300D06092A864886F70D01010105000482013D30820139020100024100B26316DB56856573156BC9DD1E93D99D5046ED4B63DAFB8AE3659D566425CF91F525480A633870BC0F7AF47ADB4061A215C95FC437636F9107CAFEB37F207CAD02030100010240056B3D8057E3ABD36FFA506D59FBF0CCE169937F74F4412C3F6B25F94007E6ADC1B5F26B7BD246AD2EF6F47276CAADC392074C0AA731E04E96B53B5FEB0025C1022100FE09D421189CA4EA3240B432F96DDE2BF748C2C7FEB6ACAC7406B102D376DDE1022100B3C3B77378F9794AC34C8B26D34B77520E1A553227D8E8FB34FF821D26D7C04D02207A1A2747B118B97B87A3E9F9064274A3153C77C2C0FEF487FF4CA80FFFAC068102202607ED02D0002F8A02A169FB0FCEB272B8AB178521EB00F74C7215EBE6F0D42D02207E14B5D7BCC3D660ADA560177C53B07A687CC480E54A858268430A3D44CE71A3");

        public class HappyPath : EncryptTests
        {
            [Theory]
            [InlineData("Taylor Swift")]
            [InlineData("Foo Fighters - The Colour And The Shape")]
            [InlineData("Ooh love, ooh loverboy\nWhat're you doin' tonight, hey, boy?\nSet my alarm, turn on my charm\nThat's because I'm a good old-fashioned loverboy")]
            [InlineData("The rain is here And you my dear Are still my friend Its true the two of us Are back as one again I was the one who left you Always coming back I cannot forget you girl Now I am up in arms again The rain is here And you my dear Are still my friend It's true the two of us Are back as one again  I was the one who left you Always coming back I cannot forget you girl Now I am up in arms again Together now I don't know how This love could end My lonely heart It falls apart For you to mend I was the one who left you Always coming back I cannot forget you girl Now I am up in arms again I was the one who left you Always coming back I cannot forget you girl Now I am up in arms again I was the one who left you Always coming back I cannot forget you girl Now I am up in arms again")]
            public void HappyPath_EncryptAndThenDecrypt(string value)
            {
                var e = new Encrypt { PublicKey = publicKey, PrivateKey = privateKey };

                WrappedValue wrapped = e.EncryptAndWrap(value);

                Assert.NotNull(wrapped);
                Assert.NotNull(wrapped.EncryptedAESKey);
                Assert.NotNull(wrapped.IV);
                Assert.NotNull(wrapped.EncryptedValue);

                string wrapHexed = wrapped.ToString();

                Assert.NotNull(wrapHexed);
                Assert.NotEqual(string.Empty, wrapHexed);

                WrappedValue unhexedWrap = null;
                try
                {
                    unhexedWrap = new WrappedValue(wrapHexed);
                }
                catch
                {
                    // no-op
                }

                Assert.NotNull(unhexedWrap);
                Assert.Equal(wrapped.EncryptedValue, unhexedWrap.EncryptedValue);

                string decryptedValue = e.UnwrapAndDecrypt(unhexedWrap);

                Assert.NotNull(decryptedValue);
                Assert.Equal(value, decryptedValue);
            }
        }

        public class RSATests : EncryptTests
        {
            [Fact]
            public void EncryptAndDecrypt_ShouldBeTheSame()
            {
                string superSecret = "JeffProbst_Survivor_is_the_best_show";

                byte[] encyptedSecret = Encrypt.AsymEncryption.Encryptor(publicKey).Encrypt(Encoding.ASCII.GetBytes(superSecret));
                Assert.NotNull(encyptedSecret);

                byte[] decryptedSecretAsBytes = Encrypt.AsymEncryption.Decryptor(privateKey).Decrypt(encyptedSecret);
                Assert.NotNull(decryptedSecretAsBytes);

                string decodedSecret = Encoding.ASCII.GetString(decryptedSecretAsBytes);
                Assert.Equal(decodedSecret, superSecret);
            }
        }

        public class AESTets : EncryptTests { }
    }
}
