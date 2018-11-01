using System;
using Json;
using Server;
using QServer.Core;

namespace models
{
    abstract public class VersmentBase : DataRow, IHistory
    {
        public new static int __LOAD__(int dp) => DpMontant;
        public static int DpType = Register<VersmentBase, VersmentType>("Type"); public VersmentType Type
        {
            get => get<VersmentType>(DpType);
            set => set(DpType, value);
        }
        public static int DpMontant = Register<VersmentBase, float>("Montant", "DECIMAL(15,2)");
        public float Montant
        {
            get => get<float>(DpMontant);
            set => set(DpMontant, value);
        }

        public static int DPDate = Register<VersmentBase, DateTime>("Date"); public DateTime Date
        {
            get => get<DateTime>(DPDate);
            set => set(DPDate, value);
        }

        public readonly static int DPCassier = Register<VersmentBase, Agent>("Cassier", PropertyAttribute.AsId, null, (d, c) => ((Database)d).GetAgent(c));
        public Agent Cassier { get { return get<Agent>(DPCassier); } set { set(DPCassier, value); } }

        public static readonly int DPObservation = Register<VersmentBase, string>("Observation", PropertyAttribute.None, null, null, "nvarchar(155)");
        public string Observation
        {
            get => get<string>(DPObservation);
            set => set(DPObservation, value);
        }



        public readonly static int DPRef = Register<VersmentBase, string>("Ref", PropertyAttribute.None, null, null, "nvarchar(10)");
        public string Ref { get { return get<string>(DPRef); } set { set(DPRef, value); } }


        public VersmentBase(Context c, JValue jv)
            : base(c, jv)
        {
        }
        public VersmentBase()
        {
        }
        public virtual bool Check(RequestArgs args)
        {
            if (Type != VersmentType.Espece) args.SendError("L'option de payment " + Type + " N'est pas Implimenter");
            if (this.Montant == 0) return args.SendError("Le Versment Soit Superieur à 0 out Inferieur à 0  ", false);
            if (Observation != null && Observation.Length > 255) return args.SendError("Observation et tree long . 255 char est le maximum");
            return true;
        }

    }

    [QServer.Core.HosteableObject(typeof(Api.Versment), typeof(VersmentSerializer))]
    public class  Versment : VersmentBase
    {
        public new static int __LOAD__(int dp) => DPClient;
        public readonly static int DPClient = Register<Versment, Client>("Client", PropertyAttribute.AsId, null, (d, c) => ((Database)d).GetClient(c));
        public Client Client { get { return get<Client>(DPClient); } set { set(DPClient, value); } }

        public readonly static int DPPour = Register<Versment, Client>("Pour", PropertyAttribute.AsId, null, (d, c) => ((Database)d).GetClient(c));
        public Client Pour { get { return get<Client>(DPPour); } set { set(DPPour, value); } }

        public readonly static int DPFacture = Register<Versment, Facture>("Facture", PropertyAttribute.AsId, null, (d, c) => ((Database)d).GetFacture(c));
        public Facture Facture { get { return get<Facture>(DPFacture); } set { set(DPFacture, value); } }

        public override int Repaire(Database db)
        {

            if (this.Client == null)
            {
                if (Facture != null)
                {
                    Client = Facture.Client;
                    if (Client != null)
                        db.Save(this, true);
                }
            }
            return this.Client == null ? 1 : 0;
        }

        public Versment(Context c, JValue jv) : base(c, jv)
        {

        }
        public Versment()
        {
        }


        public override JValue Parse(JValue json)
        {
            return json;
        }

        public override bool Check(RequestArgs args)
        {
            if (!base.Check(args)) return false;
            if (Client == null) return args.SendError("Le Fournisseur doit etre selectioner");
            return true;
        }
        public static bool Save(RequestArgs args)
        {
            var vers = args.BodyAsJson as Versment;
            if (vers == null) return args.SendError(CodeError.EmtyRequest);
            var d = args.Database;
            var orgVers = d.Versments[vers.Id];
            var monatant = vers.Montant;
            Versment current;
            Client orgFrns, newFrns = vers.Client;
            if (orgVers != null)
            {
                orgFrns = orgVers.Client;
                if (orgFrns != newFrns)
                {
                    orgFrns.MontantTotal -= orgVers.Montant;
                    d.Save(orgFrns, true);
                }
                else
                {
                    monatant -= orgVers.Montant;
                }
                current = orgVers;
            }
            else current = vers;

            var c = current.Client;
            c.VersmentTotal += monatant;
            if (d.Save(c, true))
            {
                if (orgVers != null)
                    orgVers.CopyFrom(vers);
                current.Cassier = args.User.Agent;
                d.Save(current, true);

                if (orgVers == null)
                    d.Versments.Add(current);
                return true;
            }
            return args.SendError(CodeError.fatal_error);
        }
        //public static bool Delete(RequestArgs args)
        //{
        //    var id = args.Id;
        //    if (id == -1) return args.SendError("Peut Etre Le Versment N'exist pas ou est supprimer");
        //    var d = args.Database;
        //    var orgVers = d.Versments[id];
        //    var c = orgVers.Client;
        //    c.VersmentTotal -= orgVers.Montant;
        //    if (d.Save(c, true))
        //    {
        //        d.Delete(orgVers);
        //        d.Versments.Remove(orgVers);
        //        return true;
        //    }
        //    return args.SendError(CodeError.DatabaseError);
        //}
        public bool Delete(RequestArgs args)
        {
            var d = args.Database;
            var c = this.Client;
            c.VersmentTotal -= Montant;

            switch (d.Save(d.CreateOperations(SqlOperation.Update, c).Add(SqlOperation.Delete, this)))
            {
                case true:
                    d.Versments.Remove(this);
                    return true;
                case false:
                case null:
                    c.VersmentTotal += Montant;
                    return false;
                
            }
            return false;
        }
    }

    [HosteableObject(typeof(Api.SVersment), typeof(SVersmentSerializer))]
    public class  SVersment : VersmentBase
    {
        public readonly static int DPFournisseur = Register<SVersment, Fournisseur>("Fournisseur", PropertyAttribute.AsId, null, (d, c) => ((Database)d).GetFournisseur(c));
        public Fournisseur Fournisseur { get { return get<Fournisseur>(DPFournisseur); } set { set(DPFournisseur, value); } }

        public readonly static int DPFacture = Register<SVersment, SFacture>("Facture", PropertyAttribute.AsId, null, (d, c) => ((Database)d).GetSFacture(c));
        public SFacture Facture { get { return get<SFacture>(DPFacture); } set { set(DPFacture, value); } }
        
        public new static int __LOAD__(int dp) => DPFournisseur;
        
        public override bool Check(RequestArgs args)
        {
            if (!base.Check(args)) return false;
            if (Fournisseur == null) return args.SendError("Le Fournisseur doit etre selectioner");
            return true;
        }

        public SVersment(Context c, JValue jv) : base(c, jv)
        {

        }
        public SVersment()
        {
        }
        public override JValue Parse(JValue json)
        {
            return json;
        }
        public bool Delete(RequestArgs args)
        {
            var d = args.Database;
            var c = this.Fournisseur;
            c.VersmentTotal -= Montant;
            switch (d.Save(d.CreateOperations(SqlOperation.Update, c).Add(SqlOperation.Delete, this)))
            {
                case true:
                    d.SVersments.Remove(this);
                    return true;
                case false:
                case null:
                    c.VersmentTotal += Montant;
                    return false;

            }
            //if (d.StrictSave(c, true))
            //{
            //    if (d.Delete(this))
            //    {
            //        d.Versments.Remove(this);
            //        return true;
            //    }
            //}
            return false;
        }


        public static bool Save(RequestArgs args)
        {
            var vers = args.BodyAsJson as SVersment;
            if (vers == null) return args.SendError(CodeError.EmtyRequest);
            var d = args.Database;
            var orgVers = d.SVersments[vers.Id];
            var monatant = vers.Montant;
            SVersment current;
            Fournisseur orgFrns, newFrns = vers.Fournisseur;
            if (orgVers != null)
            {
                orgFrns = orgVers.Fournisseur;
                if (orgFrns != newFrns)
                {
                    orgFrns.MontantTotal -= orgVers.Montant;
                    d.Save(orgFrns, true);
                }
                else
                {
                    monatant -= orgVers.Montant;                    
                }
                current = orgVers;
            }
            else current = vers;

            var c = current.Fournisseur;
            c.VersmentTotal += monatant;

            if (d.Save(c, true))
            {
                if (orgVers != null)
                    orgVers.CopyFrom(vers);
                current.Cassier = args.User.Agent;
                d.Save(current, true);

                if (orgVers == null)
                    d.SVersments.Add(current);
                return true;
            }
            return args.SendError(CodeError.fatal_error);
        }

        public override int Repaire(Database db)
        {
            return this.Facture == null || this.Fournisseur == null ? 1 : 0;
        }
        //public static bool Delete(RequestArgs args,SVersment orgVers)
        //{
        //    return orgVers.Delete(args);
        //}
    }
}