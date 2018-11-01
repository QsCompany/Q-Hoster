using Server;

namespace Api
{
    public class  Signup:Service
    {
        public Signup()
            : base("Signup")
        {
            
        }
        public override bool Get(RequestArgs args)
        {
            short from = 0;
            short to = 0;
            short.TryParse(args.GetParam("f") ?? "0", out from);
            if (!short.TryParse(args.GetParam("t") ?? short.MaxValue.ToString(), out to)) to = short.MaxValue;

            var b = new byte[(to - from + 1) * 2];
            unchecked
            {
                for (int i = 0; from < to; from++, i += 2)
                {
                    b[i] = (byte)(from & 0xff);
                    b[i + 1] = (byte)(from >> 8);
                }
            }
            args.Send(b);
            return true;
        }
        public override void Put(RequestArgs args)
        {
            var signup = args.BodyAsJson as models.Login;
            if (signup == null) args.CodeError(404);
            if (signup.Client == null) args.CodeError(404);
        }
        public override bool Post(RequestArgs args)
        {
            var signup = args.BodyAsJson as models.Login;
            if (signup == null) return args.CodeError(404);
            if (signup.Client == null) return args.CodeError(404);
            return false;
        }

    }
}