using QServer.Serialization;
using Server;
using System;
using System.Net;
//using TinifyAPI;

namespace Api
{    
    public class  Message : Service
    {
        public static string ActionTaken = "";
        public Message()
            : base("CallBack")
        {

        }

        public override bool Post(RequestArgs args)
        {
            args.JContext.RequireNew = (A, b) => true;
            var errr = args.BodyAsJson as models.Message;
            if (errr != null)
            {
                var om = MessageSerializer.GetRegistration(errr.Id);
                if (om == null)
                    return args.SendFail();
                om.Data = errr.Data;
                if (om.ResponseHandler != null)
                {
                    om.ResponseHandler.Action(args, om, om.ResponseHandler);
                }
            }
            return false;
        }
        public override bool Get(RequestArgs args)
        {
            var data = new byte[32] { 234, 23, 196, 234, 69, 238, 92, 244, 50, 110, 70, 181, 109, 139, 252, 209, 146, 174, 40, 140, 129, 41, 58, 89, 102, 193, 99, 194, 178, 192, 239, 152 };
            var r = args.context.Response;
            r.Headers.Add("content-type", "application/octet-stream");
            args.GZipSend(data);
            return true;
        }
    }

    public class  Picture : Service
    {
        public Picture()
            : base("Picture")
        {

        }
        public override bool CanbeDelayed(RequestArgs args)
            => true;
        public override bool Get(RequestArgs args)
        {
            string file = "";
            if (args.Path.Length == 2) file = args.Path[1];
            else return args.SendFail();
            if (file == "logo")
            {
                args.Server.BlockClient(args);
                return false;
            }
            var fio = new System.IO.FileInfo("./images/" + file);
            var data = fio.Exists ? System.IO.File.ReadAllBytes("./images/" + file) : new byte[0];
            var r = args.context.Response;

            r.Headers.Add("content-type", "application/octet-stream");

            r.AddHeader("Cache-Control", "public");
            r.Headers.Add("max-age", TimeSpan.FromDays(365).Ticks.ToString());

            r.Headers.Add("date", DateTime.Now.ToString());
            r.Headers.Add(HttpResponseHeader.Expires, new DateTime(DateTime.Now.Ticks + TimeSpan.FromDays(365).Ticks).ToString());
            r.Headers.Add("content-type", "image/png");
            args.GZipSend(data);
            return true;
        }
        //private static Source tinify(string image)
        //{
        //    var key = "1Rxg6noU2jkJvx-LVSYBjTyCGmTFMbUa";
        //    Tinify.Key = key;
        //    var source = Tinify.FromFile("unoptimized.jpg");
        //     source.ToFile("optimized.jpg").ContinueWith((r)=> { }).Wait();
        //    return source.Result;
        //}
        public override void Put(RequestArgs args)
        {

        }

        public override void Head(RequestArgs args)
        {

        }

        public override bool Post(RequestArgs args)
        {
            return false;
        }

        public override bool Delete(RequestArgs args)
        {
            return args.SendFail();
        }
    }
}