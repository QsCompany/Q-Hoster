using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QServer.Core
{
    
    //class Firewall
    //{
    //    private const string CLSID_FIREWALL_MANAGER = "{304CE942-6E39-40D8-943A-B913C40C9CD4}";
    //    private static NetFwTypeLib.INetFwMgr GetFirewallManager()
    //    {
    //        Type objectType = Type.GetTypeFromCLSID(
    //              new Guid(CLSID_FIREWALL_MANAGER));
    //        return Activator.CreateInstance(objectType) as NetFwTypeLib.INetFwMgr;
    //    }
    //    void IsFireWallActivate()
    //    {
    //        INetFwMgr manager = GetFirewallManager();
    //        bool isFirewallEnabled = manager.LocalPolicy.CurrentProfile.FirewallEnabled;
    //        if (isFirewallEnabled == false)
    //            manager.LocalPolicy.CurrentProfile.FirewallEnabled = true;
    //    }
    //    private const string PROGID_AUTHORIZED_APPLICATION = "HNetCfg.FwAuthorizedApplication";
    //    public bool AuthorizeApplication(string title, string applicationPath,
    //        NET_FW_SCOPE_ scope, NET_FW_IP_VERSION_ ipVersion)
    //    {
    //        // Create the type from prog id
    //        Type type = Type.GetTypeFromProgID(PROGID_AUTHORIZED_APPLICATION);
    //        INetFwAuthorizedApplication auth = Activator.CreateInstance(type)
    //            as INetFwAuthorizedApplication;
    //        auth.Name = title;
    //        auth.ProcessImageFileName = applicationPath;
    //        auth.Scope = scope;
    //        auth.IpVersion = ipVersion;
    //        auth.Enabled = true;



    //        INetFwMgr manager = GetFirewallManager();
    //        try
    //        {
    //            manager.LocalPolicy.CurrentProfile.AuthorizedApplications.Add(auth);
    //        }
    //        catch (Exception ex)
    //        {
    //            return false;
    //        }
    //        return true;
    //    }


    //    public Firewall()
    //    {
    //        try
    //        {

    //            IsFireWallActivate();
    //            AuthorizeApplication("QServer",Application.ExecutablePath,
    //                NET_FW_SCOPE_.NET_FW_SCOPE_ALL,
    //                NET_FW_IP_VERSION_.NET_FW_IP_VERSION_ANY);
    //        }
    //        catch (Exception e)
    //        {


    //        }
    //    }

    //}
}
