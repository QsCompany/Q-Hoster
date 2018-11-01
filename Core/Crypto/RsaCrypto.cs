using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
    static public class Rsa
    {
        static RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        static Rsa()
        {
            var f = new FileInfo("rsa");
            string xml;
            if (f.Exists)
            {
                xml = File.ReadAllText(f.FullName);
                rsa.FromXmlString(xml);
            }
            else
            {
                File.WriteAllText(f.FullName, xml = rsa.ToXmlString(true));
            }

        }

        public static byte[] EncoderBytes(string s)
        {
            var aes = new AesCryptoServiceProvider();
            aes.GenerateKey();
            var key = ToBase16(aes.Key);
            var iv = ToBase64(aes.IV);
            var ss = Encoding.UTF8.GetBytes(s);
            ss = rsa.Encrypt(ss, false);
            var x = rsa.Decrypt(ss, false);
            return ss;
        }
        public static string Encoder(string s)
        {
            //var rsa = System.Security.Cryptography.SHA1CryptoServiceProvider.Create("sha1");
            //var rsa = RSA.Create("sha1");

            var ss = Encoding.UTF8.GetBytes(s);
            ss = rsa.Encrypt(ss, false);
            return Encoding.UTF8.GetString(ss);
        }
        public static string Decode(string s)
        {
            var ss = Encoding.UTF8.GetBytes(s);
            ss = rsa.DecryptValue(ss);
            return Encoding.UTF8.GetString(ss);
        }
        public static string ToJson()
        {
            var ps = rsa.ExportParameters(!false);
            var cm = ToBase16(ps.Modulus);
            var e = Convert(ps.Exponent, true);
            var n = Convert(ps.Modulus, true);
            var d = Convert(ps.D, true);
            var p = Convert(ps.P, true);
            var q = Convert(ps.Q, true);

            var dp = Convert(ps.DP, true);
            var dq = Convert(ps.DQ, true);

            var coeff = Convert(ps.InverseQ, true);

            var c = new[] {                 
                new KeyValuePair<string,string>( "e",  e),
                new KeyValuePair<string,string>( "d",  d),
                new KeyValuePair<string,string>( "p",  p),
                new KeyValuePair<string,string>( "q",  q),
                new KeyValuePair<string,string>( "dmp1",  dp),
                new KeyValuePair<string,string>( "dmq1",  dq),

                new KeyValuePair<string,string>( "iq",  coeff),
                new KeyValuePair<string,string>( "m",  n)

            };
            string s = "";
            foreach (var b in c)
            {
                if (s != "") s += ",";
                s += string.Format("\r\n\t{0}:\"{1}\"", b.Key, b.Value);
            }
            return "{" + s + "\r\n}";
        }
        public static string Convert(byte[] c, bool b16OR64)
        {
            return b16OR64 ? ToBase16(c) : ToBase64(c);
        }
        public static string ToBase64(byte[] c)
        {
            byte[] ic;
            var j = 0;
            var i = 0;
            var count = Math.Floor((double)(c.Length / x.InputBlockSize));
            var rst = c.Length % x.InputBlockSize;
            ic = new byte[(int)(count * x.OutputBlockSize + (rst == 0 ? 0 : x.OutputBlockSize))];
            for (int cnt = 0; cnt < count; cnt++)
            {
                var v = x.TransformBlock(c, i, x.InputBlockSize, ic, j);
                j += x.OutputBlockSize;
                i += x.InputBlockSize;
                var ss = Encoding.UTF8.GetString(ic, 0, j);
            }
            if (rst != 0)
            {
                var v = x.TransformFinalBlock(c, i, rst);
                Array.Copy(v, 0, ic, j, 4);
                j += x.OutputBlockSize;
                i += rst;
            }
            return Encoding.UTF8.GetString(ic, 0, j);

        }
        public static string ToBase16(byte[] c)
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
    }
}        