using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Drawing;

namespace Fla2Csd
{
    class Program
    {
        private static string Activity = "game1000";
        private static string dirPath = App.dirPath;
        private static string cachePath = Path.Combine(dirPath, Activity);
        static void Main(string[] args)
        {
            string baseText = File.ReadAllText(@"./base.xml");
            XDocument xdoc = XDocument.Parse(baseText);
            XElement rootSchool = xdoc.Root;
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
            xdoc.Save(Path.Combine(cachePath, "Layer.csd"));
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
            fileData.SetAttributeValue("Path", "res/ui/games/"+Activity+"/skin01/"+config.image);
            return ele;
        }

        static List<ItemConfig> getConfig()
        {
            
            
            if (Directory.Exists(cachePath))
            {
                Directory.Delete(cachePath, true);
            }
            Directory.CreateDirectory(cachePath);
            string configStr = File.ReadAllText(Path.Combine(dirPath, "config.json"));
            var dict = new Dictionary<string, string>();
            Directory.GetFiles(dirPath, "*.png").ToList().ForEach((path) => {
                var md5 = GetMD5HashFromFile(path);
                var name = Path.GetFileName(path);
                if (!dict.ContainsKey(md5))
                {
                    dict[md5] = name;
                }
            });
            var config = JsonConvert.DeserializeObject<List<ItemConfig>>(configStr);
            for( var i = 0; i< config.Count(); i++)
            {
                var item = config[i];
                System.Drawing.Image image = System.Drawing.Image.FromFile(Path.Combine(dirPath, item.image));
                var height = image.Height;
                var width = image.Width;
                image.Dispose();
                var md5 = GetMD5HashFromFile(Path.Combine(dirPath, item.image));
                if (dict.ContainsKey(md5))
                {
                    item.image = dict[md5];
                    item.size = new Size();
                    item.size.height = height;
                    item.size.width = width;
                    File.Copy(Path.Combine(dirPath, item.image), Path.Combine(cachePath, item.image), true);
                }
            }
            config.Sort((a, b) => a.name.CompareTo(b.name));
            return config;
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
        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <param name="fileName">文件绝对路径</param>
        /// <returns>MD5值</returns>
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



