using System;
using models;

namespace Server.Services
{
    [QServer.Core.HosteableObject(typeof(Users),null)]
    public class  Users :Service
    {
        public Users ():base("Users")
        {

        }
        private string parse(string param, out long g, out int i)
        {
            i = -1;
            g = 0;
            if (int.TryParse(param, out i)) return null;
            if (long.TryParse(param, out g)) return null;
            return param;
        }
        public override bool CheckAccess(RequestArgs args)
        {
            return args.User.IsAgent;
        }
        public override bool Get(RequestArgs args)
        {
            if (!args.User.IsAgent) return false;
            var unv = args.GetParam("Valide");
            
            if (unv != null)
            {
                var rt = new Logins(args.Client);
                bool unvld;
                if (!bool.TryParse(unv, out unvld)) return false;
                foreach (var p in args.Database.Logins.AsList())
                {
                    var c = (p.Value) as Login;
                    if (c.IsValidated==unvld) rt.Add(c);
                }
                args.JContext.Add(typeof(Logins), new DataTableParameter(typeof(Logins)) { SerializeItemsAsId = false, FullyStringify = true });
                args.Send(rt);
                return false;
            }
            var rp = args.GetParam("Path");
            string[] path;
            if (rp == null)
            { args.Send(args.Database.Logins); return false; }
            path = rp.Split('/');
            rp = path[0];
            long g;
            int i;
            Client t;
            if (int.TryParse(rp, out i))
                t = args.Database.Clients[i];
            else if (DataRow.TryParse(rp, out g))
                t = args.Database.Clients[g];
            else return false;

            if (path.Length > 1)
            {
                var x = t[path[1]];
                if (path.Length > 2)
                {
                    rp = parse(path[2], out g, out i);
                    var row = x as DataRow;
                    if (row != null)
                        args.Send(row[rp]);
                    else if (x is DataTable)
                    {
                        if (i != -1)
                            args.Send(((DataTable)x).AsList()[i].Value);
                        else if (g != 0)
                            args.Send(((DataTable)x).AsList()[g].Value);
                        else args.Send(((DataTable)x)[rp]);
                    }
                    else return false;

                }
                else
                {
                    args.Send(x);
                }
            }
            else
            {
                args.Send(t);
            }
            return true;
        }

        public override bool SUpdate(RequestArgs args)
        {
            if (!args.User.IsAgent) return args.SendFail();
            return args.Database.Clients.SendUpdates(args);
        }
    }
}