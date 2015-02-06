using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using VitML.ImageRenderer.Models;

namespace VitML.ImageRenderer.Loaders
{
    public interface IImageLoader
    {

        event Action<BitmapImage> ImageLoaded;

        void Init(RendererConfig config);
        void Close();
    }
}
