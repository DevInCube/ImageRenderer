using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using VitML.ImageRenderer.Configurations;
using VitML.ImageRenderer.Core;
using VitML.ImageRenderer.Extensions;

namespace VitML.ImageRenderer.App.Models
{

    [XmlType("configuration")]
    public class Configuration : IObjectProvider
    {

        [XmlArray("connections")]
        [XmlArrayItem("connection")]
        public List<ConnectionConfiguration> Connections { get; set; }
        [XmlArray("storages")]
        [XmlArrayItem("storage")]
        public List<StorageConfiguration> Storages { get; set; }
        [XmlElement("player")]
        public PlayerConfiguration Player { get; set; }
        [XmlElement("window")]
        public WindowConfiguration Window { get; set; }

        public Configuration()
        {
            Connections = new List<ConnectionConfiguration>();
            Storages = new List<StorageConfiguration>();
            Player = new PlayerConfiguration();
            Window = new WindowConfiguration();
        }

        public void Initialize()
        {
            Storages.ForEach(x => x.Initialize(this));
            Player.Initialize(this as IObjectProvider);
        }

        public static Configuration Parse(string content)
        {
            return SerializationHelper.DeserializeObject<Configuration>(content);
        }

        public T GetObject<T>(string key)
        {
            Type genType = typeof(T);
            object obj = null;
            if (genType == typeof(ConnectionConfiguration))
            {
                obj = Connections.FirstOrDefault(x => x.Name == key);
            }
            else if (genType == typeof(StorageConfiguration))
            {
                obj = Storages.FirstOrDefault(x => x.Name == key);
            }
            if (obj == null) return default(T);
            return (T)obj;
        }
    }    

    public class WindowConfiguration
    {

        [XmlAttribute("title")]
        public string Title { get; set; }
        [XmlAttribute("showfps")]
        public bool ShowFPS { get; set; }

        public WindowConfiguration()
        {
            Title = "Window";
            ShowFPS = true;
        }
    }
}
