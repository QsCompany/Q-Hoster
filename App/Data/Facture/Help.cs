using Json;
using models;
using Server;
using System;
using System.Collections.Generic;

namespace QServer.Complex
{
    public static partial class FactureManager
    {

        public static bool DeleteFacture(RequestArgs args, Facture orgFacture)
        {
            if (orgFacture.Articles.Count != 0)
                return args.SendError("Delete Articles First");
            var cookieName = "delete_facture_" + orgFacture.Id;
            var @ref = new JObject() { { "Ref", (JString)orgFacture.Ref }, { "Id", (JNumber)orgFacture.Id } };
            var handler = new MessageHandler(ConfirmFactureDeleResponse, args.Id, args.Client.Id);
            var m = args.SendSpeech("Confirm",
                $"<section><h1>You are about to deleting the facture <span style='color:red'>{orgFacture.Ref}</span></h1>" +
                $"<br><h3>Do you want realy to delete this facture </h3></section>", "Delete", "Cancel", null, false, @ref, handler);

            args.Client.SetCookie(cookieName, m, DateTime.Now + TimeSpan.FromSeconds(15));
            return false;
        }

        private static bool ConfirmFactureDeleResponse(RequestArgs args, Message msg, MessageHandler handler)
        {
            var f = args.Database.Factures[(long)handler.Params[0]];
            var client = args.Database.Clients[(long)handler.Params[1]];
            var cvv = args.Client.GetCookie("delete_facture_" + f.Id + "confirmed", true, out var expired);
            if (expired) return args.SendAlert("Time Expired", "The Confirmation must be not more than 15s");
            if (msg.Action != "ok") return args.SendFail();
            var versments = args.Database.Versments;
            if (!f.HasVersments(ref versments, out var total))
                f.Delete(args);
            else
                args.SendSpeech("Confirmation", "This Fcture has Versment do you want to Delete Those Versments Or NOT", "DELETE", "KEEP", "ABORT", true, null, new MessageHandler(DeleteVersments, f.Id));

            return false;
        }

        private static bool DeleteVersments(RequestArgs args, Message msg, MessageHandler handler)
        {
            var f = args.Database.Factures[(long)handler.Params[0]];
            switch (msg.Action)
            {
                case "ok":
                    var versments = args.Database.Versments;
                    var x = f.HasVersments(ref versments, out var total);
                    foreach (var kv in versments.AsList())
                    {
                        var v = (Versment)kv.Value;
                        if (!v.Delete(args)) return args.SendError($"UnExpected Error While Deete Versment {v.Ref} : Total :{v.Montant}");
                    }
                    goto case "cancel";

                case "cancel":
                    if (f.Delete(args)) return args.SendSuccess();
                    else return false;
                case "abort":
                default:
                    return args.SendFail();
            }
        }
    }
    public static partial class FactureManager
    {
        public static bool ChangeClient(RequestArgs args, Facture f, Client client)
        {
            if (f.Client == client) return args.SendSuccess();
            var versments = args.Database.Versments;
            if (f.hasVersment(args.Database))
            {
                args.SendSpeech($"Transfer Versments", $"La facture has versments do you want to transfer those Versment to {client.Name ?? client.FullName}", "Transfer", "Keep", "Abort", true, null, new MessageHandler(TransferVersmentResponse, f.Id, client.Id));
                return true;
            }
            else
            {
                return TransferClient(args, f, null, client);
            }
        }

        private static bool TransferVersmentResponse(RequestArgs args, Message msg, MessageHandler handler)
        {
            var f = args.Database.Factures[(long)handler.Params[0]];
            var c = args.Database.Clients[(long)handler.Params[1]];
            var oc = f.Client;
            var vrsments = f.GetVersments(args.Database);
            var operation = args.Database.CreateOperations();
            switch (msg.Action)
            {
                case "ok":
                    var totVer = 0f;
                    var succ = true;
                    foreach (KeyValuePair<long, DataRow> lv in vrsments)
                    {
                        var ver = lv.Value as Versment;
                        ver.Client = c;
                        ver.Facture = null;
                        if (args.Database.Save(ver, true))
                            totVer += ver.Montant;
                        else
                        {
                            ver.Client = f.Client;
                            ver.Facture = f;
                            succ = false;
                        }
                    }
                    c.VersmentTotal += totVer;
                    oc.VersmentTotal -= totVer;
                    args.Database.Save(c, true);
                    args.Database.Save(oc, true);
                    if (!succ) return args.SendAlert("Error", "Il y a des versment ne pas supprimer", "Cancel", false);
                    return TransferClient(args, f, null, c);
                case "cancel":
                    return TransferClient(args, f, vrsments, c);
                default:
                    return args.SendFail();
            }
        }
        private static bool TransferClient(RequestArgs args, Facture f, Versments vrsments, Client c)
        {
            var succ = true;
            var oc = f.Client;
            if (vrsments != null)
                foreach (KeyValuePair<long, DataRow> lv in vrsments)
                {
                    var ver = lv.Value as Versment;
                    ver.Facture = null;

                    if (args.Database.Save(ver, true))
                        continue;
                    else
                        ver.Facture = f; succ = false;
                }
            if (succ)
            {
                f.Client = c;
                var oper = args.Database.CreateOperations(SqlOperation.Update, f);
                if (f.IsValidable)
                {
                    c.MontantTotal -= f.Total;
                    oc.MontantTotal += f.Total;
                    oper.Add(SqlOperation.Update, oc).Add(SqlOperation.Update, c);
                }
                if (args.Database.Save(oper) == true)
                    return args.SendSuccess();
                if (f.IsValidable)
                {
                    c.MontantTotal += f.Total;
                    oc.MontantTotal -= f.Total;
                }
                f.Client = oc;
                return args.SendFail();
            }
            else return args.SendAlert("Error", "Il y a des versment ne pas supprimer", "Cancel", false);
        }
    }
}
