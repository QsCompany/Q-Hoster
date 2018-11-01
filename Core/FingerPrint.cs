using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace QServer
{
    public class FingerPrint
    {
        private static string fingerPrint = string.Empty;
        public static Guid Value()
        {
            if (string.IsNullOrEmpty(fingerPrint))
            {
#if !FINAL
                try
                {
                    fingerPrint = GetHash("CPU >> " + cpuId() + "\nBIOS >> " + biosId() + "\nBASE >> " + baseId());
                }
                catch { }
#else
                fingerPrint = GetHash("CPU >> " + cpuId() + "\nBIOS >> " + biosId() + "\nBASE >> " + baseId());
#endif
                //+ "\nVIDEO >> " + videoId() + "\nMAC >> " + macId()

            }
            System.Guid.TryParse(fingerPrint, out var guid);
            return guid;
        }
        private static string GetHash(string s)
        {
            MD5 sec = new MD5CryptoServiceProvider();
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] bt = enc.GetBytes(s);
            return GetHexString(sec.ComputeHash(bt));
        }
        private static string GetHexString(byte[] bt)
        {
            var x = new[] { false, true, true, true, true, false, false, false };
            string s = string.Empty;
            var j = 0;
            for (int i = 0; i < bt.Length; i++)
            {
                byte b = bt[i];
                int n, n1, n2;
                n = (int)b;
                n1 = n & 15;
                n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char)(n2 - 10 + (int)'A')).ToString();
                else
                    s += n2.ToString();
                if (n1 > 9)
                    s += ((char)(n1 - 10 + (int)'A')).ToString();
                else
                    s += n1.ToString();
                if ((i + 1) != bt.Length && (i + 1) % 2 == 0)
                {
                    if (x[j])
                        s += "-";
                    j++;
                }
            }
            return s;
        }
#region Original Device ID Getting Code
        //Return a hardware identifier
        private static string identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
        {
            string result = "";
            System.Management.ManagementClass mc = new System.Management.ManagementClass(wmiClass);
            System.Management.ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (mo[wmiMustBeTrue].ToString() == "True")
                {
                    //Only get the first one
                    if (result == "")
                    {
                        try
                        {
                            result = mo[wmiProperty].ToString();
                            break;
                        }
                        catch(Exception e)
                        {
                            MyConsole.WriteLine(e.Message);
                        }
                    }
                }
            }
            return result;
        }
        //Return a hardware identifier
        private static string identifier(string wmiClass, params string[] wmiProperty)
        {
            System.Management.ManagementClass mc = new System.Management.ManagementClass(wmiClass);
            System.Management.ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                try
                {
                    foreach (var prop in wmiProperty)
                    {
                        var t = mo[prop] as string;
                        if (t == null) continue;
                        return t ?? "";
                    }
                }
                catch(Exception e)
                {
                    MyConsole.WriteLine(e.Message);
                }
            }
            return "";
        }

        private static string identifierS(string wmiClass, params string[] wmiProperty)
        {
            ManagementClass mc = new System.Management.ManagementClass(wmiClass);
            ManagementObjectCollection moc = mc.GetInstances();
            var rslt = "";
            foreach (ManagementObject mo in moc)
                try
                {
                    foreach (var prop in wmiProperty)
                    {
                        var t = mo[prop] as string;
                        if (t == null) continue;
                        else rslt += t;
                    }
                }
                catch(Exception e)
                {
                    MyConsole.WriteLine(e.Message);
                }
            
            return rslt;
        }

        private static string cpuId()
        {
            return identifier("Win32_Processor", "UniqueId", "ProcessorId", "Name", "Manufacturer") + identifier("Win32_Processor", "MaxClockSpeed");
        }
        //BIOS Identifier
        private static string biosId()
        {
            return identifierS("Win32_BIOS", "Manufacturer", "SMBIOSBIOSVersion", "IdentificationCode", "SerialNumber", "ReleaseDate", "Version");
        }
        //Main physical hard drive ID
        private static string diskId()
        {
            return identifierS("Win32_DiskDrive", "Model", "Manufacturer", "Signature", "TotalHeads");
        }
        //Motherboard ID
        private static string baseId()
        {
            return identifier("Win32_BaseBoard", "Model", "Manufacturer", "Name", "SerialNumber");
        }
        //Primary video controller ID
        private static string videoId()
        {
            return identifier("Win32_VideoController", "DriverVersion", "Name");
        }
        //First enabled network card ID
        private static string macId()
        {
            return identifier("Win32_NetworkAdapterConfiguration", "MACAddress", "IPEnabled");
        }
#endregion
    }
}
