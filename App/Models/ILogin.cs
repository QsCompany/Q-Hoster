using System.Collections.Generic;
using Json;
using Server;

namespace models
{
    public interface ILogin
    {
        Client Client { get; set; }
        string EncPwd { get; set; }
        string Identification { get; set; }
        bool IsLogged { get; set; }
        bool IsThrusted { get; set; }
        bool IsValidated { get; set; }
        AgentPermissions Permission { get; set; }
        string Pwd { get; set; }
        string Username { get; set; }

        bool Check();
        bool Check(out List<Client.Message> k);
        bool Check(RequestArgs args);
        bool Check(RequestArgs args, ref List<Client.Message> k);
        User InitUser();
        JValue Parse(JValue json);
        bool RegeneratePwd(string keyString);
        int Repaire(Database db);
    }
}