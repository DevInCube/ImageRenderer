using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using VitML.ImageRenderer.App.Models;
using VitML.ImageRenderer.Core;
using VitML.ImageRenderer.Loaders;
using VitML.ImageRenderer.ViewModels;

namespace VitML.ImageRenderer.App.ViewModels
{
    public class MainVM : ObservableObject
    {

        private ImagePlayer _Player = new ImagePlayer();
        private string _WindowTitle;

        public string WindowTitle
        {
            get { return _WindowTitle; }
            private set
            {
                _WindowTitle = value;
                OnPropertyChanged("WindowTitle");
            }
        }

        public ImagePlayer Player { get { return _Player; } }

        public MainVM()
        {
            WindowConfig config;
            if (false)
            {
                string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string configPath = Path.Combine(exeDir, "config.xml");
                var configLoader = new XMLConfigLoader(configPath);
                config = configLoader.Load<WindowConfig>();
            }
            else { 
                config = new WindowConfig()
                {
                    Title = "Test",
                    Directory = @"C:\Users\user\Desktop\res",
                    Cleanup = false,
                    DeleteImages = false,
                    UpdateFrequency = 0,
                    ShowFPS = true
                };
            }
            this.Setup(config);
        }

        public void Setup(WindowConfig config)
        {
            this.WindowTitle = config.Title;
            Player.Setup(config);
        }

        public void Loaded()
        {
            Player.Start();
        }

        public void Closed()
        {
            Player.Stop();
        }
    }
}
