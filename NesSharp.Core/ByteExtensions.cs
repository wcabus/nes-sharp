namespace NesSharp.Core;

public static class ByteExtensions
{
    public static byte FlipByte(this byte b)
    {
        // Flips a byte so that 0b00000001 becomes 0b10000000
        // and 0b10000000 becomes 0b00000001
        var result = 0;

        for (var i = 0; i < 8; i++)
        {
            result <<= 1;
            result |= b & 1;
            b >>= 1;
        }

        return (byte)result;
    }
}