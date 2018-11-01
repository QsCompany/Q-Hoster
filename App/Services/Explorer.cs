using Json;
using Server;
using System;
using System.IO;
using models1;
using models;
using System.Collections.Generic;

namespace models1
{
    public class Folder : DataRow
    {
        public new static int __LOAD__(int dp) => DataRow.__LOAD__(DPName);
        public static int DPName = Register<Folder, string>("Name");
        public string Name { get => get<string>(DPName); set => set(DPName, value); }
        public Folder(long id)
        {
            Id = id;
        }
        public Folder()
        {

        }
    }
    public class File : DataRow
    {
        public new static int __LOAD__(int dp) => DataRow.__LOAD__(DPName);
        public static int DPName = Register<File, string>("Name");
        public string Name { get => get<string>(DPName); set => set(DPName, value); }

        public File()
        {

        }
        public File(long id)
        {
            Id = id;
        }
    }
    public class Folders : DataTable<Folder>
    {
        public new static int __LOAD__(int dp) => dp + DataRow.__LOAD__(0);
        protected Folders(Context c, JValue jv) : base(c, jv)
        {
        }
        public Folders() : base(null)
        {

        }
        
        protected override void GetOwner(DataBaseStructure d, models.Path c)
        {
            throw new NotImplementedException();
        }
    }
    public class Files : DataTable<File>
    {
        public new static int __LOAD__(int dp) => dp + DataRow.__LOAD__(0);
        protected Files(Context c, JValue jv) : base(c, jv)
        {
        }
        public Files() : base(null)
        {
        }

        protected override void GetOwner(DataBaseStructure d, models.Path c)
        {
        }
    }

}
namespace Api
{
    enum Permission
    {
        None = 0,
        View = 1,
        Read = 3,
        Write = 7,
        Delete = 15,
        Permissions = 31
    }
    class Permissions
    {
        public DirectoryInfo dir;
        private FileInfo Info;
        private Context serializer;
        
        public JObject Value;

        private Permissions(string dir)
        {
            Reset(dir);
        }
        public void Reset(string dirPath)
        {
            Value?.Clear();
            dir = new DirectoryInfo(dirPath);
            serializer = new Context(true, null, null);
            Info = new FileInfo(System.IO.Path.Combine(dirPath, "_"));

            if (dir.Exists && Info.Exists)
            {
                var s = System.IO.File.ReadAllText(Info.FullName, System.Text.Encoding.Default);
                Value = serializer.Read(s, false) as JObject;
            }
            if (Value == null)
                Value = new JObject() { ["."] = new JNumber(0) };
        }
        public void Save()
        {
            System.IO.File.WriteAllText(Info.FullName, serializer.Stringify(Value).ToString(), System.Text.Encoding.Default);
            Reset(dir.FullName);
        }
        public Permission this[string file,string clientID ,bool save=true]
        {
            get => ((Value[clientID] is JObject f) && ((f[file] ?? f["."]) is JNumber n)) ? (Permission)(int)n.Value : Permission.None;
            set
            {
                if (!(Value[clientID] is JObject f)) Value[clientID] = f = new JObject();
                f[file] = new JNumber((decimal)value);
                if (save)
                    System.IO.File.WriteAllText(Info.FullName, serializer.Stringify(Value).ToString(), System.Text.Encoding.Default);
            }
        }
        public JObject this[string clientID] { get { if (Value[clientID] is JObject f) return f; f = new JObject(); Value[clientID] = f; return f; } }

        private static Dictionary<string, Permissions> _rcdl = new Dictionary<string, Permissions>();
        public static Permissions Create(string dir)
        {
            dir = dir.ToLowerInvariant();
            if (_rcdl.TryGetValue(dir, out var p)) return p;
            p = new Permissions(dir);
            _rcdl[dir] = p;
            return p;
        }

        public JObject GetPermissionsOfFile(string file)
        {
            JObject j = new JObject();
            file = file.ToLowerInvariant();
            foreach (var kv in Value)
            {
                if (!(kv.Value is JObject scr)) continue;
                if (scr[file] is JNumber per)
                    j[kv.Key /*client ID*/] = per;
            }
            return j;
        }        

        public bool DeletePermission(string file,string clientID )
        {
            if (this[clientID] is JObject cfls)
            {
                cfls.Remove(file);
                Save();
            }
            return true;
        }
    }
    public class Slice
    {
        public long start;
        public long size;
        public string id;
        public bool IsValide = true;
        public Slice(RequestArgs args)
        {
            if (!args.GetParam("slice_start", out start)) IsValide = false;
            if (!args.GetParam("slice_size", out size)) IsValide = false;
            if (!args.GetParam("slice_id", out id)) IsValide = false;
        }
    }
    class FileSegment
    {
        
        public string id;
        public string dir;
        public string file;
        public long size;
        public Slice slice;
        public bool IsValide = true;
        public FileSegment(RequestArgs args)
        {
            if (!args.GetParam("id", out id)) IsValide = false;
            if (!args.GetParam("dir", out dir)) IsValide = false;
            if (!args.GetParam("file", out file)) IsValide = false;
            if (!args.GetParam("size", out long size)) IsValide = false;
            slice = new Slice(args);
            IsValide = IsValide && slice.IsValide;
        }
    }
    [QServer.Core.HosteableObject(typeof(Explorer), null)]
    public partial class Explorer: Service
    {
        public override bool CanbeDelayed(RequestArgs args) => true;
        public override bool Options(RequestArgs args)
        {
            return base.Options(args);
        }
        public override bool CheckAccess(RequestArgs args) => args.User.IsAdmin;
        public Explorer() : base("explore")
        {
        }
        public override bool Post(RequestArgs args)
        {
            var t = args.BodyAsBytes;
            var fs = new FileSegment(args);
            if (!fs.IsValide) return args.SendFail();
            var f = GetDirInfo(fs.dir);
            var pr = Permissions.Create(f.FullName);
            var s = pr [fs.file.ToLowerInvariant(), args.Client.Id.ToString()];
            if (!args.User.IsAdmin && (s & Permission.Write) != Permission.Write) return args.SendInfo($"You cannot write to {fs.dir}/{fs.file}", false);

            if (t.Length != fs.slice.size)
                return args.SendFail();
            
            var fl = new FileInfo(System.IO.Path.Combine(f.FullName, fs.file));

            using (var fst = fl.Exists ? fl.OpenWrite() : fl.Create())
            {
                if (args.GetParam("seek", out long seek))
                    fst.Seek(seek, SeekOrigin.Begin);
                else
                    fst.Seek(fs.slice.start, SeekOrigin.Begin);
                fst.Write(t, 0, (int)fs.slice.size);
                fst.Close();
            }
            if (s == Permission.None)
                pr[fs.file.ToLowerInvariant(), args.Client.Id.ToString()] = Permission.Delete;
            return args.SendSuccess();
        }

        private DirectoryInfo GetDirInfo(string dir)
        {
            if (dir == null || dir.Trim() == "") dir = ".\\";
            else if (!dir.EndsWith("/")) dir += "\\";
            return new DirectoryInfo(System.IO.Path.Combine(Resource.SharedPath, dir));
        }

        public static FileInfo GetFileInfo(string file)
        {
            if (file == null || file.Trim() == "") return null;
            return new FileInfo(System.IO.Path.Combine(Resource.SharedPath, file));
        }
        public override bool Get(RequestArgs args)
        {
            if (args.HasParam("dwn"))
                return Download(args);
            if (args.User.IsAdmin) return ListAllFiles(args);
            var dir = new DirectoryInfo(System.IO.Path.Combine(Resource.SharedPath, args.GetParam("dir") ?? ""));
            var prms = Permissions.Create(dir.FullName.ToLowerInvariant());
            string clientID = args.Client.Id.ToString();
            var dirP =(int) prms[".", clientID];
            var files = prms[clientID];

            var dx = new CSV<Folder>();
            var fx = new CSV<models1.File>();
            var i = 0;
            
            var foldresCSV = dx.Stringify(args.JContext, dir.GetDirectories(), (d) => {
                if ((int)Permissions.Create(d.FullName)[".", clientID] >= (int)Permission.Read)
                    return new Folder(++i) { Name = d.Name };
                else return null;
            }).ToString();
            i = 0;
            var filesCSV = fx.Stringify(args.JContext, files, (d) =>
            {
                if (d.Key == ".") return null;
                if (d.Value is JNumber n)
                {
                    if ((int)n.Value < (int)Permission.Read)
                        return null;
                }
                else if (dirP < (int)Permission.Read) return null;
                
                return new models1.File() { Name = new FileInfo(dir.FullName + "\\" + d.Key).Name };

            }).ToString();
            args.JContext.GetBuilder().Clear();
            args.Send(new JObject() { ["Folders"] = (JString)foldresCSV, ["Files"] = (JString)filesCSV });
            return true;
        }

        private bool ListAllFiles(RequestArgs args)
        {
            var dir = new DirectoryInfo(System.IO.Path.Combine(Resource.SharedPath, args.GetParam("dir") ?? ""));
            if (dir.Exists)
            {
                var dx = new CSV<Folder>();
                var fx = new CSV<models1.File>();
                var i = 0;
                var foldresCSV = dx.Stringify(args.JContext, dir.GetDirectories(), (d) => new Folder(++i) { Name = d.Name }).ToString();
                i = 0;
                var filesCSV = fx.Stringify(args.JContext, dir.GetFiles(), (d) => new models1.File(++i) { Name = d.Name }).ToString();

                args.JContext.GetBuilder().Clear();
                args.Send(new JObject() { ["Folders"] = (JString)foldresCSV, ["Files"] = (JString)filesCSV });
                return true;
            }
            else
            {
                args.Send(new JObject() { ["IsDeleted"] = (JBool)true });
                return false;
            }
        }
        
        private bool Download(RequestArgs args)
        {
            var date = args.Client.GetCookie("download_file", false, out var expire);
            if(date !=null && !expire)
            {
                args.context.Response.StatusCode = (int)System.Net.HttpStatusCode.ResetContent;
                args.context.Response.Close();
                return false;
            }
            args.Client.SetCookie("download_file", this, DateTime.Now + TimeSpan.FromSeconds(15));

            if(args.GetParam("file",out string fl))
            {
                var file = GetFileInfo(fl);
                var pr = Permissions.Create(file.Directory.FullName);
                var s = pr[file.Name.ToLowerInvariant(), args.Client.Id.ToString()];
                if (!args.User.IsAdmin && (int)s < (int)Permission.Read) return args.SendInfo($"You cannot read the file {fl}", false);
                if (file?.Exists == true)
                {
                    sendLargeFile(args, file);
                    //args.GZipSend(v);

                    return true;
                }
                else
                {
                    args.context.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                    return false;
                }
            }
            return false;
        }

        private void sendLargeFile(RequestArgs args, FileInfo file)
        {

            var res = args.context.Response;
            var fSTR = System.IO.File.OpenRead(file.FullName);
            var nSTR = res.OutputStream;            
            var cur = 0;
            var size = 1000 * 10;
            var bLength = 0;
            var dispose = false;
            byte[] buffer = new byte[size];
            byte[] getNextBytes()
            {
                if (fSTR.Position >= fSTR.Length)
                {
                    args.IsBusy = false;
                    args.context.Response.Close();
                    if (dispose)
                        args.Dispose();
                    return null;
                }
                cur += bLength;
                fSTR.Seek(cur, SeekOrigin.Begin);
                bLength = fSTR.Read(buffer, 0, buffer.Length);
                return buffer;
            }
            void write()
            {
                if (getNextBytes() == null) return;
                try
                {

                    nSTR.BeginWrite(buffer, 0, bLength, (a) =>
                    {
                        try
                        {
                            nSTR.EndWrite(a);
                        }
                        catch (Exception e)
                        {
                        }
                        finally
                        {
                            write();
                        }
                    }, this);
                }
                catch { }
            }

            var Response = args.context.Response;
            Response.ContentType = Resource.GetContentType(file.Extension);// "application/octet-stream";
            Response.AddHeader("Content-Disposition", $"attachment;filename=\"{file.Name}\"");

            args.IsBusy = true;
            write();
            dispose = true;
        }
        
    }
    
    //[QServer.Core.Service]
    //public class DFile:Service
    //{
    //    public DFile() : base("resource")
    //    {
    //    /*
    //          app.get('/video', function(req, res) {
    //          const path = 'assets/sample.mp4'
    //          const stat = fs.statSync(path)
    //          const fileSize = stat.size
    //          const range = req.headers.range
    //          if (range) {
    //            const parts = range.replace(/bytes=/, "").split("-")
    //            const start = parseInt(parts[0], 10)
    //            const end = parts[1] 
    //              ? parseInt(parts[1], 10)
    //              : fileSize-1
    //            const chunksize = (end-start)+1
    //            const file = fs.createReadStream(path, {start, end})
    //            const head = {
    //              'Content-Range': `bytes ${start}-${end}/${fileSize}`,
    //              'Accept-Ranges': 'bytes',
    //              'Content-Length': chunksize,
    //              'Content-Type': 'video/mp4',
    //            }
    //            res.writeHead(206, head);
    //            file.pipe(res);
    //          } else {
    //            const head = {
    //              'Content-Length': fileSize,
    //              'Content-Type': 'video/mp4',
    //            }
    //            res.writeHead(200, head)
    //            fs.createReadStream(path).pipe(res)
    //          }
    //        });
    //    */
    //    }
    //}
    public partial class Explorer
    {
        protected override bool Open(RequestArgs args)
        {
            args.GetParam("dir", out string dir);
            dir = string.IsNullOrWhiteSpace(dir) ? "." : dir;

            args.GetParam("file", out string file);
            file = file ?? ".";
            var diri = GetDirInfo(dir);
            var per = Permissions.Create(diri.FullName);
            if (args.User.IsAdmin || per[file, args.Client.Id.ToString()] >= Permission.Permissions)
                switch (args.GetParam("method"))
                {
                    case "list":
                         args.Send(per.GetPermissionsOfFile(file));
                        return true;
                    case "delete":
                        if (!args.GetParam("cid", out string cid) || string.IsNullOrWhiteSpace(cid)) break;
                        return args.SendStatus(per.DeletePermission(file, cid));
                    case "set":
                        if (args.GetParam("cid", out  cid) && !string.IsNullOrWhiteSpace(cid) && args.GetParam("per", out int permission) && permission != -1 && enumPermissions.Contains(permission))
                        {
                            per[file, cid, true] =(Permission) permission;
                            return args.SendSuccess();
                        }
                        break;
                }
            return args.SendFail();
        }
        private static List<int> enumPermissions = new List<int>() { 0, 1, 3, 5, 7, 15, 31 };
    }
}
