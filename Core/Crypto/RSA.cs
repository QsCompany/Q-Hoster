using System;
using System.Linq;
namespace QServer.Crypto
{
    public delegate bool Test(int number);
    public class GCDExtended
    {
        public int GCD;
        public int FactorA;
        public int FactorB;
        public GCDExtended(int gcd,int b,int c)
        {
            GCD = gcd;
            FactorA = b;
            FactorB = c;
        }
        public GCDExtended Set(int gcd, int b, int c)
        {
            GCD = gcd;
            FactorA = b;
            FactorB = c;
            return this;
        }
        //public int this[int index]
        //{
        //    get { return index == 0 ? GCD : index == 1 ? FactorA : FactorB; }
        //    set
        //    {
        //        switch (index)
        //        {
        //            case 0:
        //                GCD = value;
        //                break;
        //            case 1:
        //                FactorA = value;
        //                break;
        //            case 2:
        //                FactorB = value;
        //                break;
        //            default:
        //                throw null;
        //        }
        //    }
        //}
    }
    public static class Math
    {
        public static int mod(this int t, int n) => ((t % n) + n) % n;
        private static Random Random = new Random();
        private static int[] _primes;

        private static int[] Primes
        {
            get
            {
                if (_primes == null)
                {
                    var s = System.IO.File.ReadAllText("Crypto/Primes.data").Split(',');
                    _primes = new int[s.Length];
                    for (int i = 0; i < s.Length; i++)
                        _primes[i] = int.Parse(s[i]);
                }
                return _primes;
            }
        }
        public static int GetRandomPrime(Test cond, int maxIndex = -1)
        {
            if (maxIndex <= 0)
                maxIndex = Primes.Length - 1;
            int p;
            do
                p = _primes[Random.Next(maxIndex)];
            while (!cond(p));
            return p;
        }
        public static int PowMod(int @base, int exp, int modulus)
        {
            @base %= modulus;
            var result = 1;
            while (exp > 0)
            {
                if ((exp & 1) != 0) result = (result * @base) % modulus;
                @base = (@base * @base) % modulus;
                exp >>= 1;
            }
            return result;
        }

        public static bool Divides(int numerator, int denominator)
        {
            if (numerator.mod(denominator) > 0)
                return false;
            return true;
        }
        /// <summary>
        /// solve Equation d=FactorA*p+FactorB*q
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static GCDExtended Gcd_extended(int p, int q)
        {
            if (q == 0)
                return new GCDExtended(p, 1, 0);

            var vals = Gcd_extended(q, p.mod(q));
            return vals.Set(vals.GCD, vals.FactorB, vals.FactorA - vals.FactorB * (p / q));
        }
        public static int[] Solve(int factor, int rem, int modulus)
        {
            var m = System.Math.Abs(modulus);
            var a = factor.mod(m);
            var b = rem.mod(m);
            var result_extended = Gcd_extended(a, m);
            var solutions = new int[result_extended.GCD];

            if (!Divides(b, result_extended.GCD))
                return solutions;

            var firstSolution = (result_extended.FactorA * (b / result_extended.GCD)).mod(m);
            for (var i = 0; i < result_extended.GCD; i++)
            {
                var otherSolution = (firstSolution + i * (m / result_extended.GCD)).mod(m);
                solutions[i] = otherSolution;
            }

            return solutions.OrderBy((x) => -x).ToArray();// solutions.sort(function(a, b) { return b - 1 });
        }
    }

    public interface ITransform
    {
        int Transform(int @byte);
        bool IsValideByte(int @byte);
    }
    public class RSAKey
    {
        public int n;
        public int e;
        public static RSACrypter GenerateRSAKey(int sourceMaxByte, int transformedMaxByte)
        {

            var p = Math.GetRandomPrime((v) => v > 100, 100);
            var q = Math.GetRandomPrime((t) =>
            {
                if (t == p) return false;
                var n1 = t * p;
                if (n1 < sourceMaxByte) return false;
                if (n1 > transformedMaxByte) return false;
                return true;
            }, 100);

            int n = p * q, h = (p - 1) * (q - 1), d, e;
            var t1 = DateTime.Now;
            do
            {
                if ((DateTime.Now - t1).TotalSeconds > 5) throw new Exception("Time exeeded");

                e = Math.GetRandomPrime((v) => v < h);
                var sols = Math.Solve(e, 1, h);
                if (sols.Length == 0) continue;
                d = sols[0];
                break;

            } while (true);
            return new RSACrypter()
            {
                Decripter = new RSA(new RSAKey() { n = n, e = d }),
                Encrypter = new RSA(new RSAKey() { n = n, e = e })
            };
        }
    }
    public class RSACrypter
    {
        public RSA Encrypter;
        public RSA Decripter;
    }
    public class RSA : ITransform
    {
        private RSAKey _key;

        public RSAKey Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public RSA(RSAKey key) => _key = key;

        public bool IsValideByte(int @byte) => @byte >= 0 && @byte < _key.n;

        public int Transform(int @byte) => Math.PowMod(@byte, _key.e, _key.n);
    }
}