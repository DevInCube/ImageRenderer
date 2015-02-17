using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using VitML.ImageRenderer.Models;
using VitML.ImageRenderer.ViewModels;

namespace VitML.ImageRenderer.Core
{
    public class Renderer : ObservableObject
    {

        private string _FPS;
        private Visibility _FPSVisible;
        private BitmapImage _Image;

        public BitmapImage Image
        {
            get { return _Image; }
            private set
            {
                _Image = value;
                OnPropertyChanged("Image");
            }
        }
        public string FPS
        {
            get { return _FPS; }
            private set
            {
                _FPS = value;
                OnPropertyChanged("FPS");
            }
        }
        public Visibility FPSVisible
        {
            get { return _FPSVisible; }
            private set
            {
                _FPSVisible = value;
                OnPropertyChanged("FPSVisible");
            }
        }

        public Renderer()
        {
            //
        }

        public void Setup(RendererConfig config)
        {
            //
        }
    }
}
