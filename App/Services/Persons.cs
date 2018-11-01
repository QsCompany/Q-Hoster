namespace Api
{/*
    public class  Persons : Service
    {
        public Persons()
            : base("Persons")
        {

        }
        public override void Get(RequestArgs args)
        {
            if (!args.User.IsAdmin) { args.SendError(CodeError.access_restricted); return; }
            args.JContext.Add(typeof(models.Clients), new DataTableParameter(typeof(models.Clients)) { SerializeItemsAsId = false });
            var client = args.User.Client;
            var e = new models.Clients(client);
            foreach (var valuePair in client.Costumers.AsList())
                e.Push(valuePair.Value as models.Client);
            foreach (var valuePair in client.Factures.AsList())
                e.Push(((models.Facture)valuePair.Value).For as models.Client);

            args.Send(e);
        }
    }*/
}