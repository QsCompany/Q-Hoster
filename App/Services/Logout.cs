using models;
using Server;

namespace Api
{
    [QServer.Core.HosteableObject(typeof(Logout))]
    public class  Logout:Service
    {
        public Logout()
            : base("Logout")
        {
        }

        public override bool Get(RequestArgs args)
        {
            var client = args.BodyAsJson as models.Client;
            if (args.Server.Logout(client, args))
                args.Send(new models.Client {Id = 0L});
            else args.Send(new models.Message(MessageType.Alert) {Content = "The Server Cannot logout you"});
            return true;
        }
    }
}