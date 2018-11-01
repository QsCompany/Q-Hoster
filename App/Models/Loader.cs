using System;

namespace models
{
    static public class Loader
    {
        public static void DataRow() { }

        internal static void Product()
        {
            throw new NotImplementedException();
        }

        public static void Client() { if (Math.Abs(models.Client.DPAbonment) == -1) throw null; }
        public static void Login() { if (Math.Abs(models.Login.DPClient) == -1) throw null; }
        public static void Agent() { if (Math.Abs(models.Agent.DPPermission) == -1) throw null; }
        public static void Article() { if (Math.Abs(models.Article.DpCount) == -1) throw null; }
        public static void Fournisseur() { if (Math.Abs(models.Fournisseur.DPAddress) == -1) throw null; }
        public static void SFacture() { if (Math.Abs(models.SFacture.DPAchteur) == -1) throw null; }
        public static void Facture() { if (Math.Abs(models.Facture.DpDate) == -1) throw null; }
        public static void Versment() { if (Math.Abs(models.Versment.DPDate) == -1) throw null; }
        public static void Category() { if (Math.Abs(models.Category.DpBase) == -1) throw null; }
        //public static void Picture() { if (Math.Abs(models.Picture.DpImageUrl) == -1) throw null; }
        public static void BVersment() { if (Math.Abs(VersmentBase.DPDate) == -1) throw null; }

        internal static void Signout()
        {
            throw new NotImplementedException();
        }
    }
}
