using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using EConfig.Services;
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
using static EConfig.Helpers;


namespace EConfig
{
    class Program
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            

            var cmds = new CommandSet("EncryptedConfig")
            {
                new KeyCommand(),
                new EncryptionCommand("encrypt")
            };
            
            cmds.Run(args);
        }
    }
}
