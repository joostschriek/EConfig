using System;
using System.Collections.Generic;
using System.Text;

namespace EConfig.Helpers
{
    public class WrappedValue
    {
        public byte[] EncryptedAESKey { get; set; }
        public byte[] IV { get; set; }
        public byte[] EncryptedValue { get; set; }
        
        public override string ToString() => $"ENC[{Hex.FromByte(EncryptedAESKey)}:{Hex.FromByte(IV)}:{Hex.FromByte(EncryptedValue)}]";
    }
}
