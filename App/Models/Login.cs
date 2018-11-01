using System.Collections.Generic;
using Json;
using QServer.Core;
using Server;

namespace models
{
    [QServer.Core.HosteableObject(typeof(Api.Login), typeof(LoginSerializer))]
    public class  Login : DataRow, IHistory, ILogin
    {
        public User InitUser()
        {
            return User = new User(this, true);
        }
        public User User;
        public new static int __LOAD__(int dp) => DataRow.__LOAD__(DPPwd);

        public static int DPIsLogged = Register<Login, bool>("CheckLogging", PropertyAttribute.NonModifiableByHost); 
        public bool IsLogged { get => get<bool>(DPIsLogged); 
            set => set(DPIsLogged, value);
        }
        public static int DPUsername = Register<Login, string>("Username", PropertyAttribute.None); public string Username { get => get<string>(DPUsername);
            set => set(DPUsername, value);
        }
        public static int DPPwd = Register<Login, string>("Pwd", PropertyAttribute.Private); public string Pwd { get => get<string>(DPPwd);
            set => set(DPPwd, value);
        }


        static string bytes2string(byte[] s)
        {
            string ss = "";
            foreach (var b in s)
                ss += (char)b;
            return ss;

        }
        static byte[] string2bytes(string s)
        {
            byte[] ss = new byte[s.Length];
            for (int i = 0; i < ss.Length; i++)
                ss[i] = (byte)s[i];
            return ss;

        }
        public static string toArray(byte[] b)
        {
            var ss = "[";
            for (int i = 0; i < b.Length; i++)
            {
                ss += (i != 0 ? "," : "") + b[i];
            }
            ss += ']';
            return ss;
        }
        public static string toJS(byte[] key,byte[] data)
        {
            return $"{{key:{toArray(key)},data:{toArray(data)}}}";
        }
        private static readonly byte[] hexMap = new byte['G'];
        static Login()
        {
            for (int i = 0; i <= 9; i++)
                hexMap[i + '0'] = (byte)i;
            for (int i = 0; i < 6; i++)
                hexMap[i + 'A'] = (byte)(i + 10);
        }
        public static byte[] hex2bytes(string s)
        {
            for (int i = 0; i <= 9; i++)
                hexMap[i + '0'] = (byte)i;
            for (int i = 0; i < 6; i++)
                hexMap[i + 'A'] = (byte)(i + 10);
            if (s.Length % 2 == 1) throw new System.Exception("Format Error");
            var t = new byte[s.Length / 2];
            for (int i = 0, j = 0; i < s.Length; i += 2, j++)
            {
                var n1 = hexMap[s[i]];
                var n2 = hexMap[s[i + 1]];
                var st = ((n1 << 4) | n2);
                t[j] = (byte)st;
            }
            return t;
        }
        public bool RegeneratePwd(string keyString)
        {
            var encpwd = this.EncPwd;
            if (encpwd == null) Pwd = null;

            else
            {
                byte[] key = new byte[32];
                var key1 = System.Text.Encoding.UTF8.GetBytes(keyString);
                copy(key, key1, 0, 32);
                var x =new QServer.AesCBC(key);
                var bytes = hex2bytes(EncPwd);
                var data = x.Decrypt(bytes);
                Pwd = UTF8.ToString(data, 0);
            }
            return Pwd == keyString;
        }
        public string EncPwd
        {
            get => get<string>(DPEncPwd);
            set => set(DPEncPwd, value);
        }

        public static int DPIdentification = Register<Login, string>("Identification"); public string Identification { get => get<string>(DPIdentification);
            set => set(DPIdentification, value);
        }
        public static int DPClient = Register<Login, Client>("Client", PropertyAttribute.AsId, null, (d, c) => ((Database)d).GetClient(c)); public Client Client { get => get<Client>(DPClient);
            set => set(DPClient, value);
        }
        
        public static int DPIsThrusted = Register<Login, bool>("IsThrusted", PropertyAttribute.NonSerializable | PropertyAttribute.NonModifiableByHost);
        
        public bool IsThrusted { get => get<bool>(DPIsThrusted);
            set => set(DPIsThrusted, value);
        }

        

        public static int DPIsValidated = Register<Login, bool>("IsValidated", PropertyAttribute.NonSerializable | PropertyAttribute.NonModifiableByHost);
        public bool IsValidated { get => get<bool>(DPIsValidated);
            set => set(DPIsValidated, value);
        }
        public virtual AgentPermissions Permission { get => AgentPermissions.None; set { } }

        public Login(Context c, JValue jv):base(c,jv)
        {
        }

        public Login()
        {
            // TODO: Complete member initialization
        }

        protected bool checkUserName(RequestArgs args=null)
        {
            string un = Username ?? "";
            if (GlobalRegExp.username.IsMatch(un))
            {
                var c = un[0];
                if (this is Agent)
                {
                    if (c != '@') Username = "@" + Username;
                    return true;
                    //return un.StartsWith("@") ? true : args?.SendError("The Agent Username must be started with @&lt;Username&gt;") ?? false;
                }
                else return c != '@';
            }
            return false;
        }
        public virtual bool Check()
        {
            return checkUserName() && GlobalRegExp.password.IsMatch(Pwd);
        }


        public virtual bool Check(out List<Client.Message> k)
        {
            k = new List<Client.Message>();
            var un = Username = Username ?? "";
            k = k ?? new List<Client.Message>();
            if (!checkUserName())
                k.Add(new Client.Message(nameof(Username), "BadFormat"));
            if (Pwd == null || !GlobalRegExp.password.IsMatch(Pwd))
                k.Add(new Client.Message(nameof(Pwd), "BadFormat"));
            if (Client == null)
                k.Add(new Client.Message(nameof(Client), "ClientIsNull"));
            return k.Count == 0;
        }

        public virtual bool Check(RequestArgs args, ref List<Client.Message> k)
		{
            var un = Username = Username ?? "";
            k = k ?? new List<Client.Message>();
			if (!checkUserName(args))
				return args.SendError("Username: Bad Format");
			if (Pwd == null || !GlobalRegExp.password.IsMatch(Pwd))
				return args.SendError("Username: Bad Format");
			if (Client == null)
				return args.SendError("Client: UnResolved");
			k = null;


			if (args.Database.Clients[Client.Id] != null)
				return args.SendError(CodeError.propably_hacking);
            return k == null ? true : k.Count == 0;
		}
        public virtual bool Check(RequestArgs args)
        {
            if (!checkUserName(args))
                return args.SendError("Username: Bad Format NotNull && Length>=6 && @Username");
            if ( !Validator.IsPassword(Pwd))
                return args.SendError("Pwd: Bad Format NotNull && Length>=6");
            if (Client == null)
                return args.SendError("Client: UnResolved");
            
            var e = this;
            var oe = args.Database.Logins[Id];
            if (oe != null && e != null && oe.Client != null && e.Client != null)
                if (oe.Username == e.Username && null == e.Pwd)
                {
                    var c = e.Client;
                    var oc = e.Client;
                    if (c.Id == oc.Id && c.FirstName == oc.FirstName && c.LastName == oc.LastName && c.Job == oc.Job &&  c.Tel == oc.Tel && c.WorkAt == oc.WorkAt)
                        return true;
                }
            if (args.User.IsAgent)
                return true;
            return false;
        }

        public override JValue Parse(JValue json)
        {
            return json;
        }

        public static int DPEncPwd = Register<Login, string>("EncPwd", PropertyAttribute.Optional | PropertyAttribute.NonSerializable);
        static void copy(byte[] dst, byte[] src, int index, int max = 16)
        {
            var mx = System.Math.Min(max, src.Length);
            int i = 0;
            for (; i < mx; i++)
            {
                dst[index + i] = src[i] == 0 ? (byte)128 : src[i];
            }
            for (; i < max; i++)
            {
                dst[index + i] = 128;
            }
        }

        public override int Repaire(Database db)
        {
            if (!Check())
                return 1;
            return 0;
        }
    }
}