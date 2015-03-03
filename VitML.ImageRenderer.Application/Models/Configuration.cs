using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
using VitML.ImageRenderer.ViewModels;

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

    public class WindowConfiguration : ObservableObject
    {

        private int _PosX, _PosY, _Width, _Height;

        [XmlAttribute("enabled")]
        public bool IsEnabled { get; set; }
        [XmlAttribute("title")]
        public string Title { get; set; }
        [XmlAttribute("showfps")]
        public bool ShowFPS { get; set; }
        [XmlAttribute("x")]
        public int PosX { get { return _PosX; } set { _PosX = value; OnPropertyChanged("PosX"); } }
        [XmlAttribute("y")]
        public int PosY { get { return _PosY; } set { _PosY = value; OnPropertyChanged("PosY"); } }
        [XmlAttribute("width")]
        public int Width { get { return _Width; } set { _Width = value; OnPropertyChanged("Width"); } }
        [XmlAttribute("height")]
        public int Height { get { return _Height; } set { _Height = value; OnPropertyChanged("Height"); } }
        [XmlAttribute("showborder")]
        public bool ShowBorder { get; set; }
        [XmlElement("player")]
        public PlayerConfiguration Player { get; set; }
        [XmlIgnore]
        public MainWindow Window { get; set; }

        public WindowConfiguration()
        {
            Title = "Window";
            ShowFPS = true;
            ShowBorder = true;
            PosX = PosY = 50;
            Width = Height = 200;
            Player = new PlayerConfiguration();
        }

        public void Initialize(IObjectProvider op)
        {
            this.Player.Initialize(op);
            if (Width < 200) Width = 200;
            if (Height < 200) Height = 200;
        }

        public void Start()
        {
            if(Window!=null && Window.IsVisible)
            {
                Window.Close();
            }
            Window = new MainWindow();
            if (!ShowBorder)
            {
                Window.WindowStyle = System.Windows.WindowStyle.None;
                Window.AllowsTransparency = true;
                Window.Background = System.Windows.Media.Brushes.Transparent;
            }
            Window.Top = PosY;
            Window.Left = PosX;
            Window.Width = Width;
            Window.Height = Height;
            Window.LocationChanged += Window_LocationChanged;
            Window.SizeChanged += Window_SizeChanged;
            (Window.DataContext as MainVM).Setup(this);
            Window.Show();
        }

        void Window_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            Width = (int)Window.Width;
            Height = (int)Window.Height;     
        }

        void Window_LocationChanged(object sender, EventArgs e)
        {
            PosX = (int)Window.Left;
            PosY = (int)Window.Top;
        }

        public void Prepare(IObjectProvider op)
        {            
            Player.Prepare(op);
        }
    }
}
