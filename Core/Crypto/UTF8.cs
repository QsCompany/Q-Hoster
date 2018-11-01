using System.Runtime.CompilerServices;
using System.Security;
[module: UnverifiableCode]
public class UTF8
{
    public static int ToArray(string str, byte[] outU8Array, int outIdx, int maxBytesToWrite)
    {
        if (!(maxBytesToWrite > 0)) // Parameter maxBytesToWrite is not optional. Negative values, 0, null, undefined and false each don't write out any bytes.
            return 0;

        var startIdx = outIdx;
        var endIdx = outIdx + maxBytesToWrite - 1; // -1 for string null terminator.
        for (var i = 0; i < str.Length; ++i)
        {
            // Gotcha: charCodeAt returns a 16-bit word that is a UTF-16 encoded code unit, not a Unicode code point of the character! So decode UTF16->UTF32->UTF8.
            // See http://unicode.org/faq/utf_bom.html#utf16-3
            // For UTF8 byte structure, see http://en.wikipedia.org/wiki/UTF-8#Description and https://www.ietf.org/rfc/rfc2279.txt and https://tools.ietf.org/html/rfc3629
            var u = (int)str[i];
            if (u >= 0xD800 && u <= 0xDFFF) u = 0x10000 + ((u & 0x3FF) << 10) | ((int)str[++i] & 0x3FF);
            if (u <= 0x7F)
            {
                if (outIdx >= endIdx) break;
                outU8Array[outIdx++] = (byte)u;
            }
            else if (u <= 0x7FF)
            {
                if (outIdx + 1 >= endIdx) break;
                outU8Array[outIdx++] = (byte)(0xC0 | (u >> 6));
                outU8Array[outIdx++] = (byte)(0x80 | (u & 63));
            }
            else if (u <= 0xFFFF)
            {
                if (outIdx + 2 >= endIdx) break;
                outU8Array[outIdx++] = (byte)(0xE0 | (u >> 12));
                outU8Array[outIdx++] = (byte)(0x80 | ((u >> 6) & 63));
                outU8Array[outIdx++] = (byte)(0x80 | (u & 63));
            }
            else if (u <= 0x1FFFFF)
            {
                if (outIdx + 3 >= endIdx) break;
                outU8Array[outIdx++] = (byte)(0xF0 | (u >> 18));
                outU8Array[outIdx++] = (byte)(0x80 | ((u >> 12) & 63));
                outU8Array[outIdx++] = (byte)(0x80 | ((u >> 6) & 63));
                outU8Array[outIdx++] = (byte)(0x80 | (u & 63));
            }
            else if (u <= 0x3FFFFFF)
            {
                if (outIdx + 4 >= endIdx) break;
                outU8Array[outIdx++] = (byte)(0xF8 | (u >> 24));
                outU8Array[outIdx++] = (byte)(0x80 | ((u >> 18) & 63));
                outU8Array[outIdx++] = (byte)(0x80 | ((u >> 12) & 63));
                outU8Array[outIdx++] = (byte)(0x80 | ((u >> 6) & 63));
                outU8Array[outIdx++] = (byte)(0x80 | (u & 63));
            }
            else
            {
                if (outIdx + 5 >= endIdx) break;
                outU8Array[outIdx++] = (byte)(0xFC | (u >> 30));
                outU8Array[outIdx++] = (byte)(0x80 | ((u >> 24) & 63));
                outU8Array[outIdx++] = (byte)(0x80 | ((u >> 18) & 63));
                outU8Array[outIdx++] = (byte)(0x80 | ((u >> 12) & 63));
                outU8Array[outIdx++] = (byte)(0x80 | ((u >> 6) & 63));
                outU8Array[outIdx++] = (byte)(0x80 | (u & 63));
            }
        }
        // Null-terminate the pointer to the buffer.
        outU8Array[outIdx] = 0;
        return outIdx - startIdx;
    }
    public static int lengthOf(string str)
    {
        var len = 0;
        for (var i = 0; i < str.Length; ++i)
        {
            var u = (int)str[i]; // possibly a lead surrogate
            if (u >= 0xD800 && u <= 0xDFFF) u = 0x10000 + ((u & 0x3FF) << 10) | (str[++i] & 0x3FF);
            if (u <= 0x7F)
            {
                ++len;
            }
            else if (u <= 0x7FF)
            {
                len += 2;
            }
            else if (u <= 0xFFFF)
            {
                len += 3;
            }
            else if (u <= 0x1FFFFF)
            {
                len += 4;
            }
            else if (u <= 0x3FFFFFF)
            {
                len += 5;
            }
            else
            {
                len += 6;
            }
        }
        return len;
    }
    public static string ToString(byte[] u8Array, int idx)
    {
        byte get(int i)
        {
            if (i < u8Array.Length) return u8Array[i];
            return 0;
        }
        var endPtr = idx;
        // TextDecoder needs to know the byte length in advance, it doesn't stop on null terminator by itself.
        // Also, use the length info to avoid running tiny strings through TextDecoder, since .subarray() allocates garbage.
        while (get(endPtr) != 0) ++endPtr;

        {
            int u0, u1, u2, u3, u4, u5;
            var str = "";
            while (true)
            {
                u0 = get(idx++);
                if (u0 == 0) return str;
                if (0 == (u0 & 0x80)) { str += (char)(u0); continue; }
                u1 = (byte)(get(idx++) & 63);
                if ((u0 & 0xE0) == 0xC0) { str += (char)(((u0 & 31) << 6) | u1); continue; }
                u2 = (byte)(get(idx++) & 63);
                if ((u0 & 0xF0) == 0xE0)
                {
                    u0 = (byte)(((u0 & 15) << 12) | (u1 << 6) | u2);
                }
                else
                {
                    u3 = (byte)(get(idx++) & 63);
                    if ((u0 & 0xF8) == 0xF0)
                    {
                        u0 = (byte)(((u0 & 7) << 18) | (u1 << 12) | (u2 << 6) | u3);
                    }
                    else
                    {
                        u4 = (byte)(get(idx++) & 63);
                        if ((u0 & 0xFC) == 0xF8)
                        {
                            u0 = (byte)(((u0 & 3) << 24) | (u1 << 18) | (u2 << 12) | (u3 << 6) | u4);
                        }
                        else
                        {
                            u5 = (byte)(get(idx++) & 63);
                            u0 = (byte)(((u0 & 1) << 30) | (u1 << 24) | (u2 << 18) | (u3 << 12) | (u4 << 6) | u5);
                        }
                    }
                }
                if (u0 < 0x10000)
                {
                    str += (char)u0;
                }
                else
                {
                    var ch = u0 - 0x10000;
                    str += (char)(0xD800 | (ch >> 10)) + (char)(0xDC00 | (ch & 0x3FF));
                }
            }
        }
    }
    public static byte[] GetBytes(string str)
    {
        var l = lengthOf(str) + 1;
        var buffer = new byte[l];
        ToArray(str, buffer, 0, l);
        return buffer;
    }
}