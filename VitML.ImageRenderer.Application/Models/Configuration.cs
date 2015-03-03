using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using VitML.ImageRenderer.App.ViewModels;
using VitML.ImageRenderer.App.Views;
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
        [XmlArray("instances")]
        [XmlArrayItem("window")]
        public List<WindowConfiguration> Instances { get; set; }

        public Configuration()
        {
            Connections = new List<ConnectionConfiguration>();
            Storages = new List<StorageConfiguration>();
            Instances = new List<WindowConfiguration>();
        }

        public void Initialize()
        {
            Storages.ForEach(x => x.Initialize(this));
            Instances.ForEach(x => { x.Initialize(this); });
        }

        public void Prepare()
        {
            Instances.ForEach(x => { x.Prepare(this); });
        }

        public void Start()
        {
            Instances.ForEach(x => { if (x.IsEnabled) x.Start(); });
        }

        public static Configuration Parse(string content)
        {
            return SerializationHelper.DeserializeObject<Configuration>(content);
        }

        public string Serialize()
        {
            return SerializationHelper.SerializeObject(this);
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


        public void AddObject<T>(T obj)
        {
            Type genType = typeof(T);
            if (genType == typeof(ConnectionConfiguration))
            {
                Connections.Add(obj as ConnectionConfiguration);
            }
            else if (genType == typeof(StorageConfiguration))
            {
                Storages.Add(obj as StorageConfiguration);
            }            
        }

        
    }    

    public class WindowConfiguration
    {

        [XmlAttribute("enabled")]
        public bool IsEnabled { get; set; }
        [XmlAttribute("title")]
        public string Title { get; set; }
        [XmlAttribute("showfps")]
        public bool ShowFPS { get; set; }
        [XmlElement("player")]
        public PlayerConfiguration Player { get; set; }

        public WindowConfiguration()
        {
            Title = "Window";
            ShowFPS = true;
            Player = new PlayerConfiguration();
        }

        public void Initialize(IObjectProvider op)
        {
            this.Player.Initialize(op);
        }

        public void Start()
        {
            MainWindow w = new MainWindow();
            (w.DataContext as MainVM).Setup(this);
            w.Show();
        }

        public void Prepare(IObjectProvider op)
        {
            Player.Prepare(op);
        }
    }
}
