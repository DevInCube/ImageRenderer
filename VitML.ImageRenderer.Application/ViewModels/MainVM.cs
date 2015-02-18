using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using VitML.ImageRenderer.App.Models;
using VitML.ImageRenderer.Core;
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
            /*string exeDir = Directory.GetCurrentDirectory();
              string configPath = exeDir + "\\config.xml";
              var configLoader = new XMLConfigLoader(configPath);
              WindowConfig config = configLoader.Load<WindowConfig>();*/
            var config = new WindowConfig()
            {
                Directory = @"C:\Users\user\Desktop\res",
                DeleteImages = false,
                UpdateFrequency = 0
            };
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
