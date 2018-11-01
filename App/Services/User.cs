using System;
using Server;

namespace Api
{
    public class  User : Service
    {
        public User()
            : base("User")
        {

        }

        public override bool Get(RequestArgs args)
        {
            var t = args.Database.Clients;
            args.Send(t[args.Id]);
            return true;
        }

        public override void Put(RequestArgs args)
        {
            
        }

        public override void Head(RequestArgs args)
        {
            throw new NotImplementedException();
        }

        public override bool Post(RequestArgs args)
        {
            throw new NotImplementedException();
        }

        public override bool Delete(RequestArgs args)
        {
            throw new NotImplementedException();
        }
    }
}