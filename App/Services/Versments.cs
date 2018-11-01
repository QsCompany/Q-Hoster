using models;
using Server;
using System.Collections.Generic;

namespace Api
{
    public class  Versments : Service
    {
        public Versments() : base("Versments")
        {

        }

        public override bool Get(RequestArgs args)
        {
            bool? reslt = null;
            var source = args.Database.Versments;
            var query = args.GetParam("q");
            if (query != null)
                foreach (var q in query.Split('&'))
                    if (reslt == false) break;
                    else
                        switch (q)
                        {
                            case "Client":
                                reslt = GetVersmentsClient(args, ref source);
                                break;

                            case "Period":
                                reslt = GetVersmentsPeriod(args, ref source);
                                break;

                            case "Facture":
                                reslt = GetVersmentsFacture(args, ref source);
                                break;

                            case "Pour":
                                reslt = GetVersmentsPour(args, ref source);
                                break;

                            case "Cassier":
                                reslt = GetVersmentsCassier(args, ref source);
                                break;
                            case "Observation":
                                reslt = GetVersmentsObservation(args, ref source);
                                break;
                            default:
                                return args.SendFail();
                        }
            else if (args.Id != -1) return GetVersmentsClient(args, ref source);

            if (reslt == false)
                return args.SendFail();
            if (!args.HasParam("total"))
            {
                args.JContext.Add(typeof(models.Versments), p1);
                args.Send(source);
            }
            else args.Send(source.Total);
            return true;
        }
        static DataTableParameter p1 = new DataTableParameter(typeof(models.Versments)) { FullyStringify = true, SerializeItemsAsId = false };
        private bool? GetVersmentsObservation(RequestArgs args, ref models.Versments source)
        {
            var id = args.GetParam("Observation");
            if (id == null) return args.SendFail();
            if (id == "") return true;
            var t = new models.Versments(null);
            foreach (KeyValuePair<long, DataRow> x in source)
            {
                var sv = (models.Versment)x.Value;
                if (sv.Observation?.Contains(id) == true) t.Add(sv);
            }
            source = t;
            return true;
        }

        private bool? GetVersmentsPeriod(RequestArgs args, ref models.Versments source)
        {

            var period = args.GetParam("Period");
            long from = 0, to = 0;
            if (period != null)
            {
                if (long.TryParse(args.GetParam("From"), out from) && long.TryParse(args.GetParam("To"), out to))
                    goto bg;
            }
            return args.SendFail();
            bg:
            var t = new models.Versments(null);
            foreach (KeyValuePair<long, DataRow> x in source)
            {
                var sv = (models.Versment)x.Value;
                var d = sv.Date.Ticks;
                if (d >= from && d <= to)
                    t.Add(sv);
            }
            source = t;
            return true;
        }

        private bool? GetVersmentsPour(RequestArgs args, ref models.Versments source)
        {
            var pour = args.GetParam("Pour");
            if (pour == null || !long.TryParse(pour, out var id)) return args.SendFail();
            var t = new models.Versments(null);
            foreach (KeyValuePair<long, DataRow> x in source)
            {
                var sv = (models.Versment)x.Value;
                if (sv.Pour?.Id == id) t.Add(sv);
            }
            source = t;
            return true;
        }

        private bool? GetVersmentsCassier(RequestArgs args, ref models.Versments source)
        {
            var _id = args.GetParam("Cassier");
            if (_id == null || !long.TryParse(_id, out var id)) return args.SendFail();
            var fact = args.Database.Agents[id];
            if (fact == null) return args.SendError("Cette Facture N'exist pas");
            return fact.GetVersments(ref source);
        }        

        private bool? GetVersmentsFacture(RequestArgs args, ref models.Versments source)
        {
            var facture = args.GetParam("Facture");
            if (facture == null || !long.TryParse(facture, out var id)) return args.SendFail();
            var fact = args.Database.Factures[id];
            if (fact == null) return args.SendError("Cette Facture N'exist pas");

            return fact.GetVersments(ref source);
        }

        private static bool GetVersmentsClient(RequestArgs args, ref models.Versments source)
        {
            if (!long.TryParse(args.GetParam("Client") ?? args.GetParam("Id"), out var id) || id == -1) return args.SendFail();
            var f = args.Database.Clients[id];
            if (f == null) return args.SendError("Cette Facture N'exist pas");
            return f.GetVersments(ref source);
        }
    }
    public class  SVersments : Service
    {
        public SVersments() : base("SVersments")
        {

        }

        public override bool CheckAccess(RequestArgs args)
        {
            var agent = args.User.Agent;
            return args.User.IsAgent || (args.User.Permission & AgentPermissions.Cassier) == AgentPermissions.Cassier;
        }
        
        public override bool Get(RequestArgs args)
        {
            bool? reslt = null;
            var source = args.Database.SVersments;
            var query = args.GetParam("q");
            if (query != null)
                foreach (var q in query.Split('|'))
                    if (reslt == false) break;
                    else
                        switch (q)
                        {
                            case "Fournisseur":
                                reslt = GetVersmentsFournisseur(args, ref source);
                                break;

                            case "Period":
                                reslt = GetVersmentsPeriod(args, ref source);
                                break;

                            case "Facture":
                                reslt = GetVersmentsFacture(args, ref source);
                                break;

                            case "Cassier":
                                reslt = GetVersmentsCassier(args, ref source);
                                break;
                            case "Observation":
                                reslt = GetVersmentsObservation(args, ref source);
                                break;
                            default:
                                return args.SendFail();
                        }
            else if (args.Id != -1) { return GetVersmentsFournisseur(args, ref source); }

            if (reslt == false)
                return args.SendFail();
            if (!args.HasParam("total"))
            {
                args.JContext.Add(typeof(models.SVersments), p1);
                args.Send(source);
            }
            else args.Send(source.Total);
            return true;
        }
        static DataTableParameter p1 = new DataTableParameter(typeof(models.SVersments)) { FullyStringify = true, SerializeItemsAsId = false };
        private bool? GetVersmentsObservation(RequestArgs args, ref models.SVersments source)
        {
            var id = args.GetParam("Observation");
            if (id == null) return args.SendFail();
            if (id == "") return true;
            var t = new models.SVersments(null);
            foreach (KeyValuePair<long, DataRow> x in source)
            {
                var sv = (models.SVersment)x.Value;
                if (sv.Observation?.Contains(id) == true) t.Add(sv);
            }
            source = t;
            return true;
        }

        private bool? GetVersmentsCassier(RequestArgs args, ref models.SVersments source)
        {
            var _id = args.GetParam("Cassier");
            if (_id == null || !long.TryParse(_id, out var id)) return args.SendFail();
            var cassier = args.Database.Agents[id];
            if (cassier == null) args.SendError("Le Cassier N'exist pas");
            return cassier.GetSVersments(ref source);
        }

        private bool? GetVersmentsFacture(RequestArgs args, ref models.SVersments source)
        {
            var facture = args.GetParam("Facture");
            if (facture == null || !long.TryParse(facture, out var id)) return args.SendFail();
            var f = args.Database.SFactures[id];
            if (f == null) return args.SendError("La Facture N'exist pas");
            return f.GetSVersments(ref source);
            
        }
        private bool? GetVersmentsPeriod(RequestArgs args, ref models.SVersments source)
        {

            var period = args.GetParam("Period");
            long from = 0, to = 0;
            if (period != null)
            {
                if (long.TryParse(args.GetParam("From"), out from) && long.TryParse(args.GetParam("To"), out to))
                    goto bg;
            }
            return args.SendFail();
            bg:
            var t = new models.SVersments(null);
            foreach (KeyValuePair<long, DataRow> x in source)
            {
                var sv = (models.SVersment)x.Value;
                var d = sv.Date.Ticks;
                if (d >= from && d <= to)
                    t.Add(sv);
            }
            source = t;
            return true;
        }

        private static bool GetVersmentsFournisseur(RequestArgs args, ref models.SVersments source)
        {
            if (!long.TryParse(args.GetParam("Fournisseur") ?? args.GetParam("Id"), out var id) || id == -1) return args.SendFail();            
            var f = args.Database.Fournisseurs[id];
            if (f == null) return args.SendError("Le Client N'exist pas");
            return f.GetSVersments(ref source);
            
        }

    }
}