using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Jil;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Utilities.IO.Pem;

namespace EConfig.Helpers
{
    public class FileActions
    {
        public virtual Dictionary<string, object> OpenFileFrom(string filename)
        {
            if (!File.Exists(filename))
            {
                return null;
            }

            using (StreamReader configStream = new StreamReader(File.OpenRead(filename)))
            {
                return JSON.Deserialize<Dictionary<string, dynamic>>(configStream);
            }
        }

        public virtual void SaveFileTo(string filename, Dictionary<string, dynamic> config)
        {
            using (var writer = new StreamWriter(filename))
            {
                JSON.SerializeDynamic(config, writer);
            }
        }

        public virtual byte[] ReadPem(string filename = "id_econfig.pem") 
        {
            if (!File.Exists(filename))
            {
                return null;
            }     

            return File.ReadAllBytes(filename); 
        }

        public virtual void WritePem(PrivateKeyInfo privateKeyInfo, string filename = "id_econfig.pem")
        {
            using (var writer = new StreamWriter(filename))
            {
                new PemWriter(writer).WriteObject(new PemObject("RSA PRIVATE KEY", privateKeyInfo.ToAsn1Object().GetDerEncoded()));
            }
        }
    }
}
