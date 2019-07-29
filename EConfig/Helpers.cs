using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EConfig
{
    public class Helpers
    {
        public static byte[] FromHexToByte(string hextacular) => Enumerable
            .Range(0, hextacular.Length)
            .Where(i => i % 2 == 0)
            .Select(i => Convert.ToByte(hextacular.Substring(i, 2), 16))
            .ToArray();

        public static string FromByteToHex(byte[] biting) => BitConverter.ToString(biting).Replace("-", string.Empty);
    }
}
