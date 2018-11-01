using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using models;
using QServer.Services;
using Server;
using Api;
using Agent = models.Agent;
using Client = models.Client;
using Cookie = System.Net.Cookie;
using Login = models.Login;
using Message = Api.Message;
using User = Server.User;
using Console = MyConsole;

//[assembly: SuppressIldasm]

//[assembly: CompilerGenerated]
//[module: UnverifiableCode]

namespace QServer.Core
{
    public class AuthenticationManager
    {
        private Server server;
        private Database db;
        private static readonly Dictionary<string, User> NativeUsers = new Dictionary<string, User>();
        private static readonly Dictionary<string, User> Users = new Dictionary<string, User>();
        private readonly Dictionary<string, User> _connectedUsers = new Dictionary<string, User>();

        public static void RespondOptions(HttpListenerContext context)
        {
            var r = context.Response;
            r.AddHeader("Access-Control-Expose-Headers", "id");
            r.AddHeader("Access-Control-Allow-Headers", "*");
            r.AddHeader("Access-Control-Allow-Methods", "*");
            r.AddHeader("Access-Control-Max-Age", "1728000");
            r.AppendHeader("Access-Control-Allow-Origin", "*");
            r.AddHeader("Cache-Control", "public");
            r.Headers.Add("Last-Modified", DateTime.Now.ToString());
            r.Headers.Add("max-age", TimeSpan.FromDays(365).Ticks.ToString());
            r.Headers.Add(HttpResponseHeader.Expires, new DateTime(DateTime.Now.Ticks + TimeSpan.FromDays(365).Ticks).ToString());
        }

        public void PublicApi(HttpListenerContext context, string raw)
        {

            switch (context.Request.Url.LocalPath.ToLower())
            {
                case "/~checklogging":
                    IsLoged(context);
                    break;
                case "/~login":
                    Login(context);
                    break;
                case "/~signup":
                    Signup(context);
                    break;

                case "/~signout":
                    Signout(context);
                    break;
                case "/~newGuid":
                    Server.Send(context, (context.Response.ContentEncoding ?? context.Request.ContentEncoding ?? Encoding.UTF8).GetBytes(Guid.NewGuid().ToString()));
                    break;
                case "/~guid":
                    var r = string.Format(Server.SGuidService, GuidService.GetGuid());
                    Server.Send(context, (context.Response.ContentEncoding ?? context.Request.ContentEncoding ?? Encoding.UTF8).GetBytes(r));
                    break;
                case "/~issecured":
                    Server.Send(context, RequestArgs.https ? Server.True : Server.False);
                    break;
                case "/~sessionid":
                    Server.Send(context, (context.Response.ContentEncoding ?? context.Request.ContentEncoding ?? Encoding.UTF8).GetBytes(global::QServer.Services.SessionIdService.SessionId));
                    break;
                case "/~smscount":
                    var cc = getId(context);
                    User user = null;
                    if (cc != null && (_connectedUsers.TryGetValue(cc, out user)))
                    {
                        var bytes = Encoding.Default.GetBytes(user.Client.GetSMSCount(Database.__Default).ToString());
                        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    }
                    break;
                case "/~isadmin":
                    cc = getId(context);
                     user = null;
                    if (cc != null && (_connectedUsers.TryGetValue(cc, out user)))
                    {    
                        if (user.IsBlocked)
                        {
                            _connectedUsers.Remove(cc);
                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            return;
                        }
                        if (context.Request.RemoteEndPoint.Address.GetHashCode() != user.Address.GetHashCode())
                        {
                            if (AnotherAccountIsStillOpened(context, user))
                                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                         
                            user.Address = context.Request.RemoteEndPoint.Address;
                        }
                        using (var rr = RequestArgs.NewRequestArgs(context, this.server, user))
                            if (user.IsAgent)
                                rr.SendSuccess();
                            else
                                rr.SendFail();
                    }
                    break;
                default:
                    if (raw.StartsWith("/~$?id") || raw.StartsWith("/~%24?"))
                        Downloader.Send(context);
                    else
                        context.Response.StatusCode = 404;
                    break;
            }
            context.Response.Close();

        }

        public AuthenticationManager(Server server)
        {
            this.server = server;
        }
        private void IsLoged(HttpListenerContext context)
        {
            var cc = getId(context);
            if (cc != null && (_connectedUsers.TryGetValue(cc, out User user)) && user.IsLogged)
            {
                user.LastAccess = DateTime.Now;
                context.Response.Close(Server. True, true);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentLength64 =Server. False.Length;
                context.Response.OutputStream.Write(Server.False, 0, Server.False.Length);
            }
        }

        public bool Logout(Client client, RequestArgs args)
        {
            return _connectedUsers.Remove(args.User.UserName);
        }

        public bool ValidateUser(RequestArgs args, Login e)
        {
            var oe = args.Database.Logins[args.Id];
            if (oe == null) return args.SendError("Login Is not exist");
            oe.IsValidated = true;
            oe.IsThrusted = true;
            if (args.Database.Save(oe, true))
            {
                var u = oe.InitUser();
                if (Users.ContainsKey(oe.Username) == false)
                    Users.Add(u.UserName, u);
                return args.SendSuccess();
            }
            return args.SendFail();
        }
        public User RegisterLogin(Login l)
        {
            if (Users.TryGetValue(l.Username, out var ouser))
                Users.Remove(l.Username);
            User user;
            Users.Add(l.Username, user = l.InitUser());
            if (ouser != null)
            {
                user.CurrentId = ouser.CurrentId;
                user.Identification = ouser.Identification;
            }
            return user;
        }
        public User UnRegisterLogin(Login l)
        {
            if (Users.TryGetValue(l.Username, out User user))
                Users.Remove(l.Username);
            return user;
        }
        //Static AuthenticationManager() => SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
        public bool DeleteUser(RequestArgs args, Login e)
        {
            var oe = args.Database.Logins[args.Id];
            if (oe == null) return args.SendError("Login Is not exist");
            var user = oe.User ?? oe.InitUser();
            if (user.IsAgent) return args.SendError("This Account Cannot be locked");
            oe.IsLogged = false;
            oe.IsThrusted = false;
            oe.IsValidated = false;
            if (oe != null) return args.SendFail();
            if (args.Database.Delete(oe))
            {
                args.Database.Logins.Remove(oe);
                if (Users.ContainsKey(oe.Username))
                    Users.Remove(oe.Username);
                if (_connectedUsers.ContainsKey(oe.Username))
                    _connectedUsers.Remove(oe.Username);
                return args.SendSuccess();
            }
            return args.SendFail();
        }

        public bool LockUser(RequestArgs args, Login e)
        {
            {
                var oe = args.Database.Logins[args.Id];
                if (oe == null) return args.SendError("Login Is not exist");
                var user = oe.User ?? oe.InitUser();
                if (user.IsAgent) return args.SendError("This Account Cannot be locked");
                oe.IsThrusted = false;
                oe.IsValidated = false;
                oe.IsLogged = false;
                if (args.Database.Save(oe, true))
                {
                    if (Users.TryGetValue(oe.Username, out user))
                    {
                        user.CurrentId = null;
                        user.IsLogged = false;
                        Users.Remove(oe.Username);
                        if (_connectedUsers.ContainsKey(oe.Username))
                            _connectedUsers.Remove(oe.Username);
                    }
                    return args.SendSuccess();
                }
            }
            return args.SendFail();
        }
        private Agent AddAdminLogin(Database database)
        {


            Agent agent = null;
            Client client;

            var d = database;
            client = CreateClient(database);
            client.Job = Job.All;
            client.Abonment = Abonment.All;

            agent = new Agent
            {
                Id = DataRow.NewGuid(),
                IsDisponible = true,
                Permission = AgentPermissions.Admin,
                Client = client,
                IsThrusted = true,
                IsValidated = true,
                //Name = ReadString(6, "Enter Admin Name"),
                //Username = ReadString(8, "Enter Username"),
                //Pwd = ReadString(10, "Enter Password"),
            };
            LoginEdit.Edit(agent, true);

            deb:
            if (Users.ContainsKey(agent.Username))
            {
                agent.Username = ReadString(8, "Enter Username");
                goto deb;
            }
            User user = agent.InitUser();
            Users.Add(user.UserName, user);
            if (d.Save(client, false))
            {
                d.Clients.Add(client);
                if (d.Save(agent, false))
                    d.Agents.Add(agent);
                else
                    goto fatal;
            }
            else goto fatal;
            AdminUser = user;
            Console.WriteLine("Admin Account Created Successfully");
            ret:
            d.Clients.Add(client);
            d.Agents.Add(agent);

            Console.WriteLine(new string('*', Console.BufferWidth - 1));
            Console.WriteLine(new string('*', Console.BufferWidth - 1));

            WriteUser(user);
            return agent;


            fatal:
            Console.WriteLine("\t\t\tClient Unsaved **** UnExpected Error (Contact Achour Brahim)");
            Thread.CurrentThread.Abort(404);
            goto ret;
        }
        
        public bool SignupAgent(RequestArgs args, Agent l)
        {
            l.RegeneratePwd(string.IsNullOrWhiteSpace(args.User.CurrentId) ? "new_account" : args.User.CurrentId);
            var d = args.Database;
            var ol = d.Agents[l.Id];
            if (ol != null)
            {
                if (string.IsNullOrWhiteSpace(l.Pwd))
                {
                    l.Pwd = ol.Pwd;
                    l.IsThrusted = ol.IsThrusted;
                    l.Identification = ol.Identification;
                }

                if (ol == this.AdminUser.Agent)
                {
                    l.Permission = ol.Permission;
                    l.IsThrusted = true;
                    l.IsValidated = true;
                    l.Client = ol.Client;
                    l.IsLogged = ol.IsLogged;
                }
            }
            if (!l.Check(args)) return false;
            if (ol == null)
            {
                var un = l.Username;
                foreach (var ls in d.Agents.AsList())
                    if (((Login)ls.Value).Username == un)
                        return args.SendError("Another User Is Used This Username <br><hl>Please Contact Administrator  ", false);
            }
            else
            {
            }

            l.IsValidated = true;
            if (!d.Save(l, ol != null))
                args.SendError(CodeError.fatal_error);
            if (ol != null)
                ol.CopyFrom(l);
            else
                d.Agents.Add(l);
            this.RegisterLogin(ol ?? l);
            return true;
        }
        public bool Signup(RequestArgs args, Login x, out User user)
        {
            var d = args.Database;
            user = null;
            var ox = d.Logins[x.Id];
            if (ox == x)
            {
                x = new models.Login();
                x.CopyFrom(ox);
            }
            x.Id = DataRow.NewGuid();
            if (ox != null) x.Id = DataRow.NewGuid();
            x.RegeneratePwd("eval(code)");
            List<Client.Message> k = new List<Client.Message>();
            if (!x.Check(args, ref k)) goto sndk;

            #region Check Client
            var c = x.Client;
            var oc = d.Clients[c.Id];
            if (string.IsNullOrWhiteSpace(c.Name)) c.Name = x.Username;
            if (!c.Check(out k)) goto sndk;
            if (c == oc)
            {
                c = new Client();
                c.CopyFrom(oc);
                x.Client = c;
            }
            c.Id = DataRow.NewGuid();

            #endregion
            if (Users.TryGetValue(x.Username, out user)) return args.SendError("This user exist ", false);

            var un = x.Username;
            var tel = x.Client.Tel;
            foreach (var ls in d.Logins.AsList())
            {
                var l = ls.Value as Login;
                if (l.Username == un) return args.SendError("This user exist ", false);
            }
            x.IsValidated = false;
            x.IsThrusted = false;
            x.IsLogged = false;
            x.Identification = null;

            if (!d.Save(x.Client, false) || !d.Save(x, false))
                args.SendError(CodeError.fatal_error);
            d.Clients.Add(x.Client);
            d.Logins.Add(x);
            return true;
            sndk:
            return args.SendError(Client.Message.ToStrin(k));
        }

        private User Signup(HttpListenerContext context)
        {
            var cc = getId(context);
            var args = RequestArgs.NewRequestArgs(context, this.server, null);
            args.JContext.RequireNew = (type, t) => true;
            var x = args.BodyAsJson as Login;
            if (Signup(args, x, out User user)) args.SendSuccess();
            return user;
        }

        public void Dispose()
        {
            _connectedUsers.Clear();
        }
        private User AdminUser;
        public Client Admin => AdminUser.Client;
        private Client CreateClient(Database database)
        {
            var client = new Client() { Id = DataRow.NewGuid() };

            Console.WriteLine("Becarfull : Vous supprimer le client Administrteur .");
            Console.WriteLine("Resaiser le :");
            ClientEdit.Show(client, true);
            if (false)
            {
                client.FirstName = ReadString(6, "Firstname", GlobalRegExp.name);
                client.LastName = ReadString(6, "LastName", GlobalRegExp.name);
                client.Tel = ReadString(6, "Tel ", GlobalRegExp.telF, GlobalRegExp.telM);
                client.Email = ReadString(6, "Email ", GlobalRegExp.mail);
            }
            if (!client.Check(out string error)) { Console.WriteLine(error); }
            if (!database.Save(client, false))
            {
                Console.WriteLine("The Client cannot fit in the Database");
            }
            return client;
        }

        public void UpdateLogins(Database database)
        {
            bool hasAdmin = false;
            foreach (var l in database.Logins.AsList())
            {

                var lg = l.Value as Login;

                if (lg.Client == null)
                {
                    Console.WriteLine(new string('*', 70));
                    Console.WriteLine($"It's found that Client Info of Login {lg.Username}@{lg.Pwd} was deleted \r\n:please Re-Enter them");
                    Console.WriteLine(new string('*', 70));
                    lg.Client = CreateClient(database);
                    Console.WriteLine(new string('*', 70));
                    Console.WriteLine(new string('*', 70));
                }
                if (lg.IsValidated)
                    if (!Users.ContainsKey(lg.Username))
                    {
                        User user;
                        Users.Add(lg.Username, user = lg.InitUser());
                        WriteUser(user);
                    }
                    else Console.Write($"The {{ User: '{lg.Username}' , Password :'{lg.Pwd}',Tel :'{lg.Client.Tel}' }} is Confussed with another Account ");
            }

            foreach (var kv in database.Agents.AsList())
            {
                
                var agent = kv.Value as Agent;
                if (!agent.IsValidated && !agent.IsThrusted) continue;
                if (!Users.ContainsKey(agent.Username))
                {
                    User user;
                    Users.Add(agent.Username, user = agent.InitUser());
                    if (user.IsAgent && !hasAdmin) AdminUser = user;
                    hasAdmin |= user.IsAgent;
                    WriteUser(user);
                }
                else
                {

                }
            }

            if (!hasAdmin)
                AddAdminLogin(database);

        }

        void WriteUser(User user)
        {
            Console.WriteLine("Username:\"{0}\" \t\t Password:\"{1}\"\t\t IsAdmin:\"{2}\"", user.UserName, user.Password, user.IsAgent);
        }
        static string ReadString(int minLength, string label = null, params System.Text.RegularExpressions.Regex[] regex)
        {
            string iuser = null;
            do
            {
                if (label != null) Console.Write("\r\n" + label + "  :");
                iuser = Console.ReadLine();
                if (regex != null && regex.Length != 0)
                {
                    foreach (var r in regex)
                    {
                        if (!r.IsMatch(iuser))
                        {
                            Console.WriteLine("Invalid Characteres ");
                            iuser = null;
                        }
                        else break;
                    }
                }
                else if (!GlobalRegExp.username.IsMatch(iuser))
                {
                    Console.WriteLine("Invalid Characteres ");
                    iuser = null;
                }
            } while (iuser == null || iuser.Length < minLength);
            return iuser;
        }

        private bool GetUserFromCookie(RequestArgs args, out User user)
        {
            var login = (Login)args.BodyAsJson;
            var cc = getId(args.context);

            if (cc != null)
                if (_connectedUsers.TryGetValue(cc, out user) && user.AllowSigninById)
                {
                    if (cc == user.CurrentId)
                        return true;
                    foreach (Cookie ck in args.context.Request.Cookies)
                    {
                        ck.Expired = true;
                        args.context.Response.SetCookie(ck);
                    }
                    user.IsLogged = false;
                }
            user = null;
            return false;
        }
        private bool GetUserFromIdentAndData(RequestArgs args, out User user)
        {
            var login = (Login)args.BodyAsJson;
            if (login != null)
            {
                var identification = login.Identification;
                var username = login.Username;
                IPAddress ipaddress = IPAddress.None;

                var pssword = login.Pwd;
                deb:
                if (string.IsNullOrEmpty(username) == false)
                    if (Users.TryGetValue(username, out user))
                    {
                        return user.Password == pssword ? true : login.RegeneratePwd(user.Password);
                    }

                if (!string.IsNullOrEmpty(identification))
                {
                    var ds = RequestArgs.aes.Decrypt(identification).Split('\0');
                    if (ds.Length == 3)
                    {
                        username = ds[1];
                        pssword = ds[0];
                        if (IPAddress.TryParse(ds[2], out ipaddress))
                        {
                            identification = null;
                            goto deb;
                        }
                    }
                }
            }
            user = null;
            return args.SendAlert("Authentication", "Le Compt soit est desactiver ou est n'est pas enregistrer<br><br>Contacter l'admin", "OK", false);
        }

        

        private User Login(HttpListenerContext context)
        {
            var args = RequestArgs.NewRequestArgs(context, this.server, null);
            var ac = Message.ActionTaken;
            args.JContext.RequireNew = (x, b) => true;
            var login = args.BodyAsJson as Login;
            if (login == null)
            {
                if (args.Method == "GET" && args.HasParam("Identification") && args.HasParam("Id"))
                {
                    login = new Login() { Id = args.Id, Identification = args.GetParam("Identification") };
                    args.BodyAsJson = login;
                }
                else
                    return null;
            }
            //var cid = getId(context);
            if (GetUserFromIdentAndData(args, out User user) || GetUserFromCookie(args, out user))
            {
            }
            else { args.SendFail(); return null; }

            var ologin = user.Login;
            var ident = RequestArgs.aes.Encrypt(user.Password + "\0" + user.UserName + "\0" + context.Request.RemoteEndPoint.Address);
            login.Identification = ident;
            login.Client = ologin.Client;
            login.IsLogged = true;

            var cc = new Cookie("id", Guid.NewGuid().ToString());
            args.SetCookie("id", cc.Value);
            args.context.Response.AddHeader("id", cc.Value);
            context.Response.AddHeader("Access-Control-Expose-Headers", "id");
            if (user.CurrentId != null) _connectedUsers.Remove(user.CurrentId);
            _connectedUsers.Add(cc.Value, user);

            user.CurrentId = cc.Value;
            user.Address = context.Request.RemoteEndPoint.Address;
            user.IsLogged = true;
            args.JContext.Add(typeof(Client), new DObjectParameter(typeof(Client), true));
            login.Id = ologin.Id;
            args.Send(login);
            return null;
        }
        private static string DisposeService = "{\"__service__\":\"notfication\"}";
        private static byte[] DisposeServiceBytes = Encoding.UTF8.GetBytes(DisposeService);
        #region Region 1

        private static string getId(HttpListenerContext context)
        {
            var cc = context.Request.Cookies["id"]?.Value;
            if (cc != null) return cc;
            cc= context.Request.Headers["xreq"];
            if (cc == null) return null;
            try
            {
                cc = Encoding.UTF8.GetString(System.Convert.FromBase64String(cc));
            }
            catch { }
            return cc.IndexOf(':') == 2 ? cc.Substring(3) : null;
        }
        public User CheckAuth(HttpListenerContext context, out bool logged)
        {
            var chifisadmin = false;
            logged = false;
            {

                var cc = getId(context);// context.Request.Cookies["id"]?.Value ?? context.Request.Headers["id"];
                cc = cc ?? "79dd93e7-1669-42fd-a95d-b65b7ed078e6";
                User user;
                if (cc != null && (_connectedUsers.TryGetValue(cc, out user)))
                {
                    if (user.IsBlocked)
                    {
                        context.Response.OutputStream.Write(DisposeServiceBytes, 0, DisposeServiceBytes.Length);
                        context.Response.OutputStream.Close();
                        //_connectedUsers.Remove(cc.Value);
                        return null;
                    }
                    logged = user.IsLogged;
                    if (chifisadmin)
                    {
                        using (var rr = RequestArgs.NewRequestArgs(context, this.server, user))
                            if (user.IsAgent)
                            {
                                rr.SendSuccess();
                                return null;
                            }
                            else
                            {

                                rr.SendFail();
                                return null;
                            }
                    }
                    return user;
                }

                //var user1 = new User(models.Database._Default.Logins["achour"], true);
                //return user1;
                
                if (context.User == null)
                {
                    //context.Response.StatusCode = 401;
                    return null;
                }

                var id = context.User.Identity as HttpListenerBasicIdentity;
                if (id != null && Users.TryGetValue(id.Name, out user))
                {

                    //user.Password = "brahim";
                    if (true || user.Password == id.Password)
                    {
                        if (cc != null)
                        {
                            if (user.CurrentId == cc)
                            {
                                user.CurrentId = null;
                                user.IsLogged = false;
                                return user;
                            };
                            var xcc = new Cookie("id", "");
                            xcc.Expired = true;
                            context.Response.SetCookie(xcc);
                        }
                        logged = false;
                        return user;
                    }
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    var False = Encoding.UTF8.GetBytes("false");
                    context.Response.OutputStream.Write(False, 0, False.Length);
                    return null;
                }
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    var False = Encoding.UTF8.GetBytes("false");
                    context.Response.OutputStream.Write(False, 0, False.Length);
                    return null;
                }
            }
        }

        private bool AnotherAccountIsStillOpened(HttpListenerContext context, User user)
        {
            if ((DateTime.Now - user.LastAccess).TotalMinutes > 15)
                return false;

            var serviceArgs = RequestArgs.NewRequestArgs(context, this.server, user);
            var t = new SecurityAccountRequest
            {
                OriginalIP = user.Address?.ToString(),
                YourIP = context.Request.RemoteEndPoint.ToString(),
                Wait = 300000,
                IsSuccess = false
            };
            serviceArgs.Send(t);
            return true;
        }

        private User Signout(HttpListenerContext context)
        {
            var cc = getId(context);
            if (cc != null && (_connectedUsers.TryGetValue(cc, out User user)))
            {
                if (user.IsBlocked)
                {
                    _connectedUsers.Remove(cc);
                    return null;
                }
                if (context.Request.RemoteEndPoint.Address.GetHashCode() != user.Address.GetHashCode())
                {
                    context.Response.Redirect("chrome://www.islamway.com");
                    return null;
                }
                user.IsLogged = false;
                _connectedUsers.Remove(cc);
                user.Address = null;
                user.CurrentId = null;
                user.Login.IsLogged = false;
                user.Login.Identification = null;
                var c = RequestArgs.NewRequestArgs(context, this.server, user);
                c.Send(user.Login);
            }
            context.Response.Redirect("http://localhost");
            context.Response.RedirectLocation = "http://localhost";

            return null;
        }

        private User CheckBasicIdentity(HttpListenerContext context, HttpListenerBasicIdentity id)
        {
            throw new NotImplementedException();
        }

        private User CheckBasicIdentity(HttpListenerBasicIdentity id)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
