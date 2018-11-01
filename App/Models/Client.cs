using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Json;

using Server;

namespace models
{
    public enum Abonment
    {
        Detaillant = 0,
        Proffessionnal = 1,
        DemiGrossist = 2,
        Grossist = 3,
        Importateur = 4,
        Exportateur = 5,
        All = -1
    }
    
    public enum AgentPermissions
    {
        None = 0,
        Agent = 1,
        Vendeur = Agent | 2,
        Achteur = Agent | 4,
        Cassier = Agent | 8,
        Validateur = Agent | 16,
        Admin = -1
    }


    public abstract class SiegeSocial : DataRow
    {
        public new static int __LOAD__(int dp) => DataRow.__LOAD__(DPTel) + 1;

        public static int DPAddress = Register<SiegeSocial, string>("Address");
        public string Address { get => get<string>(DPAddress); set => set(DPAddress, value); }


        public static int DPVille = Register<SiegeSocial, string>("Ville");
        public string Ville { get => get<string>(DPVille); set => set(DPVille, value); }


        public static int DPCodePostal = Register<SiegeSocial, string>("CodePostal");
        public string CodePostal { get => get<string>(DPCodePostal); set => set(DPCodePostal, value); }


        public static int DPSiteWeb = Register<SiegeSocial, string>("SiteWeb");
        public string SiteWeb { get => get<string>(DPSiteWeb); set => set(DPSiteWeb, value); }


        public static int DPEmail = Register<SiegeSocial, string>("Email");
        public string Email { get => get<string>(DPEmail); set => set(DPEmail, value); }


        public static int DPTel = Register<SiegeSocial, string>("Tel");
        public string Tel { get => get<string>(DPTel); set => set(DPTel, value); }


        public static int DPMobile = Register<SiegeSocial, string>("Mobile");
        public string Mobile { get => get<string>(DPMobile); set => set(DPMobile, value); }

        public SiegeSocial(Context c, JValue jv) : base(c, jv)
        {
        }

        public SiegeSocial()
        {
        }
    }

    public abstract class Company : SiegeSocial
    {
        public static new int __LOAD__(int dp) => SiegeSocial.__LOAD__(DPNAI)+1;

        public static int DPRIB = Register<Company, string>("RIB");
        public string RIB { get => get<string>(DPRIB); set => set(DPRIB, value); }


        public static int DPNRC = Register<Company, string>("NRC");
        public string NRC { get => get<string>(DPNRC); set => set(DPNRC, value); }


        public static int DPNIF = Register<Company, string>("NIF");
        public string NIF { get => get<string>(DPNIF); set => set(DPNIF, value); }

        public static int DPNCompte = Register<Company, string>("NCompte");
        public string NCompte { get => get<string>(DPNCompte); set => set(DPNCompte, value); }


        public static int DPCapitalSocial = Register<Company, string>("CapitalSocial");
        public string CapitalSocial { get => get<string>(DPCapitalSocial); set => set(DPCapitalSocial, value); }


        public static int DPNAI = Register<Company, string>("NAI");
        public string NAI { get => get<string>(DPNAI); set => set(DPNAI, value); }


        public static int DPNIS = Register<Company, string>("NIS");

        public Company(Context c, JValue jv) : base(c, jv)
        {
        }

        public Company() : base()
        {
        }

        public string NIS { get => get<string>(DPNIS); set => set(DPNIS, value); }
    }
    
    public abstract class  Dealer :Company
    {
        public new static int __LOAD__(int dp) => Company.__LOAD__(DPIsActive) + 1;
        public static int DPAvatar = Register<Dealer, string>("Avatar", PropertyAttribute.None, null, null, "nvarchar(25)");
        public readonly static int DPMontantTotal = Register<Dealer, float>("MontantTotal", PropertyAttribute.NonModifiableByHost);
        public readonly static int DPVersmentTotal = Register<Dealer, float>("VersmentTotal", PropertyAttribute.NonModifiableByHost);

        public static int DPName = Register<Dealer, string>("Name");
        public readonly static int DPObservation = Register<Dealer, string>("Observation", PropertyAttribute.None, null, null, "nvarchar(255)");
        public readonly static int DPNFactures = Register<Dealer, int>("NFactures", PropertyAttribute.NonModifiableByHost, null);
        
        public static int DPRef = Register<Dealer, string>("Ref", PropertyAttribute.None, null, null, "nvarchar(20)");
        public string Ref { get => get<string>(DPRef); set => set(DPRef, value); }

        public static int DPIsActive = Register<Dealer, bool>("IsActive", PropertyAttribute.Private);
        public bool IsActive { get => get<bool>(DPIsActive); set => set(DPIsActive, value); }
        

        
        public string Observation { get { return get<string>(DPObservation); } set { set(DPObservation, value); } }
        public float MontantTotal { get { return get<float>(DPMontantTotal); } set { set(DPMontantTotal, value); } }
        public float VersmentTotal { get { return get<float>(DPVersmentTotal); } set { set(DPVersmentTotal, value); } }
        
        public Dealer(Context c, JValue jv):base(c,jv)
        {
            
        }


        public Dealer():base()
        {
        }
        
        public string Name
        {
            get => (string)get(DPName);
            set => set(DPName, value);
        }
        public string Avatar
        {
            get => (string)get(DPAvatar);
            set => set(DPAvatar, value);
        }
    }

    [QServer.Core.HosteableObject(typeof(Api.Fournisseur), typeof(FournisseurSerializer))]
    public class  Fournisseur:Dealer
    {
        public new static int __LOAD__(int dp) => Dealer.__LOAD__(DPRef) + 1;
        
        public Fournisseur()
        {
           // Factures = new SFactures(this);
        }
        public Fournisseur(Context c, JValue jv)
            : base(c, jv)
        {
        }
        public void CloneFrom(Fournisseur c)
        {
            Email = c.Email;
            Tel = c.Tel;
            Name = c.Name;
            Address = c.Address;
            Avatar = c.Avatar;
        }
        internal bool Check(RequestArgs args,Fournisseur oper)
        {
            if (Email != null && !GlobalRegExp.mail.IsMatch(Email)) return args.SendError("UnvalideEmail");
            if (Tel != null && !GlobalRegExp.telF.IsMatch(Tel) && !GlobalRegExp.telM.IsMatch(Tel) ) return args.SendError("UnvalideEmail");
            if (Name != null && !GlobalRegExp.name.IsMatch(Name)) return args.SendError("UnvalidName");
            return true;
        }

        internal bool GetSVersments(ref SVersments source)
        {
            var id = Id;
            var t = new SVersments(this);
            foreach (KeyValuePair<long, DataRow> x in source)
            {
                var sv = (models.SVersment)x.Value;
                if (sv.Fournisseur?.Id == id) t.Add(sv);
            }
            source = t;
            return true;
        }

        public List<SFacture> GetValidableFactures(Database d)
        {
            var t = new List<SFacture>();
            var l = d.SFactures.AsList();
            for (int i = 0; i < l.Length; i++)
            {
                var f = l[i].Value as SFacture;
                if (f.IsValidable)
                    if (f.Fournisseur?.Id == this?.Id) t.Add(f);
            }
            return t;
        }

        public override int Repaire(Database db)
        {
            var t = 0f;
            bool update = false;
            foreach (SFacture l in GetValidableFactures(db))
                t += l.Total;

            if (MontantTotal != t)
                update = true;
            t = 0f;
            var allVersments = db.SVersments;

            if (GetSVersments(ref allVersments))
                foreach (var l in allVersments.AsList())
                    t += ((SVersment)l.Value).Montant;
            if (VersmentTotal != t)
                update = true;

            if (update)
                return db.Save(this, true) ? 0 : 1;
            return 0;
        }
    }

    [QServer.Core.HosteableObject(typeof(Api.Client), typeof(ClientSerializer))]
    public class  Client : Dealer, IHistory, IClient
    {
        public new static int __LOAD__(int dp) => DpLastName;


        public static int DPAbonment = Register<Client, Abonment>("Abonment");
        public Abonment Abonment { get => get<Abonment>(DPAbonment);
            set => set(DPAbonment, value);
        }
        public static int DpFirstName = Register<Client, string>("FirstName"); public string FirstName { get => get<string>(DpFirstName);
            set => set(DpFirstName, value);
        }
        public static int DpLastName = Register<Client, string>("LastName"); public string LastName { get => get<string>(DpLastName);
            set => set(DpLastName, value);
        }
        public static int DpJob = Register<Client, Job>("Job");
        public Job Job { get => get<Job>(DpJob);
            set => set(DpJob, value);
        }
        public static int DpPicture = Register<Client, string>("Picture", PropertyAttribute.None, null); public Picture Picture { get => get<Picture>(DpPicture);
            set => set(DpPicture, value);
        }
        public static int DpWorkAt = Register<Client, string>("WorkAt", "nvarchar(50)"); public string WorkAt { get => get<string>(DpWorkAt);
            set => set(DpWorkAt, value);
        }

        public string FullName => $"{FirstName ?? ""} {LastName ?? ""}";

        public float SoldTotal => MontantTotal - VersmentTotal;

        static Client()
        {
            //Regex.CompileToAssembly(new RegexCompilationInfo[] 
            //{
            //    new RegexCompilationInfo(@"(0{1}[5|7|6|9]{1}\d{8})", RegexOptions.Compiled | RegexOptions.IgnoreCase, "TelM", "Regex", true) ,
            //    new RegexCompilationInfo(@"(0{1}[2|3]{1}\d{7})", RegexOptions.Compiled | RegexOptions.IgnoreCase, "TelF", "Regex", true) ,
            //    new RegexCompilationInfo(@"[\w\.\-]*\@\w*\.\w{0,3}", RegexOptions.Compiled | RegexOptions.IgnoreCase, "Mail", "Regex", true) ,
            //    new RegexCompilationInfo(@"[a-z|A-Z\s]*", RegexOptions.Compiled | RegexOptions.IgnoreCase, "Name", "Regex", true) ,
            //    new RegexCompilationInfo(@"[\w]*", RegexOptions.Compiled | RegexOptions.IgnoreCase, "Username", "Regex", true)                
            //},
            //new System.Reflection.AssemblyName("yeh"));
        }
        public struct Message{public string Title, Body;
            public Message (string title,string body):this()
            {
                Title = title;
                Body = body;
            }
            public static string ToStrin(List<Message> k)
            {
                StringBuilder r = new StringBuilder();
                foreach (var m in k)
                    r.Append("<p><h4 style='color:Yellow'>").Append(m.Title).Append("</h4><h5 class='msg-body' style='padding-left:20px'>").Append(m.Body).Append("</h5></p>");
                return r.ToString();
            }
        }
        public bool Check(out string error)
        {
            List<Message> msg;
            error = null;
            if (Check(out msg)) return true;
            StringBuilder f = new StringBuilder();
            foreach (var m in msg)
                f.AppendFormat("<h2 style='color=yellow'>{0}</h2><h3>{1}</h3><br>", m.Title, m.Body);            
            error = f.ToString();
            return false;
        }
        public bool Check(out List<Message> messages)
        {
            var n = get<string>(DPName);
            var a = get<string>(DpFirstName);
            var b = get<string>(DpLastName);
            var c = get<string>(DPTel);
            var d = get<string>(DPEmail);
            var j = get<Job>(DpJob);
            var w = get<string>(DpWorkAt);
            messages = new List<Message>();
            
            if (n == null || !GlobalRegExp.name.IsMatch(n))
            {
                messages.Add(new Message(nameof(Name), "Client Name is Bad Format"));
            }
            if (!string.IsNullOrWhiteSpace(a) && !GlobalRegExp.name.IsMatch(a))
            {
                messages.Add(new Message(nameof(FirstName), "Bad Format"));
            }
            if (!string.IsNullOrWhiteSpace(b) && !GlobalRegExp.name.IsMatch(b))
                messages.Add(new Message(nameof(LastName), "Bad Format"));

            if (c == null || !GlobalRegExp.telM.IsMatch(c))
                if (c == null || !GlobalRegExp.telF.IsMatch(c))
                    messages.Add(new Message(nameof(Tel), "Bad Format"));

            if (!string.IsNullOrWhiteSpace(d) && !GlobalRegExp.mail.IsMatch(d))
                messages.Add(new Message(nameof(Email), "Bad Format"));
            /*if (j < 0 || (int)j > 4)
                messages.Add(new Message("FirstName", "Please don't hack my site"));*/
            
            return messages.Count == 0;
            
        }
        
        
        //public static int DPFactures = Register<Client, Factures>("Factures", PropertyAttribute.NonSerializable | PropertyAttribute.NonModifiableByHost, (s, o, n) => { return n == null ? new Factures((DataRow)s) : n; }, (d, c) => d.GetFactures(c)); public Factures Factures => get<Factures>(DPFactures);

        public void CloneFrom(Client c)
        {
            var id = Id;
            for (int i = 0; i < _values.Length; i++)
                _values[i] = c._values[i];
            Id = id;
        }
        public Client()
        {
           // set(DPFactures, new Factures(this));
        }        
        public Client(Context c,JValue jv):base(c,jv)
        {
            //set(DPFactures, new Factures(this));
        }


        internal object GetCookie(string p, bool deleteIfExist, out bool expire)
        {
            expire = false;
            Cookie cook;
            if (cookies.TryGetValue(p, out cook))
            {
                if (cook.IsExpire)
                {
                    cookies.TryRemove(p, out cook);
                    expire = true;
                    return cook.value;
                }
                if (deleteIfExist) cookies.TryRemove(p, out cook);
                return cook.value;
            }
            return null;
        }
        internal object GetCookie(string p, bool deleteIfExist)
        {
            Cookie cook;
            if (cookies.TryGetValue(p, out cook))
            {
                if (cook.IsExpire)
                {
                    cookies.TryRemove(p, out cook);
                    return null;
                }
                if (deleteIfExist) cookies.TryRemove(p, out cook);
                return cook.value;
            }
            return null;
        }

        private ConcurrentDictionary<string, Cookie> cookies =
            new ConcurrentDictionary<string, Cookie>();

        internal void SetCookie(string p, object message,DateTime expire)
        {
            Cookie c;
            cookies.AddOrUpdate(p, c = new Cookie(message, expire), (o, n) => c);
        }


        public List<Facture> GetValidableFactures(Database d)
        {
            var t = new List<Facture>();
            var l = d.Factures.AsList();
            for (int i = 0; i < l.Length; i++)
            {
                var f = l[i].Value as Facture;
                if (f.IsValidable)
                    if (f.Client == this) t.Add(f);
            }
            return t;
        }
        internal bool GetVersments(ref Versments source)
        {
            var id = Id;
            var t = new Versments(this);
            foreach (KeyValuePair<long, DataRow> x in source)
            {
                var sv = (models.Versment)x.Value;
                if (sv.Client?.Id == id) t.Add(sv);
            }
            source = t;
            return true;
        }

        public override int Repaire(Database db)
        {
            var t = 0f;
            bool update = false;
            foreach (Facture l in GetValidableFactures(db))
                t += l.Total;

            if (MontantTotal != t)
                update = true;
            t = 0f;
            var allVersments = db.Versments;
            if (GetVersments(ref allVersments))
                foreach (var l in allVersments.AsList())
                    t += ((Versment)l.Value).Montant;
            if (VersmentTotal != t)
                update = true;
            if (update)
                return db.Save(this, true) ? 0 : 1;
            return 0;
        }

        public void AddMessage(SMS sms) {
            if (sms.IsReaded) return;
            UnReadedSMS.Add(sms);
        }
        private SMSs UnReadedSMS;
        public static SMSs GetSMSs(RequestArgs args) => args.Client.GetSMSs(args.Database);
        public  SMSs GetSMSs(Database db)
        {
            var s = UnReadedSMS;
            if (s == null)
            {
                var id = Id;
                s = new SMSs();
                var l = db.SMSs.AsList();
                for (int i = 0; i < l.Length; i++)
                {
                    var v = (SMS)l[i].Value;
                    if (!v.IsReaded && v.To?.Id == id) s.Add(v);
                }
            }
            return s;
        }
        public void MakeSMSReaded(SMS sms)
        {
            if (!sms.IsReaded) throw new Exception("Le SMS doit etre IsReaded==true");
            UnReadedSMS?.Remove(sms.Id);
        }

        public void DeleteUnReadedSMS(SMS sms)
        {
            if(!sms.IsReaded) UnReadedSMS?.Remove(sms);
        }
        public int GetSMSCount(Database db) => GetSMSs(db)?.Count ?? 0;
    }

    public class Cookie
    {
        public DateTime Expire;
        public object value;
        public bool IsExpire => DateTime.Now.Ticks > Expire.Ticks;

        public Cookie(object msg, DateTime expire)
        {
            value = msg;
            Expire = expire;
        }
    }
    
}