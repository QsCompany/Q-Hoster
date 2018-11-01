//using Server.Zip;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Serialization;
using models;
using Path = System.IO.Path;
using QServer.Properties;
using System.Runtime.Serialization;

namespace models
{
    //enum AgentPermissions
    //{

    //}
}

namespace Server
{
    struct iref
    {
    }

    [Serializable]
     public class  ResourceLoader
    {
        public static List<Type> ExtraTypes = new List<Type> { typeof(AdminResource), typeof(Resource), typeof(SpecialResource), typeof(ResourceLoader) };

        public List<Resource> resources = new List<Resource>();
        public List<string> EndPoints = new List<string> { "http://*:80/", "http://*:8080" };
        private  DirectoryInfo _path;
        private FileInfo f;
        public FileInfo Manifest
        {
            get
            {
                return f ?? (f = new FileInfo(Path.Combine(_path.FullName, "Manifest")));
            }
        }
        public ResourceLoader()
        {
            var f = Manifest;
        }
        private int y;
        public static object k;
        public ResourceLoader(string path)
        {
            _path = new DirectoryInfo(path);
        }
        public ResourceLoader Load()
        {
            if (!new FileInfo(Manifest.FullName).Exists)
            {
                Reset();
                return this;
            }
            XmlSerializer s = new XmlSerializer(typeof(List<Resource>), ExtraTypes.ToArray());
            if (Manifest.Exists)
                try
                {
                    using (var f = File.Open(Manifest.FullName, FileMode.OpenOrCreate))
                        resources = s.Deserialize(f) as List<Resource>;
#if DEV
                    Console.WriteLine($"Resource has {resources.Count} rawURLs");
#endif
                }
                catch (Exception e)
                {
                    MyConsole.WriteLine(e.Message);
                    resources = new List<Resource>();
                    Console.WriteLine($"Error when loading Resource Manifest");
                }
            else resources = new List<Resource>();
            ReValidate();
            return this;
        }
        public void Save()
        {
            resources.Add(new AdminResource(Manifest, "/Manifest", "text/xml") { Etag = "" });
            using (var t = File.Open(Manifest.FullName, FileMode.OpenOrCreate))
            {
                t.Flush(true);
                XmlSerializer s = new XmlSerializer(typeof(List<Resource>), ExtraTypes.ToArray());
                s.Serialize(t, resources);
            }
        }
        public void ReValidate()
        {
            foreach (var rs in resources.ToArray())
            {
                if (rs.FileInfo.Exists || rs.GZipFileInfo.Exists) continue;
                resources.Remove(rs);
            }
            if (Manifest.Exists) Manifest.Delete();
            using (var t = File.Open(Manifest.FullName, FileMode.OpenOrCreate))
            {
                t.Flush(true);
                XmlSerializer s = new XmlSerializer(typeof(List<Resource>), ExtraTypes.ToArray());
                s.Serialize(t, resources);
            }
        }
        public void SaveTo(string dir)
        {
            using (var t = File.Open(System.IO.Path.Combine(dir, nameof(Manifest)), FileMode.OpenOrCreate))
            {
                t.Flush(true);
                XmlSerializer s = new XmlSerializer(typeof(List<Resource>), ExtraTypes.ToArray());
                s.Serialize(t, resources);
            }
        }
        public void Reset()
        {
            resources.Clear();
            ProcessFolder(_path);
            Save();
        }
        private void ProcessFolder(DirectoryInfo d)
        {
            Resource l;
            foreach (var file in d.GetFiles())
                if ((l = Resource.New(file)) != null)
                    resources.Add(l);

            foreach (var folder in d.GetDirectories())
            {
                if (folder.FullName==Resource.ZipResourcePath.FullName) continue;
                ProcessFolder(folder);
            }
            
        }
        
    }
    [Serializable]
     public class  AdminResource:Resource
    {
        public AdminResource()
        {

        }
        protected override bool CheckAccess(RequestArgs a)
        {
            return a.User.IsAgent;
        }
        public AdminResource(FileInfo info, string rawUrl, string contentType) : base(info, rawUrl, contentType) { }
    }
    [Serializable]
     public class  SpecialResource : Resource
    {
        public SpecialResource()
        {

        }
        private AgentPermissions _perm;
        public SpecialResource(AgentPermissions perm,FileInfo info, string rawUrl, string contentType) : base(info, rawUrl, contentType) 
        {
            _perm = perm;
        }
        protected override bool CheckAccess(RequestArgs a)
        {
            return (a.User.Permission & _perm) == _perm;
        }
    }

    public class EntryPoint : Resource
    {
        public EntryPoint(Resource resource) : base(resource.FileInfo, "/", resource.ContentType)
        {
        }
        public override void Reponse(HttpListenerContext c)
        {
            c.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            base.Reponse(c);
        }
        public override void Reponse(RequestArgs args)
        {
            args.context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            base.Reponse(args);
        }
    }
    
    [Serializable]
    public class ReqHeader /*: ISerializable*/
    {
        //public ReqHeader(SerializationInfo info, StreamingContext context)
        //{
        //    Key = (string)info.GetValue(nameof(Key), typeof(String));
        //    Value = (string)info.GetValue(nameof(Value), typeof(String));
        //}

        public ReqHeader()
        {

        }
        public ReqHeader(string key, string value)
        {
            Key = key;
            Value = value;
        }
        [XmlAttribute]
        public string Key { get; set; }
        [XmlAttribute]
        public string Value { get; set; }

        //public void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue(nameof(Key), Key, typeof(String));
        //    info.AddValue(nameof(Value), Value, typeof(String));
        //}
    }
    [Serializable]
    public class  Resource
    {
        public Resource() { }
        public static string GetContentType(string ext)
        {
            return contentTypes.TryGetValue(ext, out ext) ? ext : "application/octet-stream";
        }
        private static Dictionary<string, string> contentTypes = new Dictionary<string, string>();
        private static int i;

        public static string EntryPoint
        {
            get => Settings.Default.EntryPoint;
            set
            {
                Settings.Default.EntryPoint = (value ?? "").Replace('\\', '/');
                Settings.Default.Save();
            }
        }

        public static string MySQLPath
        {
            get { return Settings.Default.MySQLPath; }
            set
            {
                Settings.Default.MySQLPath = value;
                Settings.Default.Save();
            }
        }
        public static string ResourcePath
        {
            get => Settings.Default.Path;
            set
            {
                _zipPath = null;
                Settings.Default.Path = value;
                Settings.Default.Save();
            }
        }
        static Resource()
        {
            Settings.Default.SettingsLoaded += (e, a) => SharedPath = SharedPath;
            InitContentTypes();
        }

        private static DirectoryInfo sharedPathInfo;
        public static string SharedPath
        {
            get => Settings.Default.SharedPath;
            set
            {
                _zipPath = null;
                Settings.Default.SharedPath = value;
                Settings.Default.Save();
                sharedPathInfo = new DirectoryInfo(value);
                if (SharedPathInfo.Exists) return;
                var fi = new FileInfo(value);
                if (fi.Exists) SharedPath = fi.Directory.FullName;
            }
        }
        private static DirectoryInfo _zipPath;
        public static DirectoryInfo ZipResourcePath => _zipPath ?? (_zipPath = new DirectoryInfo(Path.Combine(ResourcePath, "__bin__")));

        public static string DatabasePath
        {
            get => Settings.Default.DPath?.ToLowerInvariant();
            set
            {
                Settings.Default.DPath = value?.ToLowerInvariant();
                Settings.Default.Save();
            }
        }

        public static string[] Addresses
        {
            get  {
                return (Settings.Default.Addresses ?? "").Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            set
            {
                Settings.Default.Addresses = value == null ? null : string.Join(";", value);
                Settings.Default.Save();
            }
        }

        public static string ServerIP
        {
            get => Settings.Default.ServerIP;
            set
            {
                Settings.Default.ServerIP = value;
                Settings.Default.Save();
            }
        }

        public static string UserID
        {
            get => Settings.Default.UserID;
            set
            {
                Settings.Default.UserID = value;
                Settings.Default.Save();
            }
        }

        public static string Password
        {
            get => Settings.Default.Password;
            set
            {
                Settings.Default.Password = value;
                Settings.Default.Save();
            }
        }
        public static uint Port
        {
            get => Settings.Default.Port;
            set
            {
                Settings.Default.Port = value;
                Settings.Default.Save();
            }
        }
        
        public static void SetAddresses(string s)
        {
            Settings.Default.Addresses = s;
            Settings.Default.Save();
        }
        public static IPAddress[] ParseAddresses(string s)
        {
            db:
            if (s == "*" || string.IsNullOrWhiteSpace(s))
            {
                return Dns.GetHostAddresses(Environment.GetEnvironmentVariable("COMPUTERNAME"));
            }
            else
            {
                var x = s.Split(',', ';');
                var b = new List<IPAddress>();
                foreach (var ip in x)
                {
                    if (IPAddress.TryParse(ip, out var ipa)) b.Add(ipa);
                }
                if (b.Count != 0) return b.ToArray();
                s = "*";
                goto db;
            }
        }


        private static void InitContentTypes()
        {
            contentTypes.Add(".323", "text/h323");
            contentTypes.Add(".3g2", "video/3gpp2");
            contentTypes.Add(".3gp2", "video/3gpp2");
            contentTypes.Add(".3gp", "video/3gpp");
            contentTypes.Add(".3gpp", "video/3gpp");
            contentTypes.Add(".aac", "audio/aac");
            contentTypes.Add(".aaf", "application/octet-stream");
            contentTypes.Add(".apk", "application/octet-stream");
            contentTypes.Add(".aca", "application/octet-stream");
            contentTypes.Add(".accdb", "application/msaccess");
            contentTypes.Add(".accde", "application/msaccess");
            contentTypes.Add(".accdt", "application/msaccess");
            contentTypes.Add(".acx", "application/internet-property-stream");
            contentTypes.Add(".adt", "audio/vnd.dlna.adts");
            contentTypes.Add(".adts", "audio/vnd.dlna.adts");
            contentTypes.Add(".afm", "application/octet-stream");
            contentTypes.Add(".ai", "application/postscript");
            contentTypes.Add(".aif", "audio/x-aiff");
            contentTypes.Add(".aifc", "audio/aiff");
            contentTypes.Add(".aiff", "audio/aiff");
            contentTypes.Add(".application", "application/x-ms-application");
            contentTypes.Add(".art", "image/x-jg");
            contentTypes.Add(".asd", "application/octet-stream");
            contentTypes.Add(".asf", "video/x-ms-asf");
            contentTypes.Add(".asi", "application/octet-stream");
            contentTypes.Add(".asm", "text/plain");
            contentTypes.Add(".asr", "video/x-ms-asf");
            contentTypes.Add(".asx", "video/x-ms-asf");
            contentTypes.Add(".atom", "application/atom+xml");
            contentTypes.Add(".au", "audio/basic");
            contentTypes.Add(".avi", "video/x-msvideo");
            contentTypes.Add(".axs", "application/olescript");
            contentTypes.Add(".bas", "text/plain");
            contentTypes.Add(".bcpio", "application/x-bcpio");
            contentTypes.Add(".bin", "application/octet-stream");
            contentTypes.Add(".bmp", "image/bmp");
            contentTypes.Add(".c", "text/plain");
            contentTypes.Add(".cab", "application/vnd.ms-cab-compressed");
            contentTypes.Add(".calx", "application/vnd.ms-office.calx");
            contentTypes.Add(".cat", "application/vnd.ms-pki.seccat");
            contentTypes.Add(".cdf", "application/x-cdf");
            contentTypes.Add(".chm", "application/octet-stream");
            contentTypes.Add(".class", "application/x-java-applet");
            contentTypes.Add(".clp", "application/x-msclip");
            contentTypes.Add(".cmx", "image/x-cmx");
            contentTypes.Add(".cnf", "text/plain");
            contentTypes.Add(".cod", "image/cis-cod");
            contentTypes.Add(".cpio", "application/x-cpio");
            contentTypes.Add(".cpp", "text/plain");
            contentTypes.Add(".crd", "application/x-mscardfile");
            contentTypes.Add(".crl", "application/pkix-crl");
            contentTypes.Add(".crt", "application/x-x509-ca-cert");
            contentTypes.Add(".csh", "application/x-csh");
            contentTypes.Add(".css", "text/css");
            contentTypes.Add(".csv", "text/csv");
            contentTypes.Add(".cur", "application/octet-stream");
            contentTypes.Add(".dcr", "application/x-director");
            contentTypes.Add(".deploy", "application/octet-stream");
            contentTypes.Add(".der", "application/x-x509-ca-cert");
            contentTypes.Add(".dib", "image/bmp");
            contentTypes.Add(".dir", "application/x-director");
            contentTypes.Add(".disco", "text/xml");
            contentTypes.Add(".dll", "application/x-msdownload");
            contentTypes.Add(".dll.config", "text/xml");
            contentTypes.Add(".dlm", "text/dlm");
            contentTypes.Add(".doc", "application/msword");
            contentTypes.Add(".docm", "application/vnd.ms-word.document.macroEnabled.12");
            contentTypes.Add(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            contentTypes.Add(".dot", "application/msword");
            contentTypes.Add(".dotm", "application/vnd.ms-word.template.macroEnabled.12");
            contentTypes.Add(".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template");
            contentTypes.Add(".dsp", "application/octet-stream");
            contentTypes.Add(".dtd", "text/xml");
            contentTypes.Add(".dvi", "application/x-dvi");
            contentTypes.Add(".dvr-ms", "video/x-ms-dvr");
            contentTypes.Add(".dwf", "drawing/x-dwf");
            contentTypes.Add(".dwp", "application/octet-stream");
            contentTypes.Add(".dxr", "application/x-director");
            contentTypes.Add(".eml", "message/rfc822");
            contentTypes.Add(".emz", "application/octet-stream");
            contentTypes.Add(".eot", "application/vnd.ms-fontobject");
            contentTypes.Add(".eps", "application/postscript");
            contentTypes.Add(".etx", "text/x-setext");
            contentTypes.Add(".evy", "application/envoy");
            contentTypes.Add(".exe", "application/octet-stream");
            contentTypes.Add(".exe.config", "text/xml");
            contentTypes.Add(".fdf", "application/vnd.fdf");
            contentTypes.Add(".fif", "application/fractals");
            contentTypes.Add(".fla", "application/octet-stream");
            contentTypes.Add(".flr", "x-world/x-vrml");
            contentTypes.Add(".flv", "video/x-flv");
            contentTypes.Add(".gif", "image/gif");
            contentTypes.Add(".gtar", "application/x-gtar");
            contentTypes.Add(".gz", "application/x-gzip");
            contentTypes.Add(".h", "text/plain");
            contentTypes.Add(".hdf", "application/x-hdf");
            contentTypes.Add(".hdml", "text/x-hdml");
            contentTypes.Add(".hhc", "application/x-oleobject");
            contentTypes.Add(".hhk", "application/octet-stream");
            contentTypes.Add(".hhp", "application/octet-stream");
            contentTypes.Add(".hlp", "application/winhlp");
            contentTypes.Add(".hqx", "application/mac-binhex40");
            contentTypes.Add(".hta", "application/hta");
            contentTypes.Add(".htc", "text/x-component");
            contentTypes.Add(".htm", "text/html");
            contentTypes.Add(".html", "text/html");
            contentTypes.Add(".htt", "text/webviewhtml");
            contentTypes.Add(".hxt", "text/html");
            contentTypes.Add(".ical", "text/calendar");
            contentTypes.Add(".icalendar", "text/calendar");
            contentTypes.Add(".ico", "image/x-icon");
            contentTypes.Add(".ics", "text/calendar");
            contentTypes.Add(".ief", "image/ief");
            contentTypes.Add(".ifb", "text/calendar");
            contentTypes.Add(".iii", "application/x-iphone");
            contentTypes.Add(".inf", "application/octet-stream");
            contentTypes.Add(".ins", "application/x-internet-signup");
            contentTypes.Add(".isp", "application/x-internet-signup");
            contentTypes.Add(".IVF", "video/x-ivf");
            contentTypes.Add(".jar", "application/java-archive");
            contentTypes.Add(".java", "application/octet-stream");
            contentTypes.Add(".jck", "application/liquidmotion");
            contentTypes.Add(".jcz", "application/liquidmotion");
            contentTypes.Add(".jfif", "image/pjpeg");
            contentTypes.Add(".jpb", "application/octet-stream");
            contentTypes.Add(".jpe", "image/jpeg");
            contentTypes.Add(".jpeg", "image/jpeg");
            contentTypes.Add(".jpg", "image/jpeg");
            contentTypes.Add(".js", "application/javascript");
            contentTypes.Add(".jsx", "text/jscript");
            contentTypes.Add(".latex", "application/x-latex");
            contentTypes.Add(".lit", "application/x-ms-reader");
            contentTypes.Add(".lpk", "application/octet-stream");
            contentTypes.Add(".lsf", "video/x-la-asf");
            contentTypes.Add(".lsx", "video/x-la-asf");
            contentTypes.Add(".lzh", "application/octet-stream");
            contentTypes.Add(".m13", "application/x-msmediaview");
            contentTypes.Add(".m14", "application/x-msmediaview");
            contentTypes.Add(".m1v", "video/mpeg");
            contentTypes.Add(".m2ts", "video/vnd.dlna.mpeg-tts");
            contentTypes.Add(".m3u", "audio/x-mpegurl");
            contentTypes.Add(".m4a", "audio/mp4");
            contentTypes.Add(".m4v", "video/mp4");
            contentTypes.Add(".man", "application/x-troff-man");
            contentTypes.Add(".manifest", "application/x-ms-manifest");
            contentTypes.Add(".map", "text/plain");
            contentTypes.Add(".mdb", "application/x-msaccess");
            contentTypes.Add(".mdp", "application/octet-stream");
            contentTypes.Add(".me", "application/x-troff-me");
            contentTypes.Add(".mht", "message/rfc822");
            contentTypes.Add(".mhtml", "message/rfc822");
            contentTypes.Add(".mid", "audio/mid");
            contentTypes.Add(".midi", "audio/mid");
            contentTypes.Add(".mix", "application/octet-stream");
            contentTypes.Add(".mmf", "application/x-smaf");
            contentTypes.Add(".mno", "text/xml");
            contentTypes.Add(".mny", "application/x-msmoney");
            contentTypes.Add(".mov", "video/quicktime");
            contentTypes.Add(".movie", "video/x-sgi-movie");
            contentTypes.Add(".mp2", "video/mpeg");
            contentTypes.Add(".mp3", "audio/mpeg");
            contentTypes.Add(".mp4", "video/mp4");
            contentTypes.Add(".mp4v", "video/mp4");
            contentTypes.Add(".mpa", "video/mpeg");
            contentTypes.Add(".mpe", "video/mpeg");
            contentTypes.Add(".mpeg", "video/mpeg");
            contentTypes.Add(".mpg", "video/mpeg");
            contentTypes.Add(".mpp", "application/vnd.ms-project");
            contentTypes.Add(".mpv2", "video/mpeg");
            contentTypes.Add(".ms", "application/x-troff-ms");
            contentTypes.Add(".msi", "application/octet-stream");
            contentTypes.Add(".mso", "application/octet-stream");
            contentTypes.Add(".mvb", "application/x-msmediaview");
            contentTypes.Add(".mvc", "application/x-miva-compiled");
            contentTypes.Add(".nc", "application/x-netcdf");
            contentTypes.Add(".nsc", "video/x-ms-asf");
            contentTypes.Add(".nws", "message/rfc822");
            contentTypes.Add(".ocx", "application/octet-stream");
            contentTypes.Add(".oda", "application/oda");
            contentTypes.Add(".odc", "text/x-ms-odc");
            contentTypes.Add(".ods", "application/oleobject");
            contentTypes.Add(".oga", "audio/ogg");
            contentTypes.Add(".ogg", "video/ogg");
            contentTypes.Add(".ogv", "video/ogg");
            contentTypes.Add(".ogx", "application/ogg");
            contentTypes.Add(".one", "application/onenote");
            contentTypes.Add(".onea", "application/onenote");
            contentTypes.Add(".onetoc", "application/onenote");
            contentTypes.Add(".onetoc2", "application/onenote");
            contentTypes.Add(".onetmp", "application/onenote");
            contentTypes.Add(".onepkg", "application/onenote");
            contentTypes.Add(".osdx", "application/opensearchdescription+xml");
            contentTypes.Add(".otf", "font/otf");
            contentTypes.Add(".p10", "application/pkcs10");
            contentTypes.Add(".p12", "application/x-pkcs12");
            contentTypes.Add(".p7b", "application/x-pkcs7-certificates");
            contentTypes.Add(".p7c", "application/pkcs7-mime");
            contentTypes.Add(".p7m", "application/pkcs7-mime");
            contentTypes.Add(".p7r", "application/x-pkcs7-certreqresp");
            contentTypes.Add(".p7s", "application/pkcs7-signature");
            contentTypes.Add(".pbm", "image/x-portable-bitmap");
            contentTypes.Add(".pcx", "application/octet-stream");
            contentTypes.Add(".pcz", "application/octet-stream");
            contentTypes.Add(".pdf", "application/pdf");
            contentTypes.Add(".pfb", "application/octet-stream");
            contentTypes.Add(".pfm", "application/octet-stream");
            contentTypes.Add(".pfx", "application/x-pkcs12");
            contentTypes.Add(".pgm", "image/x-portable-graymap");
            contentTypes.Add(".pko", "application/vnd.ms-pki.pko");
            contentTypes.Add(".pma", "application/x-perfmon");
            contentTypes.Add(".pmc", "application/x-perfmon");
            contentTypes.Add(".pml", "application/x-perfmon");
            contentTypes.Add(".pmr", "application/x-perfmon");
            contentTypes.Add(".pmw", "application/x-perfmon");
            contentTypes.Add(".png", "image/png");
            contentTypes.Add(".pnm", "image/x-portable-anymap");
            contentTypes.Add(".pnz", "image/png");
            contentTypes.Add(".pot", "application/vnd.ms-powerpoint");
            contentTypes.Add(".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12");
            contentTypes.Add(".potx", "application/vnd.openxmlformats-officedocument.presentationml.template");
            contentTypes.Add(".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12");
            contentTypes.Add(".ppm", "image/x-portable-pixmap");
            contentTypes.Add(".pps", "application/vnd.ms-powerpoint");
            contentTypes.Add(".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12");
            contentTypes.Add(".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow");
            contentTypes.Add(".ppt", "application/vnd.ms-powerpoint");
            contentTypes.Add(".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12");
            contentTypes.Add(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
            contentTypes.Add(".prf", "application/pics-rules");
            contentTypes.Add(".prm", "application/octet-stream");
            contentTypes.Add(".prx", "application/octet-stream");
            contentTypes.Add(".ps", "application/postscript");
            contentTypes.Add(".psd", "application/octet-stream");
            contentTypes.Add(".psm", "application/octet-stream");
            contentTypes.Add(".psp", "application/octet-stream");
            contentTypes.Add(".pub", "application/x-mspublisher");
            contentTypes.Add(".qt", "video/quicktime");
            contentTypes.Add(".qtl", "application/x-quicktimeplayer");
            contentTypes.Add(".qxd", "application/octet-stream");
            contentTypes.Add(".ra", "audio/x-pn-realaudio");
            contentTypes.Add(".ram", "audio/x-pn-realaudio");
            contentTypes.Add(".rar", "application/octet-stream");
            contentTypes.Add(".ras", "image/x-cmu-raster");
            contentTypes.Add(".rf", "image/vnd.rn-realflash");
            contentTypes.Add(".rgb", "image/x-rgb");
            contentTypes.Add(".rm", "application/vnd.rn-realmedia");
            contentTypes.Add(".rmi", "audio/mid");
            contentTypes.Add(".roff", "application/x-troff");
            contentTypes.Add(".rpm", "audio/x-pn-realaudio-plugin");
            contentTypes.Add(".rtf", "application/rtf");
            contentTypes.Add(".rtx", "text/richtext");
            contentTypes.Add(".scd", "application/x-msschedule");
            contentTypes.Add(".sct", "text/scriptlet");
            contentTypes.Add(".sea", "application/octet-stream");
            contentTypes.Add(".setpay", "application/set-payment-initiation");
            contentTypes.Add(".setreg", "application/set-registration-initiation");
            contentTypes.Add(".sgml", "text/sgml");
            contentTypes.Add(".sh", "application/x-sh");
            contentTypes.Add(".shar", "application/x-shar");
            contentTypes.Add(".sit", "application/x-stuffit");
            contentTypes.Add(".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12");
            contentTypes.Add(".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide");
            contentTypes.Add(".smd", "audio/x-smd");
            contentTypes.Add(".smi", "application/octet-stream");
            contentTypes.Add(".smx", "audio/x-smd");
            contentTypes.Add(".smz", "audio/x-smd");
            contentTypes.Add(".snd", "audio/basic");
            contentTypes.Add(".snp", "application/octet-stream");
            contentTypes.Add(".spc", "application/x-pkcs7-certificates");
            contentTypes.Add(".spl", "application/futuresplash");
            contentTypes.Add(".spx", "audio/ogg");
            contentTypes.Add(".src", "application/x-wais-source");
            contentTypes.Add(".ssm", "application/streamingmedia");
            contentTypes.Add(".sst", "application/vnd.ms-pki.certstore");
            contentTypes.Add(".stl", "application/vnd.ms-pki.stl");
            contentTypes.Add(".sv4cpio", "application/x-sv4cpio");
            contentTypes.Add(".sv4crc", "application/x-sv4crc");
            contentTypes.Add(".svg", "image/svg+xml");
            contentTypes.Add(".svgz", "image/svg+xml");
            contentTypes.Add(".swf", "application/x-shockwave-flash");
            contentTypes.Add(".t", "application/x-troff");
            contentTypes.Add(".tar", "application/x-tar");
            contentTypes.Add(".tcl", "application/x-tcl");
            contentTypes.Add(".tex", "application/x-tex");
            contentTypes.Add(".texi", "application/x-texinfo");
            contentTypes.Add(".texinfo", "application/x-texinfo");
            contentTypes.Add(".tgz", "application/x-compressed");
            contentTypes.Add(".thmx", "application/vnd.ms-officetheme");
            contentTypes.Add(".thn", "application/octet-stream");
            contentTypes.Add(".tif", "image/tiff");
            contentTypes.Add(".tiff", "image/tiff");
            contentTypes.Add(".toc", "application/octet-stream");
            contentTypes.Add(".crx", "application/x-chrome-extension");
            contentTypes.Add(".tr", "application/x-troff");
            contentTypes.Add(".trm", "application/x-msterminal");
            contentTypes.Add(".ts", "video/vnd.dlna.mpeg-tts");
            contentTypes.Add(".tsv", "text/tab-separated-values");
            contentTypes.Add(".ttf", "application/octet-stream");
            contentTypes.Add(".tts", "video/vnd.dlna.mpeg-tts");
            contentTypes.Add(".txt", "text/plain");
            contentTypes.Add(".u32", "application/octet-stream");
            contentTypes.Add(".uls", "text/iuls");
            contentTypes.Add(".ustar", "application/x-ustar");
            contentTypes.Add(".vbs", "text/vbscript");
            contentTypes.Add(".vcf", "text/x-vcard");
            contentTypes.Add(".vcs", "text/plain");
            contentTypes.Add(".vdx", "application/vnd.ms-visio.viewer");
            contentTypes.Add(".vml", "text/xml");
            contentTypes.Add(".vsd", "application/vnd.visio");
            contentTypes.Add(".vss", "application/vnd.visio");
            contentTypes.Add(".vst", "application/vnd.visio");
            contentTypes.Add(".vsto", "application/x-ms-vsto");
            contentTypes.Add(".vsw", "application/vnd.visio");
            contentTypes.Add(".vsx", "application/vnd.visio");
            contentTypes.Add(".vtx", "application/vnd.visio");
            contentTypes.Add(".wav", "audio/wav");
            contentTypes.Add(".wax", "audio/x-ms-wax");
            contentTypes.Add(".wbmp", "image/vnd.wap.wbmp");
            contentTypes.Add(".wcm", "application/vnd.ms-works");
            contentTypes.Add(".wdb", "application/vnd.ms-works");
            contentTypes.Add(".webm", "video/webm");
            contentTypes.Add(".wks", "application/vnd.ms-works");
            contentTypes.Add(".wm", "video/x-ms-wm");
            contentTypes.Add(".wma", "audio/x-ms-wma");
            contentTypes.Add(".wmd", "application/x-ms-wmd");
            contentTypes.Add(".wmf", "application/x-msmetafile");
            contentTypes.Add(".wml", "text/vnd.wap.wml");
            contentTypes.Add(".wmlc", "application/vnd.wap.wmlc");
            contentTypes.Add(".wmls", "text/vnd.wap.wmlscript");
            contentTypes.Add(".wmlsc", "application/vnd.wap.wmlscriptc");
            contentTypes.Add(".wmp", "video/x-ms-wmp");
            contentTypes.Add(".wmv", "video/x-ms-wmv");
            contentTypes.Add(".wmx", "video/x-ms-wmx");
            contentTypes.Add(".wmz", "application/x-ms-wmz");
            contentTypes.Add(".woff", "font/x-woff");
            contentTypes.Add(".wps", "application/vnd.ms-works");
            contentTypes.Add(".wri", "application/x-mswrite");
            contentTypes.Add(".wrl", "x-world/x-vrml");
            contentTypes.Add(".wrz", "x-world/x-vrml");
            contentTypes.Add(".wsdl", "text/xml");
            contentTypes.Add(".wtv", "video/x-ms-wtv");
            contentTypes.Add(".wvx", "video/x-ms-wvx");
            contentTypes.Add(".x", "application/directx");
            contentTypes.Add(".xaf", "x-world/x-vrml");
            contentTypes.Add(".xaml", "application/xaml+xml");
            contentTypes.Add(".xap", "application/x-silverlight-app");
            contentTypes.Add(".xbap", "application/x-ms-xbap");
            contentTypes.Add(".xbm", "image/x-xbitmap");
            contentTypes.Add(".xdr", "text/plain");
            contentTypes.Add(".xht", "application/xhtml+xml");
            contentTypes.Add(".xhtml", "application/xhtml+xml");
            contentTypes.Add(".xla", "application/vnd.ms-excel");
            contentTypes.Add(".xlam", "application/vnd.ms-excel.addin.macroEnabled.12");
            contentTypes.Add(".xlc", "application/vnd.ms-excel");
            contentTypes.Add(".xlm", "application/vnd.ms-excel");
            contentTypes.Add(".xls", "application/vnd.ms-excel");
            contentTypes.Add(".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12");
            contentTypes.Add(".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12");
            contentTypes.Add(".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            contentTypes.Add(".xlt", "application/vnd.ms-excel");
            contentTypes.Add(".xltm", "application/vnd.ms-excel.template.macroEnabled.12");
            contentTypes.Add(".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template");
            contentTypes.Add(".xlw", "application/vnd.ms-excel");
            contentTypes.Add(".xml", "text/xml");
            contentTypes.Add(".json", "text/json");
            contentTypes.Add(".xof", "x-world/x-vrml");
            contentTypes.Add(".xpm", "image/x-xpixmap");
            contentTypes.Add(".xps", "application/vnd.ms-xpsdocument");
            contentTypes.Add(".xsd", "text/xml");
            contentTypes.Add(".xsf", "text/xml");
            contentTypes.Add(".xsl", "text/xml");
            contentTypes.Add(".xslt", "text/xml");
            contentTypes.Add(".xsn", "application/octet-stream");
            contentTypes.Add(".xtp", "application/octet-stream");
            contentTypes.Add(".xwd", "image/x-xwindowdump");
            contentTypes.Add(".z", "application/x-compress");
            contentTypes.Add(".zip", "application/x-zip-compressed");
            contentTypes.Add(".woff2", "font/x-woff");
        }

        protected virtual bool CheckAccess(RequestArgs a){
            return true;
        }
        
        

        public byte[] GetBuffer() => File.ReadAllBytes(FileInfo.FullName);
        public byte[] SaveGZip()
        {
            
            try
            {
                var gzip = GZipFileInfo;
                if (!FileInfo.Exists) return null;
                using (var x = File.OpenRead(FileInfo.FullName))
                    _buffer = RequestArgs.EncodeGZip(x);
                if (!gzip.Directory.Exists)
                    gzip.Directory.Create();
                
                File.WriteAllBytes(gzip.FullName, _buffer);
                return _buffer;
            }
            catch (Exception)
            {
                if (_buffer != null) return _buffer;
            }
            return null;
        }

        public byte[] ReadGZip()
        {
            try
            {
                var gzip = GZipFileInfo;
                if (!DEBUGGER.DisableCache)
                {
                    if (_buffer != null) return _buffer;
                    if (gzip.Exists)
                        return _buffer = File.ReadAllBytes(gzip.FullName);
                }
                if (!FileInfo.Exists) return _buffer = new byte[0];
                using (var x = File.OpenRead(FileInfo.FullName))
                    _buffer = RequestArgs.EncodeGZip(x);
                if (!gzip.Directory.Exists)
                    gzip.Directory.Create();
                File.WriteAllBytes(gzip.FullName, _buffer);
                return _buffer;
            }
            catch (Exception)
            {
            }
            return _buffer;
        }
        public byte[] ReadZipBuffer()
        {
            if (GZipFileInfo.Exists) return File.ReadAllBytes(GZipFileInfo.FullName);
            if (!FileInfo.Exists) return null;
            using (var x = File.OpenRead(FileInfo.FullName))
                return RequestArgs.EncodeGZip(x);
        }
        public virtual byte[] getGzipBuffer() => _buffer = ReadGZip();
        public string filePath { get; set; }
        public string RawUrl { get; set; }
        public FileInfo FileInfo => new FileInfo(Path.Combine(ResourcePath, filePath));
        
        public string RelativeFile
        {
            get
            {
                return GetRelativePath(FileInfo.FullName);
            }
        }
        public string RelativeGZipFile
        {
            get
            {
                return GetRelativePath(GZipFileInfo.FullName);
            }
        }
        string GetRelativePath(string filespec)
        {
            string folder = ResourcePath;
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
        public FileInfo GZipFileInfo => new FileInfo(Path.Combine(ZipResourcePath.FullName, RelativeFile));

        public string ContentType { get; set; }
        public string Etag { get; set; }
        public ReqHeader[] Headers { get; set; } = { new ReqHeader("Service-Worker-Allowed", "/") };
        [NonSerialized]
        private byte[] _buffer;
        public Resource(FileInfo info, string rawUrl, string contentType)
        {
            filePath = info.FullName;
            RawUrl = rawUrl;
            ContentType = contentType;
            Etag = "35e674a2bf4bd" + i++ + ":0";
        }
        public virtual void Reponse(RequestArgs args)
        {
            var c = args.context;
            if (!DEBUGGER.DisableCache)
            {
                var lm = c.Request.Headers.Get("If-Modified-Since");
                if (lm != null)
                    if (DateTime.TryParse(lm, out var date) && date < FileInfo.LastWriteTime)
                    {
                        c.Response.StatusCode = (int)HttpStatusCode.NotModified;
                        return;
                    }
            }
            var bytes = getGzipBuffer();
            if (DEBUGGER.DisableCache)
            {
                 args.GZipSend(bytes);
                return;
            }
            if (bytes != null)
            {
                var m = c.Request.HttpMethod;
                var tt = GZipFileInfo.LastAccessTimeUtc.ToString();
                var t = c.Request.Headers.ToString();
                var r = c.Response;
                r.Headers.Add("content-type", ContentType);
                if (Headers != null)
                    try
                    {
                        for (int i = 0; i < Headers.Length; i++)
                            r.Headers.Add(Headers[i].Key, Headers[i].Value);
                    }
                    catch { }
      
                r.AddHeader("Cache-Control", "public");
                r.Headers.Add("max-age", TimeSpan.FromDays(365).Ticks.ToString());

                r.Headers.Add("Last-Modified", GZipFileInfo.LastWriteTimeUtc.ToString());

                r.Headers.Add("date", DateTime.Now.ToString());
                r.Headers.Add("etag", Etag);
                r.Headers.Add(HttpResponseHeader.Expires, new DateTime(DateTime.Now.Ticks + TimeSpan.FromDays(365).Ticks).ToString());
                r.Headers.Add(HttpResponseHeader.Vary, "Accept-Encoding");

                if (CheckAccess(args))
                {
                    r.AddHeader("content-length", bytes.Length.ToString());
                    var buffer = args.GZipSend(bytes, true);
                }
                else
                {
                    args.GZipSend(new byte[0]);
                }
            }
            else
            {
                args.SendAlert("Virus", "L'Url de fichier est supprimmer" + args.Url);
            }
        }
        public virtual bool RequireAuth => false;

        public static DateTime LastTimeBackup
        {
            get
            {
                return Settings.Default.LastTimeBackup;
            }
            set
            {
                Settings.Default.LastTimeBackup = value;
                Settings.Default.Save();
            }
        }
        public static string BackupDir
        {
            get
            {
                return Settings.Default.BuckupDir;
            }
            set
            {
                Settings.Default.BuckupDir = value;
                Settings.Default.Save();
            }
        }

        public static DirectoryInfo SharedPathInfo
        {
            get
            {
                if (sharedPathInfo == null)
                {
                    deb:
                    sharedPathInfo = new DirectoryInfo(SharedPath);                    
                    if (!sharedPathInfo.Exists)
                    {
                        var fi = new FileInfo(SharedPath);
                        if (fi.Exists) { SharedPath = fi.Directory.FullName; goto deb; }
                        if (!sharedPathInfo.Exists) sharedPathInfo.Create();
                    }
                }
                return sharedPathInfo;
            }
        }

        public virtual void Reponse(HttpListenerContext c)
        {
            var lm = c.Request.Headers.Get("If-Modified-Since");
            if (lm != null)
                if (DateTime.TryParse(lm, out var date) && date < FileInfo.LastWriteTime)
                {
                    c.Response.StatusCode = (int)HttpStatusCode.NotModified;
                    return;
                }
            var bytes = getGzipBuffer();
            if (bytes != null)
            {
                var r = c.Response;
                if (DEBUGGER.DisableCache)
                {
                    r.Headers.Add("content-type", ContentType);
                    RequestArgs.GZipSend(c, bytes, true);
                    return;
                }
                var m = c.Request.HttpMethod;
                var tt = GZipFileInfo.LastAccessTimeUtc.ToString();
                var t = c.Request.Headers.ToString();
                
                r.Headers.Add("content-type", ContentType);
                if (Headers != null)
                    try
                    {
                        for (int i = 0; i < Headers.Length; i++)
                            r.Headers.Add(Headers[i].Key, Headers[i].Value);
                    }
                    catch { }

#if NOCACHE
                r.AddHeader("content-length", bytes.Length.ToString());
                c.Response.OutputStream.Write(bytes, 0, bytes.Length);
                //args.Send(GetBuffer());
                
                return;
#endif

                r.AddHeader("Cache-Control", "public");
                r.Headers.Add("max-age", TimeSpan.FromDays(365).Ticks.ToString());

                r.Headers.Add("Last-Modified", GZipFileInfo.LastWriteTimeUtc.ToString());

                r.Headers.Add("date", DateTime.Now.ToString());
                r.Headers.Add("etag", Etag);
                r.Headers.Add(HttpResponseHeader.Expires, new DateTime(DateTime.Now.Ticks + TimeSpan.FromDays(365).Ticks).ToString());
                r.Headers.Add(HttpResponseHeader.Vary, "Accept-Encoding");


                r.AddHeader("content-length", bytes.Length.ToString());
                var buffer = RequestArgs.GZipSend(c, bytes, true);

            }
            else
            {
                c.Response.StatusCode = 404;
                c.Response.Close();
                //RequestArgs.GZipSend(c, new byte[0], false);
                
            }

        }

        public static Resource New(FileInfo file)
        {
            var rawUrl = file.FullName.Replace(ResourcePath, "\\").Replace('\\', '/').Replace("//","/").ToLowerInvariant();
            var contentType = (string)null;
            if (!contentTypes.TryGetValue(file.Extension.ToLowerInvariant(), out contentType)) return null;
            var x = new Resource(file, rawUrl, contentType);
            x.filePath = x.RelativeFile;
            return x;
        }
        public static bool UseRelativePath = true;
    }
}
