//namespace Regex
//{
//    public class TelF : System.Text.RegularExpressions.Regex
//    {
//        public TelF()
//            : base(@"^(0{1}[2|3]{1}\d{7})$")
//        {

//        }
//    }
//    public class TelM : System.Text.RegularExpressions.Regex
//    {
//        public TelM()
//            : base(@"^(0{1}[5|7|6|9]{1}\d{8})$")
//        {

//        }
//    }
//    public class Mail : System.Text.RegularExpressions.Regex
//    {
//        public Mail()
//            : base(@"^[\w\.\-]*\@\w*\.\w{0,3}$",System.Text.RegularExpressions.RegexOptions.IgnoreCase)
//        {

//        }
//    }
//    public class Name : System.Text.RegularExpressions.Regex
//    {
//        public Name()
//            : base(@"^[a-z|A-Z\s]*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
//        {

//        }
//    }
//    public class Username : System.Text.RegularExpressions.Regex
//    {
//        public Username()
//            : base(@"^([a-zA-Z\@][a-zA-Z\d\@\._]{5,20})$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
//        {
//        }
//    }    

//    public static class GlobalRegExp
//    {
//        public static System.Text.RegularExpressions.Regex toRegexp(this string s) => new System.Text.RegularExpressions.Regex(s, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
//        public static readonly System.Text.RegularExpressions.Regex telM = @"(0{1}[5|7|6|9]{1}\d{8})".toRegexp();
//        public static readonly System.Text.RegularExpressions.Regex telF = @"(0{1}[2|3]{1}\d{7})".toRegexp();
//        public static readonly System.Text.RegularExpressions.Regex mail = @"^(?:[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+\.)*[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!\.)){0,61}[a-zA-Z0-9]?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\[(?:(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\.){3}(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\]))$".toRegexp();
//        public static readonly System.Text.RegularExpressions.Regex name = @"^[a-z|A-Z\s]*$".toRegexp();
//        public static readonly System.Text.RegularExpressions.Regex username = @"^([a-zA-Z\@][a-zA-Z\d\@\._]{5,20})$".toRegexp();
//        public static readonly System.Text.RegularExpressions.Regex dimention = @"[\w\s\.\d\/\*\ \+\-\%\=°]*".toRegexp();
//        public static readonly System.Text.RegularExpressions.Regex alphanumeric = @"^[a-zA-Z0-9]*$".toRegexp();
//        public static readonly System.Text.RegularExpressions.Regex url = @"^(?!mailto:)(?:(?:https?|ftp):\/\/)?(?:\S+(?::\S*)?@)?(?:(?:(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]+-?)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]+-?)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})))|localhost)(?::\d{2,5})?(?:\/[^\s]*)?$".toRegexp();
//        public static readonly System.Text.RegularExpressions.Regex ip = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$".toRegexp();
//        public static readonly System.Text.RegularExpressions.Regex num = @"^-?[0-9]+$".toRegexp();
//        public static readonly System.Text.RegularExpressions.Regex @int = @"^(?:-?(?:0|[1-9][0-9]*))$".toRegexp();
//        public static readonly System.Text.RegularExpressions.Regex @decimal = @"^(?:-?(?:0|[1-9][0-9]*))?(?:\.[0-9]*)?$".toRegexp();
//        public static readonly System.Text.RegularExpressions.Regex text = @"^[a-zA-Z0-9\s\.]*$".toRegexp();
//        public static readonly System.Text.RegularExpressions.Regex password = @"^[a-zA-Z0-9\s\.\@\!\?]+$".toRegexp();
//        public static readonly System.Text.RegularExpressions.Regex @ref = @"^[A-Z]{1}[0-9]{1,5}$".toRegexp();


//        static GlobalRegExp()
//        {
//            /*
//             * 
//            checks['ip'] = str => !!str.match(/^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/);
//            checks['numeric'] = (str) => !!str.match(/^-?[0-9]+$/);
//            checks['int'] = str => !!str.match(/^(?:-?(?:0|[1-9][0-9]*))$/);
//            checks['decimal'] = str => !!str.match(/^(?:-?(?:0|[1-9][0-9]*))?(?:\.[0-9]*)?$/);

//            checks['text'] = (str): boolean => !!str.match(/^[a-zA-Z0-9\s\.]*$/);
//            checks['password'] = (str): boolean => !!str.match(/^[a-zA-Z0-9\s\.\@\!\?]+$/);
//            checks['any'] = (str): boolean => true;
//            checks['ref'] = (str): boolean => !!str.match(/^[A-Z]{1}[0-9]{1,5}$/);
//             */
//        }
//    }
//    //    new RegexCompilationInfo(@"(0{1}[5|7|6|9]{1}\d{8})", RegexOptions.Compiled | RegexOptions.IgnoreCase, "TelM", "Regex", true) ,
//    //    new RegexCompilationInfo(@"(0{1}[2|3]{1}\d{7})", RegexOptions.Compiled | RegexOptions.IgnoreCase, "TelF", "Regex", true) ,
//    //    new RegexCompilationInfo(@"[\w\.\-]*\@\w*\.\w{0,3}", RegexOptions.Compiled | RegexOptions.IgnoreCase, "Mail", "Regex", true) ,
//    //    new RegexCompilationInfo(@"[a-z|A-Z\s]*", RegexOptions.Compiled | RegexOptions.IgnoreCase, "Name", "Regex", true) ,
//    //    new RegexCompilationInfo(@"[\w]*", RegexOptions.Compiled | RegexOptions.IgnoreCase, "Username", "Regex", true)                
//}
public static class GlobalRegExp
{
    public static System.Text.RegularExpressions.Regex toRegexp(this string s) => new System.Text.RegularExpressions.Regex(s, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    public static readonly System.Text.RegularExpressions.Regex telM = @"(0{1}[5|7|6|9]{1}\d{8})".toRegexp();
    public static readonly System.Text.RegularExpressions.Regex telF = @"(0{1}[2|3]{1}\d{7})".toRegexp();
    public static readonly System.Text.RegularExpressions.Regex mail = @"^(?:[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+\.)*[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!\.)){0,61}[a-zA-Z0-9]?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\[(?:(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\.){3}(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\]))$".toRegexp();
    public static readonly System.Text.RegularExpressions.Regex name = @"^[a-z|A-Z\d\s]*$".toRegexp();
    public static readonly System.Text.RegularExpressions.Regex username = @"^([a-zA-Z\@][a-zA-Z\d\@\._]{5,20})$".toRegexp();
    public static readonly System.Text.RegularExpressions.Regex dimention = @"[\w\s\.\d\/\*\ \+\-\%\=°]*".toRegexp();
    public static readonly System.Text.RegularExpressions.Regex alphanumeric = @"^[\w\d\s]*$".toRegexp();
    public static readonly System.Text.RegularExpressions.Regex url = @"^(?!mailto:)(?:(?:https?|ftp):\/\/)?(?:\S+(?::\S*)?@)?(?:(?:(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]+-?)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]+-?)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})))|localhost)(?::\d{2,5})?(?:\/[^\s]*)?$".toRegexp();
    public static readonly System.Text.RegularExpressions.Regex ip = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$".toRegexp();
    public static readonly System.Text.RegularExpressions.Regex num = @"^-?[0-9]+$".toRegexp();
    public static readonly System.Text.RegularExpressions.Regex @int = @"^(?:-?(?:0|[1-9][0-9]*))$".toRegexp();
    public static readonly System.Text.RegularExpressions.Regex @decimal = @"^(?:-?(?:0|[1-9][0-9]*))?(?:\.[0-9]*)?$".toRegexp();
    public static readonly System.Text.RegularExpressions.Regex text = @"[.]*".toRegexp();
    public static readonly System.Text.RegularExpressions.Regex password = @"^[a-zA-Z0-9\s\.\@\!\?]+$".toRegexp();
    public static readonly System.Text.RegularExpressions.Regex @ref = @"^[A-Z]{1}[0-9]{1,5}$".toRegexp();
    public static readonly System.Text.RegularExpressions.Regex alphapitic = @"^[a-z|A-Z]+$".toRegexp();

    static GlobalRegExp()
    {
        /*
         * 
        checks['ip'] = str => !!str.match(/^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/);
        checks['numeric'] = (str) => !!str.match(/^-?[0-9]+$/);
        checks['int'] = str => !!str.match(/^(?:-?(?:0|[1-9][0-9]*))$/);
        checks['decimal'] = str => !!str.match(/^(?:-?(?:0|[1-9][0-9]*))?(?:\.[0-9]*)?$/);

        checks['text'] = (str): boolean => !!str.match(/^[a-zA-Z0-9\s\.]*$/);
        checks['password'] = (str): boolean => !!str.match(/^[a-zA-Z0-9\s\.\@\!\?]+$/);
        checks['any'] = (str): boolean => true;
        checks['ref'] = (str): boolean => !!str.match(/^[A-Z]{1}[0-9]{1,5}$/);
         */
    }
}
