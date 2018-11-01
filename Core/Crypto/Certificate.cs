using System.Linq;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using System.Collections.Generic;
using Path = System.IO.Path;

namespace Server
{
    public class Certificate
    {

        string bindCertToIPPort(System.Net.IPAddress iP, int port, string certhash, Guid appID) => $"netsh http add sslcert ipport={iP.ToString()}:{port} certhash={certhash} appid={{{appID}" + "}";
        string getSSLBinder(string thumprint) => bindCertToIPPort(IPAddress.Any, 443, thumprint, Guid.Parse("49616d4d-7573-6c69-6d4d-757a61626974"));
        public string createCertificateAuthority()
        {
            //
            var ca = "makecert -n \"CN=vMargeCA\" -r -sv vMargeCA.pvk vMargeCA.cer";
            var ssl = "makecert -sk vMargeSignedByCA -iv vMargeCA.pvk -n \"CN=vMargeSignedByCA\" -ic vMargeCA.cer vMargeSignedByCA.cer -sr localmachine -ss My";
            return ssl;
        }
        //public string createCertificateAuthority1(string fileName,DateTime debDate,DateTime expDate,string issuer,params string[] domains)
        //{
            
        //    var certificateAuthority = $"makecert.exe -n \"CN={issuer}\" -r -pe -a sha512 -len 4096 -cy authority -sv {issuer}.pvk {issuer}.cer -ss My -sr LocalMachine";

        //    string getDomains()
        //    {
        //        for (int i = 0; i < domains.Length; i++)
        //            domains[i] = $"\"CN={domains[i]}\"";
        //        return string.Join(",", domains);
        //    }

        //    var certificateServer = $"makecert.exe -n {getDomains()} -iv {issuer}.pvk -ic {issuer}.cer -pe -a sha512 -len 4096 -b {debDate.ToShortDateString()} -e {expDate.ToShortDateString()} -sky exchange -eku 1.3.6.1.5.5.7.3.1 -sv {getPath("","pvk")} {getPath("","cer")} -ss My -sr LocalMachine";


        //    //var x = new[] { certificateAuthority, certificateServer ,bindCertToIPPort(IPAddress.Any,443};
        //    return certificateAuthority + "\r\n" + certificateServer;
        //}
        public static string AppID => (Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute),false).ToArray()[0] as GuidAttribute).Value;
        private string ThumPrintOf(string file)
        {
            X509Certificate2 x509 = new X509Certificate2();
            x509.Import(File.ReadAllBytes(file));
            return x509.Thumbprint;
        }
    }


    public class CertInfo
    {
        private List<string> memo = new List<string>();
        public string SubjectName { set => memo.Add("CN=" + value); }
        public override string ToString() => "\"" + string.Join(", ", memo) + "\"";
    }
    public class CertificateBuilder
    {
        public string OutputPath { get; set; }
        public CertInfo Info { get; } = new CertInfo();
        public string FileName { get; set; }
        public DateTime DebDate { get; set; }
        public DateTime ExpDate { get; set; }
        public string Issuer { get; set; }

        public string ClientCertName { get; set; }



        public string CAutherity =>
            $"makecert.exe -n \"CN={Issuer}\" -r -pe -a sha512 -len 4096 -cy authority -sv {getPath("CA", "pvk")} {getPath("CA", "cer")} -ss My -sr LocalMachine";

        public string CServer => $"makecert.exe -n {Info} -iv {getPath("CA", "pvk")} -ic {getPath("CA", "cer")} -pe -a sha512 -len 4096 -b {DebDate.ToString("mm/dd/yyyy")} -e {ExpDate.ToString("mm/dd/yyyy")} -sky exchange -eku 1.3.6.1.5.5.7.3.1 -sv {getPath("", "pvk")} {getPath("", "cer")} -ss My -sr LocalMachine";
        public string CCient => $"makecert.exe -n \"CN={ClientCertName}\" -iv {getPath("CA", "pvk")} -ic {getPath("CA", "cer")} -pe -a sha512 -len 4096 -b {DebDate.ToString("mm/dd/yyyy")} -e {ExpDate.ToString("mm/dd/yyyy")} -sky exchange -eku 1.3.6.1.5.5.7.3.2 -sv {getPath("CC", "pvk")} {getPath("CC", "cer")}";

        private string getPath(string prefix, string ext) => Path.Combine(OutputPath, prefix + FileName + "." + ext);
    }


}