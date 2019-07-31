using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Jil;

namespace EConfig.Helpers
{
    public class FileActions
    {
        public virtual Dictionary<string, object> OpenFileFrom(string filename)
        {
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
    }
}
