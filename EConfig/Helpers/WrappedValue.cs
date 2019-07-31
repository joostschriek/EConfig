using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EConfig.Helpers
{
    public class WrappedValue
    {
        public byte[] EncryptedAESKey { get; set; }
        public byte[] IV { get; set; }
        public byte[] EncryptedValue { get; set; }

        public WrappedValue() { }
        public WrappedValue(string value)
        {
            var stripped = value.Substring(0, value.Length - 1).Substring(4).Split(':');

            EncryptedAESKey = Hex.ToByte(stripped[0]);
            IV = Hex.ToByte(stripped[1]);
            EncryptedValue = Hex.ToByte(stripped[2]);
        }

        public override string ToString() => $"ENC[{Hex.FromByte(EncryptedAESKey)}:{Hex.FromByte(IV)}:{Hex.FromByte(EncryptedValue)}]";
    }
}
