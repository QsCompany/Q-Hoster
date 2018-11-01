namespace Api
{
    /*
    public class  Person : Service
    {
        public Person()
            : base("Person")
        {

        }
        public override void Get(RequestArgs args)
        {
            args.JContext.Add(typeof(models.Clients), new DObjectParameter(typeof(models.Clients)) { DIsFrozen = true });
            var t = args.Database.Persons;
            var id = args.GetParam("Id");
            if (id == null) return;
            var g = models.DataRow.Parse(id);
            var p = t[g];
            if (p == null)
                args.SendFail();
            else
                args.Send(p);
        }
        public override bool Post(RequestArgs args)
        {
            args.JContext.RequireNew = (type, t) => type != "models.Client";
            var per = args.BodyAsJson as models.Client;
            var oper = (models.Client)null;
            var ism = false;
            if (per == null)
            {
                args.SendError("costumer_argnull");
                return;
            }
            else if ((oper =args.Client.Costumers[per.Id]) != null)
            {
                if(per.Owner!=null)
                    if (per.Owner.Id != args.Client.Id)
                    {
                        args.SendError(CodeError.propably_hacking);
                        return;
                    }
                ism = true;
            }
            per.Owner = args.Client;
            List<models.Client.Message> ss;
            if (!per.Check(out ss))
            {
                StringBuilder r = new StringBuilder();
                foreach (var m in ss)
                {
                    r.Append("<p><h4 style='color:Yellow'>")
                        .Append(m.Title)
                        .Append("</h4><h5 class='msg-body' style='padding-left:20px'>")
                        .Append(m.Body)
                        .Append("</h5></p>");
                }
                args.SendError(r.ToString(), false);
                return;
            }            
            if (ism)
                oper.CopyFrom(per);
            else
            {
                args.Client.Costumers.Add(per);
                args.Database.Persons.Add(per);
            }
            args.Database.Save(oper ?? per, ism);            
            args.SendSuccess();
        }

        public override void Delete(RequestArgs args)
        {
            bool exp;
            var m = args.Client.GetCookie("delete_person", true,out exp) as models.Message;
            if (m == null|| exp)
            {
                args.Client.SetCookie("delete_person",
                    m =
                        args.SendConfirm("Confirm---" + (exp ? "Expired" : ""), "<input type='text'></input>", "DELETE",
                            "CLOSE"),
                    DateTime.Now + TimeSpan.FromMinutes(0.3));
                QServer.Serialization.MessageSerializer.Register(m);
                return;
            }
           
            if (m.Action != "ok")
            {
                args.SendFail();
                return;
            }
                        var id = args.Id;
            if(id==-1) return;
            var per = args.Client.Costumers[id];
            if (per == null)
            { args.SendSuccess(); return; }
            foreach (var c in args.Client.Factures.AsList())
            {
                if ((c.Value as models.Facture).For.Id != per.Id) continue;
                args.SendError(CodeError.person_undeleted);
                return;
            }
            args.Database.Delete(per);
            args.Client.Costumers.Remove(per);
            args.SendSuccess();
        }
    }*/
}