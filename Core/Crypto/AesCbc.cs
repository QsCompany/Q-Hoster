using System;
using System.Globalization;
using System.Text;
using Server;
using System.Diagnostics;
using static Dyn;
namespace QServer
{
    public static class JScript
    {
        
        private static readonly byte[] aray = new byte[0];
        
        public static  int parseInt( string s){
            return int.Parse(s,NumberStyles.HexNumber);
        }
        public static string ToArrayString(byte[] b)
        {
            return string.Join(",", b);
        }
        public static int parseInt(this string s, int radix)
        {
        
            switch (radix)
            {
                case 16:
                        s = s.ToUpper();
                        if (s.StartsWith("0X")) s = s.Substring(2);
                    var b = 1;
                    var r = 0;
                    for (int i = s.Length - 1; i >= 0; i--)
                    {
                        var c = (int)s[i];
                        if (c >= 65 && c <= 70)
                            c = c - 55;
                        else if (c >= 48 && c <= 57)
                            c = c - 48;
                        else break;
                        r += b * c;
                        b *= 16;
                    }
                    return r;
                case 10:                    
                     b = 1;
                     r = 0;
                    for (int i = s.Length - 1; i >= 0; i--)
                    {
                        var c = (int)s[i];
                        if (c >= 48 && c <= 57)
                            c = c - 48;
                        else break;
                        r += b * c;
                        b *= 10;
                    }
                    return r;
                case 64:
                default:
                    break;
            }
            throw null;
        }
        public static byte[] slice(this byte[] b, int s, int e = -1)
        {
            if (e == -1) e = b.Length;
            var x = new byte[e - s];
            Array.Copy(b, s, x, 0, x.Length);
            return x;
        }
        public static byte[] expand(this byte[] b, int l)
        {
            if (l < b.Length) return b;
            if (l == b.Length) return b;
            var x = new byte[l];            
            Array.Copy(b, 0, x, 0, Math.Min(b.Length, l));
            return x;
        }
        public static byte[] concat(this byte[] b, byte[] c)
        {
            b = b ?? aray;
            c = c ?? aray;
            var x = new byte[b.Length + c.Length];
            Array.Copy(b, x, b.Length);
            Array.Copy(c, 0, x, b.Length, c.Length);
            return x;
        }
        public static byte[] clone(this byte[] b)
        {
            return (byte[])(b ?? aray).Clone();
        }

        public static char charAt(this string b, int i)
        {
            if (i >= b.Length || i < 0) return '\0';
            return b[i];
        }
        public static int indexOf(this string b, char c,int s=0)
        {
            return b.IndexOf(c, s);
        }
        public static string substring(this string b, int s, int length)
        {
            return b.Substring(s, length);
        }
    }

    public class  Aes
    {
        public static byte[] Sbox = { 99, 124, 119, 123, 242, 107, 111, 197, 48, 1, 103, 43, 254, 215, 171, 118, 202, 130, 201, 125, 250, 89, 71, 240, 173, 212, 162, 175, 156, 164, 114, 192, 183, 253, 147, 38, 54, 63, 247, 204, 52, 165, 229, 241, 113, 216, 49, 21, 4, 199, 35, 195, 24, 150, 5, 154, 7, 18, 128, 226, 235, 39, 178, 117, 9, 131, 44, 26, 27, 110, 90, 160, 82, 59, 214, 179, 41, 227, 47, 132, 83, 209, 0, 237, 32, 252, 177, 91, 106, 203, 190, 57, 74, 76, 88, 207, 208, 239, 170, 251, 67, 77, 51, 133, 69, 249, 2, 127, 80, 60, 159, 168, 81, 163, 64, 143, 146, 157, 56, 245, 188, 182, 218, 33, 16, 255, 243, 210, 205, 12, 19, 236, 95, 151, 68, 23, 196, 167, 126, 61, 100, 93, 25, 115, 96, 129, 79, 220, 34, 42, 144, 136, 70, 238, 184, 20, 222, 94, 11, 219, 224, 50, 58, 10, 73, 6, 36, 92, 194, 211, 172, 98, 145, 149, 228, 121, 231, 200, 55, 109, 141, 213, 78, 169, 108, 86, 244, 234, 101, 122, 174, 8, 186, 120, 37, 46, 28, 166, 180, 198, 232, 221, 116, 31, 75, 189, 139, 138, 112, 62, 181, 102, 72, 3, 246, 14, 97, 53, 87, 185, 134, 193, 29, 158, 225, 248, 152, 17, 105, 217, 142, 148, 155, 30, 135, 233, 206, 85, 40, 223, 140, 161, 137, 13, 191, 230, 66, 104, 65, 153, 45, 15, 176, 84, 187, 22 };
        public static byte[] ShiftRowTab = { 0, 5, 10, 15, 4, 9, 14, 3, 8, 13, 2, 7, 12, 1, 6, 11 }, ShiftRowTab_Inv;
        public static byte[] Sbox_Inv, xtime;
        static Aes()
        {
            Sbox_Inv = new byte[256];
            byte b = 0;
            do
            {
                Sbox_Inv[Sbox[b]] = b;
                b++;
            } while (b != 0);
            //for (b = 0; b <= 255; b++)
            //    Sbox_Inv[Sbox[b]] = b;
            ShiftRowTab_Inv = new byte[16];
            for (b = 0; b < 16; b++)
                ShiftRowTab_Inv[ShiftRowTab[b]] = b;
            xtime = new byte[256];
            for (b = 0; b < 128; b++)
            {
                xtime[b] = (byte)(b << 1);
                xtime[128 + b] = (byte)(b << 1 ^ 27);
            }
        }
        protected byte[] key;

        public byte[] Key
        {
            set => key = InitKey(value);
        }
        public Aes(byte[] key)
        {
            this.key = InitKey(key);
        }

        protected virtual byte[] InitKey(byte[] key)
        {
            return key;
        }

        public static byte[] ExpandKey(byte[] b)
        {
            int c = b.Length, d, e = 1;
            switch (c)
            {
                case 176:
                case 208:
                case 240:
                    return b;
                case 16:
                    d = 176;
                    break;
                case 24:
                    d = 208;
                    break;
                case 32:
                    d = 240;
                    break;
                default:
                    throw new Exception("my.ExpandKey: Only key lengths of 16, 24 or 32 bytes allowed!");
            }
            for (var g = c; g < d; g += 4)
            {
                var h = b.slice(g - 4, g);
                if (g % c == 0)
                {
                    h = new[] { (byte)(Sbox[h[1]] ^ e), Sbox[h[2]], Sbox[h[3]], Sbox[h[0]] };
                    if ((e <<= 1) >= 256)
                        e ^= 283;
                }
                else
                    if (c > 24 && g % c == 16)
                        h = new[] { Sbox[h[0]], Sbox[h[1]], Sbox[h[2]], Sbox[h[3]] };
                b = b.expand(g + 4);
                for (var f = 0; f < 4; f++)
                    b[g + f] = (byte)(b[g + f - c] ^ h[f]);
            }
            return b;
        }

        //public static void SubBytes(byte[] a, byte[] c)
        //{
        //    for (var d = 0; d < 16; d++)
        //        a[d] = c[a[d]];
        //}
        public static void SubBytes(byte[] a, byte[] c)
        {
            for (var d = 0; d < 16; d++)
                a[d] = c[a[d]];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a">Of length 16</param>
        /// <param name="c">Of length 16</param>
        public static void AddRoundKey(byte[] a, byte[] c)
        {
            for (var d = 0; d < 16; d++)
                a[d] ^= c[d];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a">Of length 16</param>
        /// <param name="c">Of length 16+s</param>
        /// <param name="s"></param>
        public static void AddRoundKey(byte[] a, byte[] c, int s)
        {
            for (var d = 0; d < 16; d++)
                a[d] ^= c[d + s];
        }
        public static void ShiftRows(byte[] a, byte[] c)
        {
            var d = (byte[])a.Clone();//.slice(0, 16);//.clone();
            for (var e = 0; e < 16; e++)
                a[e] = d[c[e]];
        }

        public static void ShiftRows(byte[] a, byte[] c, int s)
        {
            var d = a.clone();
            for (var e = 0; e < 16; e++)
                a[e] = d[c[e + s]];
        }

        public static void MixColumns(byte[] b)
        {
            for (var c = 0; c < 16; c += 4)
            {
                byte d = b[c + 0]
                  , e = b[c + 1]
                  , g = b[c + 2]
                  , h = b[c + 3]
                  , f = (byte)(d ^ e ^ g ^ h);
                b[c + 0] ^= (byte)(f ^ xtime[d ^ e]);
                b[c + 1] ^= (byte)(f ^ xtime[e ^ g]);
                b[c + 2] ^= (byte)(f ^ xtime[g ^ h]);
                b[c + 3] ^= (byte)(f ^ xtime[h ^ d]);
            }
        }
        public static void MixColumns_Inv(byte[] b)
        {
            for (var c = 0; c < 16; c += 4)
            {
                byte d = b[c + 0]
                  , e = b[c + 1]
                  , g = b[c + 2]
                  , h = b[c + 3]
                  , f = (byte)(d ^ e ^ g ^ h)
                  , o = xtime[f]
                  , p = (byte)(xtime[xtime[o ^ d ^ g]] ^ f);
                f ^= xtime[xtime[o ^ e ^ h]];
                b[c + 0] ^= (byte)(p ^ xtime[d ^ e]);
                b[c + 1] ^= (byte)(f ^ xtime[e ^ g]);
                b[c + 2] ^= (byte)(p ^ xtime[g ^ h]);
                b[c + 3] ^= (byte)(f ^ xtime[h ^ d]);
            }
        }

        public virtual byte[] Encrypt(byte[] b)
        {
            //b = b.clone();
            var d = key.Length;
            AddRoundKey(b, key, 0);
            var e = 16;
            for (; e < d - 16; e += 16)
            {
                SubBytes(b, Sbox);
                ShiftRows(b, ShiftRowTab);
                MixColumns(b);
                AddRoundKey(b, key, e);//.slice(e, e + 16));
            }
            SubBytes(b, Sbox);
            ShiftRows(b, ShiftRowTab);
            AddRoundKey(b, key, e);//.slice(e, d));
            return b;
        }
        public virtual byte[] Decrypt(byte[] b)
        {
            var c = key;
            var d = c.Length;
            AddRoundKey(b, c, d - 16);
            ShiftRows(b, ShiftRowTab_Inv);
            SubBytes(b, Sbox_Inv);
            for (d -= 32; d >= 16; d -= 16)
            {
                AddRoundKey(b, c, d);
                MixColumns_Inv(b);
                ShiftRows(b, ShiftRowTab_Inv);
                SubBytes(b, Sbox_Inv);
            }
            AddRoundKey(b, c, 0);
            return b;
        }
    }
    public class  AesCBC:Aes
    {
        public AesCBC(byte[] key):base(key)
        {
        }
        protected override byte[] InitKey(byte[] key)
        {
            return ExpandKey(key);
        }
        public static byte[] string2bytes(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }
        public static string bytes2string(byte[] s)
        {
            return Encoding.UTF8.GetString(s);
        }
        public static byte[] blockXOR(byte[] a, byte[] c)
        {
            var d = new byte[16];
            for (var e = 0; e < 16; e++)
                d[e] = (byte)(a[e] ^ c[e]);
            return d;
        }
        public static byte[] blockXOR(byte[] a, int @as, byte[] c, int cs)
        {
            var d = new byte[16];
            for (var e = 0; e < 16; e++)
                d[e] = (byte)(a[e + @as] ^ c[e + cs]);
            return d;
        }
        public static byte[] blockIV()
        {
            var c = new byte[16];
            SecureRandom.NextBytes(c);
            return c;
        }
        public static byte[] pad16(byte[] a)
        {
            return a.expand(a.Length + (16 - a.Length % 16) % 16);
        }
        public static byte[] depad(byte[] a)
        {
            var i = a.Length - 1;
            for (; i >= 0; i--)
                if (a[i] != 0) break;
            return a.slice(0, i + 1);
        }

        public override byte[] Encrypt(byte[] data)
        {
            data = pad16(data);
            var igg = new byte[data.Length + 16];
            blockIV().CopyTo(igg, 0);
            var i0 = 0;
            for (var h = 0; h < data.Length / 16; h++)
            {
                var f = blockXOR(igg, i0, data, i0);
                base.Encrypt(f);
                f.CopyTo(igg, i0 = i0 + 16);
            }
            return igg;
        }
        public string Encrypt(string str)
        {
            var data = Encoding.UTF8.GetBytes(str);
            var igg = Encrypt(data);
            return UTF8Encoding.UTF8.GetString(AES.ToBase64(igg));
        }
        public string Decrypt(string str)
        {
            var data = AES.FromBase64(Encoding.UTF8.GetBytes(str));
            var igg = Decrypt(data);
            return UTF8Encoding.UTF8.GetString(igg);
        }

        public override  byte[] Decrypt(byte[] data)
        {
            byte[] g = new byte[0];
            var i0 = 0;
            var i1 = 16;
            var i2 = 32;
            for (var h = 1; h < data.Length / 16; h++)
            {
                var f = data.slice(i1, i2);
                base.Decrypt(f);
                g = g.concat(blockXOR(data, i0, f, 0));
                i0 = i1;
                i1 = i2;
                i2 += 16;
            }
            g = depad(g);
            return g;
        }

    }
}

public static class Dyn
{
    [DebuggerNonUserCode]
    public static void Resize(ref byte[] data, int len)
    {
        var t = new byte[len];
        Array.Copy(data, 0, t, 0, Math.Min(len, data.Length));
        data = t;
    }
    [DebuggerNonUserCode]
    public static byte[] slice(this byte[] data, int start, int? end=null)
    {
        if (end == null) end = data.Length;
        var t = new byte[(int)end - start];
        Array.Copy(data, start, t, 0, Math.Min(data.Length - start, t.Length));
        return t;
    }
    [DebuggerNonUserCode]
    public static byte[] push(ref byte[] data, byte b)
    {
        Resize(ref data, data.Length + 1);
        data[data.Length - 1] = b;
        return data;
    }
    [DebuggerNonUserCode]
    public static byte[] concat(this byte[] data,byte[] data1)
    {
        var t = new byte[data.Length + data1.Length];
        Array.Copy(data, t, data.Length);
        Array.Copy(data1, 0, t, data.Length, data1.Length);
        return t;
    }
}
namespace Crypto
{
    interface ICrypto
    {
        byte[] Encrypt(byte[] data);
        byte[] Decrypt(byte[] data);
    }
    class Aes : ICrypto
    {
        static byte[] Sbox = new byte[] { 99, 124, 119, 123, 242, 107, 111, 197, 48, 1, 103, 43, 254, 215, 171, 118, 202, 130, 201, 125, 250, 89, 71, 240, 173, 212, 162, 175, 156, 164, 114, 192, 183, 253, 147, 38, 54, 63, 247, 204, 52, 165, 229, 241, 113, 216, 49, 21, 4, 199, 35, 195, 24, 150, 5, 154, 7, 18, 128, 226, 235, 39, 178, 117, 9, 131, 44, 26, 27, 110, 90, 160, 82, 59, 214, 179, 41, 227, 47, 132, 83, 209, 0, 237, 32, 252, 177, 91, 106, 203, 190, 57, 74, 76, 88, 207, 208, 239, 170, 251, 67, 77, 51, 133, 69, 249, 2, 127, 80, 60, 159, 168, 81, 163, 64, 143, 146, 157, 56, 245, 188, 182, 218, 33, 16, 255, 243, 210, 205, 12, 19, 236, 95, 151, 68, 23, 196, 167, 126, 61, 100, 93, 25, 115, 96, 129, 79, 220, 34, 42, 144, 136, 70, 238, 184, 20, 222, 94, 11, 219, 224, 50, 58, 10, 73, 6, 36, 92, 194, 211, 172, 98, 145, 149, 228, 121, 231, 200, 55, 109, 141, 213, 78, 169, 108, 86, 244, 234, 101, 122, 174, 8, 186, 120, 37, 46, 28, 166, 180, 198, 232, 221, 116, 31, 75, 189, 139, 138, 112, 62, 181, 102, 72, 3, 246, 14, 97, 53, 87, 185, 134, 193, 29, 158, 225, 248, 152, 17, 105, 217, 142, 148, 155, 30, 135, 233, 206, 85, 40, 223, 140, 161, 137, 13, 191, 230, 66, 104, 65, 153, 45, 15, 176, 84, 187, 22 };
        static byte[] ShiftRowTab = new byte[] { 0, 5, 10, 15, 4, 9, 14, 3, 8, 13, 2, 7, 12, 1, 6, 11 };
        static byte[] ShiftRowTab_Inv;
        static byte[] Sbox_Inv, xtime;
        public byte[] Key;
        static Aes()
        {

            Sbox_Inv = new byte[256];
            int b = 0;
            for (; b < 256; b++)
                Sbox_Inv[Sbox[b]] = (byte)b;
            ShiftRowTab_Inv = new byte[16];
            for (b = 0; b < 16; b++)
                ShiftRowTab_Inv[ShiftRowTab[b]] = (byte)b;
            xtime = new byte[256];
            for (b = 0; b < 128; b++)
            {
                xtime[b] = (byte)(b << 1);
                xtime[128 + b] = (byte)(b << 1 ^ 27);
            }
        }
        public Aes(byte[] key)
        {
            this.Key = this.InitKey(key);
        }
        public virtual byte[] InitKey(byte[] key) {
            return key;
        }

        public static byte[] ExpandKey(ref byte[] b)
        {
            int c = b.Length, d = 0;
            int e = 1;
            var c1 = c;
            if (c <= 16) { c1 = 16; d = 176; }
            else if (c <= 24) { c1 = 24; d = 208; }
            else if (c <= 32) { c1 = 32; d = 240; }
            else throw new Exception("my.ExpandKey: Only key lengths of 16, 24 or 32 bytes allowed!");
            Dyn.Resize(ref b, d);

            for (var g = c; g < d; g += 4)
            {
                var h = b.slice(g - 4, g);
                if (g % c == 0)
                {
                    h = new byte[] { (byte)(Sbox[h[1]] ^ e), (byte)(Sbox[h[2]]), Sbox[h[3]], Sbox[h[0]] };
                    if ((e <<= 1) >= 256)
                        e ^= 283;
                } else
                {
                    if (c > 24 && g % c == 16)
                    {
                        h = new byte[] { Sbox[h[0]], Sbox[h[1]], Sbox[h[2]], Sbox[h[3]] };
                    }
                }
                for (var f = 0; f < 4; f++)
                    b[g + f] = (byte)(b[g + f - c] ^ h[f]);
            }
            return b;
        }

        public virtual byte[] Encrypt(byte[] data) {
            var Key = this.Key;
            var d = Key.Length;
            Aes.AddRoundKey(data, Key.slice(0, 16));
            var e = 16;
            for (; e < d - 16; e += 16)
            {
                Aes.SubBytes(data, Sbox);
                Aes.ShiftRows(data, ShiftRowTab);
                Aes.MixColumns(data);
                Aes.AddRoundKey(data, Key.slice(e, e + 16));
            }
            Aes.SubBytes(data, Sbox);
            Aes.ShiftRows(data, ShiftRowTab);
            Aes.AddRoundKey(data, Key.slice(e, d));
            return data;

        }

        public virtual byte[] Decrypt(byte[] data) {
            var Key = this.Key;
            var d = Key.Length;
            Aes.AddRoundKey(data, Key.slice(d - 16, d));
            Aes.ShiftRows(data, ShiftRowTab_Inv);
            Aes.SubBytes(data, Sbox_Inv);
            for (d -= 32; d >= 16; d -= 16)
            {
                Aes.AddRoundKey(data, Key.slice(d, d + 16));
                Aes.MixColumns_Inv(data);
                Aes.ShiftRows(data, ShiftRowTab_Inv);
                Aes.SubBytes(data, Sbox_Inv);
            }
            Aes.AddRoundKey(data, Key.slice(0, 16));
            return data;
        }

        public static void SubBytes(byte[] a, byte[] c)
        {
            for (var d = 0; d < 16; d++)
                a[d] = c[a[d]];
        }
        public static void AddRoundKey(byte[] a, byte[] c)
        {
            for (var d = 0; d < 16; d++)
                a[d] ^= c[d];
        }

        public static void ShiftRows(byte[] a, byte[] c)
        {
            var d = new byte[0].concat(a);
            for (int e = 0; e < 16; e++)
                a[e] = d[c[e]];
        }

        public static void MixColumns(byte[] b)
        {
            var _xtime = xtime;
            for (var c = 0; c < 16; c += 4)
            {
                byte d = b[c + 0]
                    , e = b[c + 1]
                    , g = b[c + 2]
                    , h = b[c + 3]
                    , f = (byte)(d ^ e ^ g ^ h);
                b[c + 0] ^= (byte)(f ^ _xtime[d ^ e]);
                b[c + 1] ^= (byte)(f ^ _xtime[e ^ g]);
                b[c + 2] ^= (byte)(f ^ _xtime[g ^ h]);
                b[c + 3] ^= (byte)(f ^ _xtime[h ^ d]);
            }
        }
        public static void MixColumns_Inv(byte[] b)
        {
            ;
            var _xtime = xtime;
            for (var c = 0; c < 16; c += 4)
            {
                byte d = b[c + 0]
                    , e = b[c + 1]
                    , g = b[c + 2]
                    , h = b[c + 3]
                    , f = (byte)(d ^ e ^ g ^ h)
                    , o = (byte)(_xtime[f])
                    , p = (byte)(_xtime[_xtime[o ^ d ^ g]] ^ f);
                f ^= _xtime[_xtime[o ^ e ^ h]];
                b[c + 0] ^= (byte)(p ^ _xtime[d ^ e]);
                b[c + 1] ^= (byte)(f ^ _xtime[e ^ g]);
                b[c + 2] ^= (byte)(p ^ _xtime[g ^ h]);
                b[c + 3] ^= (byte)(f ^ _xtime[h ^ d]);
            }
        }
    }

    class AesCBC : Aes
    {
        public AesCBC(byte[] key) : base(key) {
        }
        public override byte[] InitKey(byte[] key) {
            Aes.ExpandKey(ref key);
            return key;
        }
        public static byte[] blockXOR(byte[] a, byte[] c)
        {
            var d = new byte[16];
            for (var e = 0; e < 16; e++)
                d[e] = (byte)(a[e] ^ c[e]);
            return d;
        }
        public static byte[] blockIV() {
            var a = new SecureRandom();
            var c =new byte[16];
            a.nextBytes(c);
            return c;
        }

        public static byte[] pad16(byte[] a) {
            var c = a.slice(0);
            var d = (16 - a.Length % 16) % 16;
            for (var i = a.Length; i < a.Length + d; i++)
                push(ref c, 0);
            return c;
        }

        public static byte[] depad(byte[] a)
        {
            for (a = a.slice(0); a[a.Length - 1] == 0;)
                a = a.slice(0, a.Length - 1);
            return a;
        }


        public override byte[] Encrypt(byte[] data)
        {
            var e = AesCBC.pad16(data);
            var g = AesCBC.blockIV();
            for (var h = 0; h < e.Length / 16; h++)
            {
                var f = e.slice(h * 16, h * 16 + 16);
                f = AesCBC.blockXOR(g.slice(h * 16, h * 16 + 16), f);
                base.Encrypt(f);
                g = g.concat(f);
            }
            return g;
        }

        public override byte[] Decrypt(byte[] data)
        {
            var g = new byte[0];
            for (var h = 1; h < data.Length / 16; h++)
            {
                var f = data.slice(h * 16, h * 16 + 16);
                var o = data.slice((h - 1) * 16, (h - 1) * 16 + 16);
                base.Decrypt(f);
                f = AesCBC.blockXOR(o, f);
                g = g.concat(f);
            }
            return AesCBC.depad(g);
        }
    }
}
class SecureRandom
{
    private static Random r = new Random();
    public void nextBytes(byte[] a)
    {
        r.NextBytes(a);
    }
    public static void NextBytes(byte[] a)
    {
        r.NextBytes(a);
    }

}
class Arcfour
{
    public int j, i;
    public byte[] s;
}