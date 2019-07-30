using System;
using System.Linq;

namespace EConfig.Helpers
{
    public class Hex
    {
        public static byte[] ToByte(string hextacular) => Enumerable
            .Range(0, hextacular.Length)
            .Where(i => i % 2 == 0)
            .Select(i => Convert.ToByte(hextacular.Substring(i, 2), 16))
            .ToArray();

        public static string FromByte(byte[] biting) => BitConverter.ToString(biting).Replace("-", string.Empty);
    }
}
