using System.Collections.Generic;

namespace models
{
    public interface IClient
    {
        Abonment Abonment { get; set; }
        string FirstName { get; set; }
        string FullName { get; }
        Job Job { get; set; }
        string LastName { get; set; }
        Picture Picture { get; set; }
        float SoldTotal { get; }
        string WorkAt { get; set; }

        void AddMessage(SMS sms);
        bool Check(out List<Client.Message> messages);
        bool Check(out string error);
        void CloneFrom(Client c);
        void DeleteUnReadedSMS(SMS sms);
        int GetSMSCount(Database db);
        SMSs GetSMSs(Database db);
        List<Facture> GetValidableFactures(Database d);
        void MakeSMSReaded(SMS sms);
        int Repaire(Database db);
    }
}