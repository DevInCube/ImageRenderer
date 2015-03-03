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
        private bool _ShowFPS;
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
        public bool ShowFPS
        {
            get { return _ShowFPS; }
            private set
            {
                _ShowFPS = value;
                OnPropertyChanged("ShowFPS");
            }
        }

        public ImagePlayer Player { get { return _Player; } }

        public MainVM()
        {
            //
        }

        public void Setup(WindowConfiguration config)
        {
            this.WindowTitle = config.Title;
            this.ShowFPS = config.ShowFPS;
            Player.Setup(config.Player);
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
