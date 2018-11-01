using Server;

namespace Api
{
    
    public class  Pictures :TableService
    {
        public Pictures ():base("Pictures")
        {

        }
        public override bool Get(RequestArgs args)
        {
            return false;
        }

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
            return false;
        }
    }
}