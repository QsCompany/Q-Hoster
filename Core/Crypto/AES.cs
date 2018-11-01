using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
    public class  AES
    {

        Aes rsa;
        ICryptoTransform _decoder;
        ICryptoTransform _encoder;
        public AES(byte[] key)
        {
            
            rsa = new AesManaged();
            rsa.GenerateKey();
            rsa.GenerateIV();
            _decoder = rsa.CreateDecryptor();
            _encoder = rsa.CreateEncryptor();
        }

        public byte[] EncodeBytes(string s)
        {
            return Transform(_encoder, Encoding.UTF8.GetBytes(s));
        }
        public byte[] EncodeBytes(byte[] e)
        {
            var es = new PerformanceCounter();

            return Transform(_encoder ?? rsa.CreateEncryptor(), e);
        }
        public byte[] DecodeBytes(string s)
        {
            return Transform(rsa.CreateDecryptor(), Encoding.UTF8.GetBytes(s));
        }

        public byte[] DecodeBytes(byte[] e)
        {

            return Transform(_decoder ?? rsa.CreateDecryptor(), e);
        }

        public string Encode(string s)
        {
            return Encoding.UTF8.GetString(Transform(_encoder, Encoding.UTF8.GetBytes(s)));
        }
        public string Decode(string s)
        {
            return Encoding.UTF8.GetString(Transform(_decoder, Encoding.UTF8.GetBytes(s)));
        }

        public string ToBase16(byte[] c)
        {
            string s = "";
            if (c == null) return "";
            foreach (var b in c)
            {
                s += b.ToString("x2"); //: "," + b.ToString();
            }
            return s;
        }

        private static ToBase64Transform x = new ToBase64Transform();
        private static FromBase64Transform fx = new FromBase64Transform(FromBase64TransformMode.DoNotIgnoreWhiteSpaces);

        public static byte[] Transform(ICryptoTransform x, byte[] c)
        {
            if (x is RijndaelManagedTransform)
            {
                var t = (RijndaelManagedTransform)x;
                t.Reset();
            }
            byte[] ic;
            var j = 0;
            var i = 0;
            var rst = c.Length % x.InputBlockSize;
            var count = Math.Floor((double)(c.Length / x.InputBlockSize));
            var tcount = count + (rst == 0 ? 0 : 1);
            //c = Expand(c, (int)(x.InputBlockSize * tcount));
            ic = new byte[(int)(tcount * x.OutputBlockSize)];
            var g = x.GetType().GetInterfaces();
            var jtot = (int)(count * x.OutputBlockSize);
            //x.TransformBlock(c, 0, c.Length, ic, 0);
            //var xcc = x.TransformBlock(c, 0, c.Length, ic, 0);
            for (int cnt = 0; j < jtot; cnt++)
            {
                x.TransformBlock(c, 0, c.Length, ic, 0);
                var v = x.TransformBlock(c, i, x.InputBlockSize, ic, j);
                j += v;
                i += x.InputBlockSize;
                //var ss = Encoding.UTF8.GetString(ic, 0, j);
            }
            if (rst != 0)
            {
                var v = x.TransformFinalBlock(c, i, rst);
                Array.Copy(v, 0, ic, j, v.Length);
                j += x.OutputBlockSize;
                i += rst;
            }
            return ic;
        }

        private static byte[] Expand(byte[] c, int p)
        {
            var cx = new byte[p];
            Array.Copy(c, cx, c.Length);
            return cx;
        }

        public static byte[] FromBase64(byte[] p)
        {
            return fx.TransformFinalBlock(p, 0, p.Length);
        }
        public string FromBase64(string p)
        {
            return Encoding.UTF8.GetString(Transform(fx, Encoding.UTF8.GetBytes(p)));
        }
        public string ToBase64(string p)
        {
            return Encoding.UTF8.GetString(Transform(fx, Encoding.UTF8.GetBytes(p)));
        }
        public static byte[] ToBase64(byte[] p)
        {
            return Transform(x, p);
            var c = p.Length * 2;
            var ix = new byte[p.Length / 3 * 4 + (p.Length % 3 == 0 ? 0 : 4) + 10000];
            //var it = x.TransformBlock(p, 0, p.Length, ix, 0);
            p.CopyTo(ix, 0);
            return x.TransformFinalBlock(ix, 1, p.Length);
        }


    }
}        