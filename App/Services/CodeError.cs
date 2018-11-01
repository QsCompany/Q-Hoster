using System.Collections.Generic;
using Server;

namespace QServer.Core
{
    sealed public class CodeError : DObject
    {
        private static int counter = 1;
        private static Dictionary<string, CodeError> errors = new Dictionary<string, CodeError>();
        private static Dictionary<int, CodeError> _errors = new Dictionary<int, CodeError>();

        public static int DPName = Register<CodeError, string>("Name");
        public static int DPCode = Register<CodeError, int>("Code");
        public static int DPMessage = Register<CodeError, string>("Message");
        public static int DPIsSuccess = Register<CodeError, bool>("IsSuccess");


        public string Name => get<string>(DPName);

        public int Code => get<int>(DPCode);

        public string Message => get<string>(DPMessage);

        public bool IsSuccess => get<bool>(DPIsSuccess);

        private CodeError(string name, string msg, bool @throw = true)
        {
            set(DPCode, ++counter);
            set(DPMessage, msg);
            set(DPIsSuccess, !@throw);
            set(DPName, name);
        }

        public static CodeError GetError(int code)
        {
            return _errors[code];
        }

        public static CodeError GetError(string codeName)
        {
            CodeError er;
            if (errors.TryGetValue(codeName ?? "", out er))
                return er;
            return new CodeError(codeName, "contact admin <br>CodeError:<h1>"+codeName+"</h1>", true);
        }

        public static CodeError Register(string name, string msg, bool @throw = true, int code = -1)
        {
            CodeError c;
            errors.Add(name, c = new CodeError(name, msg, @throw));
            _errors.Add(c.Code, c);
            return c;
        }

        static CodeError()
        {
            Register("facture_empty", "<h2>Alert<h2> the facture cannot be empty .please buy any <h4>things<h4><h1>...</h1> ");
            Register("product_not_exist", "the arguments say that the product is <h1>not exist</h1>");
            Register("propably_hacking", "Are you <h2>hacking<h2> my site !<br><h1 class='text-center'>NO WAY<br> the Facture DELETED </h1>");
            Register("duplicated_article", "duplicated Article from another facture cannot be validate");
            Register("facture_isfrozen", "<h2>Alert<h2> the facture cannot be Modified .because it's <h4>Frozen<h4><h1>...</h1> ");

            Register("costumer_argnull", "Error Occured When Creating Costumer {ARGUMENT_NULL}");
            Register("person_undeleted", "The user has factures cannot be deleted<br><h1>DELETE Them<h1><h3>first<h3>");
            Register("access_restricted", "Access Restricted to <h1>ADMIN ONLY</h1>");
            Register("hacker_steel", " <h2>Becarful</h2> You are trying to <h1>steel</h1><h3> Information</h3> of others <br><p class='color:black'>Please d'ont try it. you cannot</p> ");
            Register("facture_saved", "The Facture <h2>successfully</h2> <h1>SAVED</h1>", false);
            Register("article_price_not_setted", "The Price Of Article Not Setted  <h2>Fatal </h2> <h1>ERROR</h1>", true);
            /*Register("", "");
        Register("", "");
        Register("", "");
        Register("", ""); */
        }

        public const string facture_empty = "facture_empty";
        public const string product_not_exist = "product_not_exist";
        public const string propably_hacking = "propably_hacking";
        public const string duplicated_article = "duplicated_article";
        public const string facture_isfrozen = "facture_isfrozen";
        public const string costumer_argnull = "costumer_argnull";
        public const string person_undeleted = "person_undeleted";
        public const string access_restricted = "access_restricted";
        public const string hacker_steel = "hacker_steel";
        public const string facture_saved = "facture_saved";
        public static string article_price_not_setted = "article_price_not_setted";
        public static string article_product_not_setted = "article_price_not_setted";
        public static string fatal_error;
        public static string QtelthZero;
        public static string DatabaseError;


        public static string UknownError { get; set; }
        public static string EmtyRequest = "EmptyRequest";
    }
}