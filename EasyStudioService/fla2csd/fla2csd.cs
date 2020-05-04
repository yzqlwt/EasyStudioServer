using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace EasyStudioService.fla2csd
{
    class Fla2Csd
    {
        public static void tocsd()
        {
            if (!Directory.Exists(App.cachePath))
            {
                Directory.CreateDirectory(App.cachePath);
            }
            Directory.GetFiles(App.cachePath, "*").ToList().ForEach(File.Delete);
            string baseText = File.ReadAllText(@"./base.xml");
            XDocument xdoc = XDocument.Parse(baseText);
            XElement children = xdoc.Root.XPathSelectElements("/GameFile/Content/Content/ObjectData/Children").FirstOrDefault();
            IEnumerable<XElement> elements = children.XPathSelectElements("./*");
            var element = elements.FirstOrDefault<XElement>();
            var config = getConfig();
            config.ForEach((item) =>
            {
                XElement newEle = Clone<XElement>(element);
                newEle.Name = "AbstractNodeData";
                newEle = initImageView(item, newEle);
                children.Add(newEle);
            });
            element.Remove();
            xdoc.Save(Path.Combine(App.cachePath, "Layer.csd"));
            addToCCSDocument(config);
        }
        static List<ItemConfig> getConfig()
        {
            string configStr = File.ReadAllText(Path.Combine(App.dirPath, "config.json"));
            var dict = new Dictionary<string, string>();
            Directory.GetFiles(App.dirPath, "*.png").ToList().ForEach((path) => {
                var md5 = GetMD5HashFromFile(path);
                var name = Path.GetFileName(path);
                if (!dict.ContainsKey(md5))
                {
                    dict[md5] = name;
                }
            });
            var config = JsonConvert.DeserializeObject<SymbolConfig>(configStr);
            App.Activity = config.activity;
            App.Skin = config.skin;
            var symbols = config.symbols;
            for (var i = 0; i < symbols.Count(); i++)
            {
                var item = symbols[i];
                System.Drawing.Image image = System.Drawing.Image.FromFile(Path.Combine(App.dirPath, item.image));
                var height = image.Height;
                var width = image.Width;
                image.Dispose();
                var md5 = GetMD5HashFromFile(Path.Combine(App.dirPath, item.image));
                if (dict.ContainsKey(md5))
                {
                    item.image = dict[md5];
                    item.size = new Size();
                    item.size.height = height;
                    item.size.width = width;
                    File.Copy(Path.Combine(App.dirPath, item.image), Path.Combine(App.cachePath, item.image), true);
                }
            }
            symbols.Sort((a, b) => a.name.CompareTo(b.name));
            return symbols;
        }

        static XElement initImageView(ItemConfig config, XElement ele)
        {
            ele.SetAttributeValue("Name", config.name);
            var position = ele.XPathSelectElements("./Position").FirstOrDefault();
            position.SetAttributeValue("X", config.position.x);
            position.SetAttributeValue("Y", config.position.y);
            var fileData = ele.XPathSelectElements("./FileData").FirstOrDefault();
            var size = ele.XPathSelectElements("./Size").FirstOrDefault();
            size.SetAttributeValue("X", config.size.width);
            size.SetAttributeValue("Y", config.size.height);
            fileData.SetAttributeValue("Path", "res/ui/games/" + App.Activity + "/"+ App.Skin + "/" + config.image);
            return ele;
        }

        public static List<XElement> addToCCSDocument(List<ItemConfig>  config)
        {
            var path = App.ccsPath;
            string baseText = File.ReadAllText(path);
            XDocument xdoc = XDocument.Parse(baseText);
            XElement root = xdoc.Root;
            var children = root.XPathSelectElements("/Solution/SolutionFolder/Group/RootFolder/Folder[@Name='res']/Folder[@Name='ui']/Folder[@Name='games']/*").ToList();
            var activityEle = children.Find((ele) =>
            {
                return ele.Attribute("Name").Value == App.Activity;
            });
            if(activityEle == null)
            {
                activityEle = getFolderElement(App.Activity);
                children.Last().AddAfterSelf(activityEle);
            }
            var skinEle = activityEle.Elements().ToList().Find((ele) =>
            {
                return ele.Attribute("Name").Value == App.Skin;
            });
            if (skinEle != null)
            {
                skinEle.Remove();
            }
            skinEle = getFolderElement(App.Skin);
            activityEle.Add(skinEle);
            var ccsProjectPath = Path.GetDirectoryName(App.ccsPath);
            var targetDir = Path.Combine(ccsProjectPath, @"cocosstudio\res\ui\games", App.Activity, App.Skin);
            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, true);
            }
            Directory.CreateDirectory(targetDir);
            Directory.GetFiles(App.cachePath, "*").ToList().ForEach((file) =>
            {
                var extension = Path.GetExtension(file);
                var fileName = Path.GetFileName(file);
                if (extension == ".png")
                {
                    var imageEle = getImageElement(fileName);
                    skinEle.Add(imageEle);
                }
                else if(extension == ".csd")
                {
                    var csdEle = getProjectElement("Layer.csd");
                    skinEle.Add(csdEle);
                }
                File.Copy(file, Path.Combine(targetDir, fileName));
            });

            xdoc.Save(App.ccsPath);
            return children;
        }

        static XElement getFolderElement(string name)
        {
            XElement folder = new XElement("Folder",
                new XAttribute("Name", name));
            return folder;
        }

        static XElement getImageElement(string name)
        {
            XElement folder = new XElement("Image",
                new XAttribute("Name", name));
            return folder;
        }

        static XElement getProjectElement(string name)
        {
            XElement folder = new XElement("Project",
                new XAttribute("Name", name),
                new XAttribute("Type", "Layer"));
            return folder;
        }

        public static T Clone<T>(T obj)
        {
            T ret = default(T);
            if (obj != null)
            {
                XmlSerializer cloner = new XmlSerializer(typeof(T));
                MemoryStream stream = new MemoryStream();
                cloner.Serialize(stream, obj);
                stream.Seek(0, SeekOrigin.Begin);
                ret = (T)cloner.Deserialize(stream);
            }
            return ret;
        }
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
    }
}
