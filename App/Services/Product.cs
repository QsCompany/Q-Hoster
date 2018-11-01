using System;
using Json;
using QServer.Core;
using Server;

namespace Api
{
    public class  Product : Service
    {
        public Product()
            : base("Product")
        {

        }
        //public override bool CheckAccess(RequestArgs args)
        //{
        //    return args.User.IsAdmin;
        //}
        public override bool CheckAccess(RequestArgs args)
        {
            return args.Method == "GET" ? true : args.User.IsAgent;
        }
        public override bool Create(RequestArgs args)
        {
            var p = new models.Product() { Id = models.DataRow.NewGuid(), Name = "New Product" };
            args.Send(p);
            return true;
        }
        public override bool Delete(RequestArgs args)
        {
            var id = args.Id;
            var d = args.Database;
            var prd = d.Products[id];
            if (prd == null) return args.SendError(CodeError.product_not_exist);
            {
                var arts = d.Articles.AsList();
                for (int i = arts.Length - 1; i >= 0; i--)
                {
                    var t = (models.Article)arts[i].Value;
                    if (t.Product?.Id == id) return args.SendError($"This Product Cannot be delleted <br> The a facture with Id = {t?.Facture?.Id} contain this product ");
                }
            }
            {
                var arts = d.FakePrices.AsList();
                for (int i = arts.Length - 1; i >= 0; i--)
                {
                    var t = (models.FakePrice)arts[i].Value;
                    if (t.Product?.Id == id && t.Facture!=null) return args.SendError($"This Product Cannot be delleted <br> There a sel facture  with Id = {t.Facture.Id} contain this product ");
                }
            }
            if (!d.Delete(prd))
                return args.SendError(CodeError.fatal_error);
            d.Products.Remove(id);
            return args.SendSuccess();
        }
        public override bool Get(RequestArgs args)
        {
            var id = args.Id;
            if (id == -1) return args.CodeError(404);
            var e = args.Database.Products[id];
            args.Send(e);
            return true;
        }

        private new static bool RequireNew(string n, JObject x) { return n == "models.Product"; }
        public override bool Post(RequestArgs args)
        {
            if (args.GetParam("Operation", out string operation) && operation == "AVATAR")
                return this.SaveAvatar(args);
            bool isNew;
            args.JContext.RequireNew = RequireNew;
            var prd = args.BodyAsJson as models.Product;
            if (prd == null) return args.SendFail();
            if (!prd.Check(args, out isNew)) return false;
            var d = args.Database;

            if (d.Save(prd, !isNew))
            {
                if (isNew)
                    d.Products.Add(prd);
                else d.Products[prd.Id].CopyFrom(prd);
                return args.SendSuccess();
            }
            return args.SendFail();
        }

        private bool SaveAvatar(RequestArgs args)
        {
            var dd = args.BodyAsBytes;
            if (args.GetParam("Name", out string Name) && args.GetParam("Size", out int Size) && args.GetParam("PID", out long PID))
            {
                if (dd.Length != Size)
                    return args.SendInfo("Le fichier est corruptue", false);
                var p = args.Database.Products[PID];
                if (p == null) return args.SendInfo("Le Produit Peut etre supprimez",false);
                var md5 = CalculateMD5(dd);
                var file = md5 ?? models.DataRow.NewGuid().ToString() + "_" + Name;
                try
                {
                    var fio = new System.IO.FileInfo("./images/" + file);
                    System.IO.File.WriteAllBytes(fio.FullName, dd);
                    var st = p.SaveStat();
                    p.Picture = file;
                    if (args.Database.Save(p, true))
                    {
                        args.Send(JString.StringifyString(p.Picture));
                        return true;
                    }
                    p.Restore(st);
                    return args.SendFail();
                }
                catch (Exception e)
                {
                    return args.SendError(CodeError.fatal_error);
                }
            }
            return true;
        }
        static string CalculateMD5(byte[] bytes)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                {
                    var hash = md5.ComputeHash(bytes, 0, bytes.Length);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
    
}