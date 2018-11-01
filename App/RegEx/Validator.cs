

namespace QServer.Core
{
    public class Validator
    {
        public static bool IsEmail(string s)
        {
            return s != null && GlobalRegExp.mail.IsMatch(s);
        }

        public static bool IsIP(string s)
        {
            return s != null && GlobalRegExp.ip.IsMatch(s);
        }
        public static bool IsTel(string s)
        {
            return s != null && (GlobalRegExp.telM.IsMatch(s) || GlobalRegExp.telF.IsMatch(s));
        }


        public static bool IsName(string s)
        {
            return s != null && GlobalRegExp.name.IsMatch(s);
        }
        public static bool IsUsername(string s)
        {
            return s != null && GlobalRegExp.username.IsMatch(s);
        }
        public static bool IsProductName(string s)
        {
            return s != null && GlobalRegExp.text.IsMatch(s);
        }

        public static bool IsDescription(string s)
        {
            return true;
        }
        
        public static bool IsSerieName(string s)
        {
            return string.IsNullOrWhiteSpace(s) || GlobalRegExp.alphanumeric.IsMatch(s);
        }
        public static bool IsDimention(string s)
        {
            return string.IsNullOrWhiteSpace(s) || GlobalRegExp.dimention.IsMatch(s);
        }

        internal static bool IsCategoryName(string s)
        {
            return s != null && GlobalRegExp.name.IsMatch(s);
        }

        internal static bool IsPassword(string pwd)
        {
            return !string.IsNullOrWhiteSpace(pwd) && pwd.Length >= 6 && GlobalRegExp.password.IsMatch(pwd);
        }
    }
}
