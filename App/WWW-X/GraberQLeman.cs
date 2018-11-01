//using System;
//using HtmlAgilityPack;
//using System.IO;
//using System.Net;
//using System.Linq;
//using System.Collections.Generic;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.Runtime.Serialization;

//namespace QServers
//{

//    public static class Help
//    {
//        public static HtmlWeb web = new HtmlWeb();
//        public static HtmlDocument LoadHtml(this string url)
//        {
//            url = (url.StartsWith("http://") || url.StartsWith("https") ? url : ("http://shop.qleman.ch" + url));
//            return web.Load(url);
//        }
//        public static string Replace(this string s, char @old, char @new) => new System.Text.RegularExpressions.Regex(old.ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase).Replace(s, @new.ToString());
//        public static List<HtmlNode> FindByClassName(this HtmlNode node, string name)
//        {
//            var r = new List<HtmlNode>();
//            List<HtmlNode> nodes = new List<HtmlNode>() { node };
//            for (int i = 0; i < nodes.Count; i++)
//            {
//                node = nodes[i];
//                if (node.HasClass(name)) r.Add(node);
//                nodes.AddRange(node.ChildNodes);
//            }
//            return r;
//        }
//        public static HtmlNode FirstHasClassName(this HtmlNode node, string name)
//        {
//            List<HtmlNode> nodes = new List<HtmlNode>() { node };
//            for (int i = 0; i < nodes.Count; i++)
//            {
//                node = nodes[i];
//                if (node.HasClass(name)) return node;
//                nodes.AddRange(node.ChildNodes);
//            }
//            return null;
//        }
//        public static HtmlNode FirstElementChild(this HtmlNode node)
//        {
//            var c = node?.FirstChild;
//            while (c != null && (c.NodeType != HtmlNodeType.Element))
//                c = c.NextSibling;
//            return c;
//        }

//        public static HtmlNode NextElementSibling(this HtmlNode node)
//        {
//            do
//                node = node.NextSibling;
//            while (node != null && (node.NodeType != HtmlNodeType.Element));
//            return node;
//        }
//        public static string CleanFileName(this string fileName)
//        {
//            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), "!_"));
//        }
//    }

//    class GraberQLeman
//    {
//        HtmlWeb web = new HtmlWeb();
//        static void Main(string[] args)
//        {
//            var cats = new Catalogues(new DirectoryInfo("./Catalogues"));
//            //cats.RegisterProduits();
//            cats.ExtractInfo();
//            return;

//            var catsr = new Catalogues("http://shop.qleman.ch/?srv=katalog");

            

//            return;
//            var g = new GraberQLeman();
//            for (var i = 'A'; i <= 'Z'; i++)
//                g.GrabbeIndex(i);
//            for (var i = 'A'; i <= 'Z'; i++)
//                g.LoadIndexed(i);
//        }

//        private void LoadIndexed(char a)
//        {
//            var dir = new DirectoryInfo($@".\QLeman\Index{a}\");
//            var file = new FileInfo($@".\QLeman\Index{a}\Index{a}.html");
//            if (!file.Exists) GrabbeIndex(a);
//            HtmlDocument f = new HtmlDocument();
//            f.Load(file.FullName);
//            ParseIndex(f,dir);
//        }

//        private void ParseIndex(HtmlDocument f,DirectoryInfo directory)
//        {
//            foreach(var ul in f.DocumentNode.Descendants("ul"))
//            {
//                foreach (var li in ul.ChildNodes)
//                {
//                    if (li.NodeType == HtmlNodeType.Element)
//                    {
//                        var a = GetA(li);
//                        if (a != null)
//                        {
//                            var catName = CleanFileName(a.InnerText);
//                            var dir = new DirectoryInfo(Path.Combine(directory.FullName, catName));
//                            var file = Path.Combine(dir.FullName, catName + ".html");
//                            if (new FileInfo(file).Exists) continue;
//                            var href = a.Attributes["href"]?.Value;

                            
//                            if (!dir.Exists) dir.Create();
//                            var doc = web.Load("http://shop.qleman.ch" + href);
//                            var result = doc.GetElementbyId("id_finder_result");
//                            if (result == null)
//                            {

//                            }
//                            else
//                            {
//                                doc.DocumentNode.RemoveAll();
//                                doc.DocumentNode.AppendChild(result);
//                            }
                            
//                            doc.Save(file);
//                        }
//                    }
//                }
//            }
//        }
//        private static string CleanFileName(string fileName)
//        {
//            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), "!_"));
//        }
//        private static HtmlNode GetA(HtmlNode node)
//        {
//            var c = node.FirstChild;
//            while (c != null && (c.NodeType != HtmlNodeType.Element || c.Name != "a"))
//                c = c.NextSibling;
//            return c;
//        }
//        public void GrabbeIndex(char a)
//        {
//            var file = new System.IO.FileInfo($@".\QLeman\Index{a}\Index{a}.html");
//            if (file.Exists) return;
//            if (!file.Directory.Exists) file.Directory.Create();
//            var url = $"http://shop.qleman.ch/pages2015/rpc/rpc_index_lst.cfm?indexCHR={char.ToUpper(a)}&partnerId=23&cmsrub=0";
            
//            var html = web.Load(url);
            
            

//            html.Save(file.FullName);
//        }

//    }
//    [Serializable]
//    public class Catalogue
//    {
//        public string Name { get; }
//        public string Href { get; }
//        public string ImageUrl { get; }
//        public Categories Categories { get; } = new Categories();
//        public Catalogue(HtmlNode node)
//        {
//            Name = node.FirstHasClassName("cBox_txt")?.InnerText;
//            Href = node.Attributes["href"]?.Value;
//            var p = node.FirstHasClassName("cBox_img")?.Attributes["style"]?.Value;
//            if (p != null)
//            {
//                var i0 = p.IndexOf('\'');
//                var i1 = p.IndexOf('\'', i0 + 1);
//                ImageUrl = p.Substring(i0 + 1, i1 - i0 - 1);
//            }
//        }

//        protected Catalogue()
//        {
//            Name = "Genearle";
//        }

//        internal void Load()
//        {
//            var doc = (Href.StartsWith("http://") || Href.StartsWith("https") ? Href : ("http://shop.qleman.ch"+ Href)).LoadHtml();
//            Categories.Parse(doc.DocumentNode);
            

//        }

//        internal void DownloadImages()
//        {
//            foreach (var t in this.Categories)
//                t.DownloadImages();
            
//        }

//        internal void ExtractInfo()
//        {
//            Categories.ExtractInfo();
//        }

//        internal void Save(DirectoryInfo dir)
//        {

//            BinaryFormatter bf = new BinaryFormatter(); 
            
//            var f = new FileInfo(Path.Combine(dir.FullName, Name.CleanFileName() + ".xml"));
//            using (var s = File.Open(f.FullName, FileMode.Open))
//            {
//                s.Flush(true); 
//                bf.Serialize(s, this);
//            }
//            try
//            {
//                System.Xml.Serialization.XmlSerializer z = new System.Xml.Serialization.XmlSerializer(typeof(Catalogue));
//                using (var s = File.Open(f.FullName + ".xml", FileMode.OpenOrCreate))
//                {
//                    s.Flush(true);
//                    z.Serialize(s, this);
//                }
//            }
//            catch { }
//        }
//    }
//    [Serializable]
//    class DefaultCatalogue : Catalogue
//    {
//        public DefaultCatalogue(HtmlNode node) : base(node)
//        {
//        }
//        public DefaultCatalogue():base()
//        {
            
            
//        }
//    }

//    [Serializable]
//    public class Catalogues : List<Catalogue>
//    {
//        public static Catalogues Instance { get; private set; }
//        public Catalogue Default
//        {
//            get
//            {
//                foreach (var t in this)
//                {
//                    if (t is DefaultCatalogue) return t;
//                }
//                DefaultCatalogue g;
//                Add(g = new DefaultCatalogue());
//                return g;
//            }
//        }

//        public Catalogues(string url):this()
//        {
//            var doc = url.LoadHtml();
//            var content = doc.GetElementbyId("content_catalog");
//            foreach (var box in content.FindByClassName("cBox"))
//                Add(new Catalogue(box));

//            foreach (var cat in this)
//            {
//                cat.Load();
//                BinaryFormatter f = new BinaryFormatter();
//                using (var strm = File.Open("./" + cat.Name.CleanFileName() + ".xml", FileMode.OpenOrCreate))
//                    f.Serialize(strm, cat);
//            }
//        }

//        public Catalogues(DirectoryInfo directoryInfo) : this()
//        {
//            BinaryFormatter bf = new BinaryFormatter();
//            foreach (var f in directoryInfo.GetFiles())
//            {

//                if (f.Extension == ".xml")
//                {
//                    using (var s = File.Open(f.FullName, FileMode.Open))
//                        this.Add(bf.Deserialize(s) as Catalogue);
//                }
//            }
//        }

//        internal void DownloadImages()
//        {
//            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter f = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
//            foreach (var cat in this)
//                using (var strm = File.Open("./Catalogues" + cat.Name.CleanFileName() + ".xml", FileMode.OpenOrCreate))
//                {
//                    strm.Flush(true);
//                    f.Serialize(strm, cat);
//                }
//            foreach (var t in this)
//                t.DownloadImages();

//        }

//        public void ExtractInfo()
//        {
//            var cc = Category.GetCategory("00");
//            if (cc == null) cc = Category.Default;
//            foreach (var item in this)
//            {
//                for (var i = 0; i < item.Categories.Count; i++)
//                {
//                    var c = item.Categories[i];
//                    for(var j=0;j< c.Produits.Count;j++)
//                    {
//                        var x = c.Produits[j];
//                        x.ExtractInfo();
//                        //item.Save(new DirectoryInfo("./Catalogues"));
//                    }
//                }

//                item.Save(new DirectoryInfo("./Catalogues"));
//            }
//        }

//        internal static Produit GetProduct(HtmlNode result)
//        {
//            var p = new Produit(result);
//            var p1 = Produit.GetProduit(p.Id);
//            if(p1==null)
//            {
//                news++;
//                var cat = Category.GetByProduitId(p.Id) ?? Category.Default;

//                cat.Produits.Add(p);
//                Produit.Register(p.Id, p);
//                return p;
//            }
//            return p1;
//        }
//        static int news;
//        public void RegisterProduits()
//        {
//            foreach (var item in this)
//                foreach (var c in item.Categories)
//                    foreach (var x in c.Produits)
//                        Produit.Register(x.Id, x);
//        }

//        [OnDeserialized]
//        public void OnDeserialized(StreamingContext c)
//        {
//            Instance = this;
//        }
//        public Catalogues()
//        {
//            Instance = this;
//        }
                        
//    }
//    [Serializable]
//    class DefaultCategory:Category
//    {
//        public DefaultCategory()
//        {

//        }
//    }

//    [Serializable]
//    public class Category
//    {
//        public string Index { get; set; }
//        public string Name { get; set; }
//        public string Url { get; set; }
//        public Categories Subs { get; } = new Categories();
//        public Produits Produits { get; } = new Produits();
//        public static Category Default
//        {
//            get {

//                foreach (var cc in Catalogues.Instance.Default.Categories)
//                    if (cc is DefaultCategory) return cc;

//                var t = new DefaultCategory() { Index = "00", Name = "Generale" };
//                Catalogues.Instance.Default.Categories.Add(t);
//                return t;
//            }
//        }

//        protected Category()
//        {

//        }
//        public static bool TryParse(HtmlNode node,out Category category)
//        {
//            category = null;
//            var td1 = node.FirstElementChild();
//            var td2 = td1?.NextElementSibling();
//            var td3 = td2?.NextElementSibling();
//            if (td3 == null) return false;
//            var Index = (td1.FirstElementChild() ?? td1).InnerText;
//            var Name = td3.InnerText;
//            string Url = null;
//            var onc = td2.Attributes["onclick"]?.Value ?? td1.Attributes["onclick"]?.Value ?? td3.Attributes["onclick"]?.Value;
//            if (onc != null)
//            {
//                var i0 = onc.IndexOf('\'') + 1;
//                var i1 = onc.IndexOf('\'', i0);
//                Url = onc.Substring(i0, i1 - i0);
//            }
//            category= new Category() { Index = Index, Name = Name, Url = Url };
//            return true;
//        }

//        public void Load()
//        {
//            var doc = Url.LoadHtml();
//            Produits.Parse(doc.GetElementbyId("id_finder_result"));
//        }

//        internal void DownloadImages()
//        {

//            foreach (var t in this.Produits)
//                t.DownloadImages();
//        }

//        internal void ExtractInfo()
//        {
//            Produits.ExtractInfo();
//        }
//        [OnDeserialized]
//        public void OnDeserialized(StreamingContext context)
//        {
//            Register();
//        }
//        private static Dictionary<string, Category> categories = new Dictionary<string, Category>();
//        public static Category GetCategory(string id)
//        {
//            if (categories.TryGetValue(id, out var c)) return c;
//            return null;
//        }
//        public void Register()
//        {
//            var ids = this.Index.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

//            if (ids.Length == 2)
//            {
//                var i0 = int.Parse(ids[0]);
//                var i1 = int.Parse(ids[1]);
//                for (; i0 <= i1; i0++)
//                {
//                    if (categories.TryGetValue(i0.ToString(), out var cat))
//                        cat.Subs.Add(this);
//                    else categories.Add(i0.ToString(), this);
//                }
//            }
//            else
//                foreach (var id in ids)
//                {
//                    if (categories.TryGetValue(id, out var cat))
//                        cat.Subs.Add(this);
//                    else categories.Add(id, this);
//                }
//        }

//        public static Category GetByProduitId(string pid)
//        {
//            var i0 = pid.IndexOf('.');
//            if (i0 == -1) return categories.TryGetValue("00", out var c) ? c : Category.Default;
//            pid = pid.Substring(0, i0);

//            if (categories.TryGetValue(pid, out var t)) return t;
//            return null;
//        }
//        public Category GetByName(string name)
//        {
//            foreach (var cat in this.Subs)
//                if (cat.Name == name) return cat;
//            return null;
//        }
//    }
//    [Serializable]
//    public class Categories:List<Category>
//    {
//        public Categories()
//        {
//        }
//        public Categories(HtmlNode node) => Parse(node);
//        public void Parse(HtmlNode node)
//        {
//            if (node == null) return;
//            var crbox = node.FirstHasClassName("crBox");
//            var tr = crbox?.FirstElementChild();
//            while (tr != null)
//            {
//                if (tr.ChildNodes.Count >= 3 && Category.TryParse(tr, out var cat))
//                {
//                    this.Add(cat);
//                    cat.Load();
//                }
//                tr = tr.NextElementSibling();
//            }
//        }

//        internal void ExtractInfo()
//        {
//            foreach (var item in this)
//                item.ExtractInfo();
//        }
//    }
//    [Serializable]
//    public class Produit
//    {
//        public static int Count = 0;
//        public Produit()
//        {
//            Count++;
//        }
//        public Produit(HtmlNode c)
//        {
//            Count++;
//            var img = c.FirstHasClassName("pBox_image")?.FirstElementChild();
//            var box = c.FirstHasClassName("pBox_text");
//            var num = box?.FirstHasClassName("pBox_artNr");
//            var name = box?.FirstHasClassName("pBox_title");
//            this.Url = c.Attributes["href"]?.Value;
//            this.Name = name.InnerText;
//            this.Id = num?.InnerText;
//            this.ImageUrl = getUrl(img.Attributes["style"]?.Value);
//        }

//        public string Url { get; }
//        public string Name { get; }
//        public string Id { get; }
//        public string ImageUrl { get; set; }
//        public string Description { get; set; }
//        public Produits Accessoires { get; private set; } = new Produits();

//        private static string getUrl(string url)
//        {
//            if (string.IsNullOrWhiteSpace(url)) return "";
//            var i0 = url.IndexOf("url(")+4;
//            var i1 = url.IndexOf(")", i0);
//            return url.Substring(i0, i1 - i0).Trim('\'');
//        }
//        static DirectoryInfo Dir = new DirectoryInfo("p:\\Catalogues");
//        internal void DownloadImages()
//        {
//            if (ImageUrl.StartsWith(".\\")) return;
//            var uri = new Uri(ImageUrl);
//            var path = Path.Combine(".\\Catalogues", Help.Replace(uri.AbsolutePath.Substring(1), '/', '\\'));
//            try
//            {
//                var file = new FileInfo(path);
//                if (file.Extension == "") return;
//                path = Path.Combine(file.Directory.FullName, file.Name.CleanFileName());
//                file = new FileInfo(path);
//                this.ImageUrl = file.FullName.Replace(Dir.FullName, ".");
//                if (file.Exists) return;
//                if (!file.Directory.Exists) file.Directory.Create();
//                try
//                {
//                    c.DownloadFile(uri, file.FullName);
//                }
//                catch (WebException e){ }
//                catch(Exception e) { ImageUrl = uri.ToString(); }
//            }
//            catch { }
            
//        }
//        static WebClient c = new WebClient();
//        public static int Extracted ;
//        public void ExtractInfo()
//        {
//            if (this.Description != null) return;
//            Extracted++;
//            var file = Path.Combine(Dir.FullName, "HtmlProducts", (Id + "--" + Name).CleanFileName() + ".html");
//            var x = Url.LoadHtml();
//            x.Save(file);
//            var r = x.GetElementbyId("id_referenz");
//            string refs = "";
//            while(r!=null)
//            {
//                if (r.Name == "script") break;
//                r = r.NextElementSibling();
//            }
//            if (r == null) return;
//            var s = r?.InnerText;
            
//            if (s == null)
//            {

//            }
//            else
//            {
//                const string cc = "$(\"#id_referenz\").load(\"";
//                var i0 = s.IndexOf(cc) + cc.Length;
//                var i1 = s.IndexOf("\"", i0);
//                refs = s.Substring(i0, i1 - i0);
//            }
            
//            this.Description = x.DocumentNode.FirstHasClassName("pdhText_text")?.InnerText?.Trim('\r', '\n', ' ');
//            if (!string.IsNullOrWhiteSpace(refs))
//            {
//                var x1 = refs.LoadHtml();
//                var result = x1.GetElementbyId("id_finder_result");
//                result = result.FirstElementChild();
//                while (result!=null)
//                {
//                    this.Accessoires.Add(Catalogues.GetProduct(result));
//                    result = result.NextElementSibling();
//                }
//            }
//        }
//        [OnDeserialized]
//        public void OnDeserializedMethod(StreamingContext context)
//        {
//            if (Accessoires == null)
//                Accessoires = new QServers.Produits();
//            Register(null, this);
//        }
//        private static Dictionary<string, Produit> Produits = new Dictionary<string, Produit>();
//        public static Produit Register(string @ref,Produit produit)
//        {
//            if (Produits.TryGetValue(@ref ?? produit.Id, out var p)) return p;
//            Produits.Add(@ref ?? produit.Id, produit);
//            return produit;
//        }

//        internal static Produit GetProduit(string id)
//        {
//            return Produits.TryGetValue(id, out var x) ? x : null;
//        }
//    }
//    [Serializable]
//    public class Produits:List<Produit>
//    {
//        public Produits()
//        {
//        }
        

//        public void Load(HtmlNode node)
//        {
//            var c = node.FirstElementChild();
//            if (c == null) return;
//            do
//            {
//                Add(new Produit(c));

//            } while ((c = c.NextElementSibling()) != null);
//        }

//        internal void ExtractInfo()
//        {

//            foreach (var item in this)
//                item.ExtractInfo();
//        }

//        internal void Parse(HtmlNode htmlNode)
//        {
//            var t = htmlNode.FirstElementChild();
//            while (t != null)
//            {
//                if (t.Name == "a")
//                    this.Add(new Produit(t));
//                t = t.NextElementSibling();
//            }
//        }
//    }
//}
