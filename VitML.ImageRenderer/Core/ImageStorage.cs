using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace VitML.ImageRenderer.Core
{
    public abstract class ImageStorage
    {

        public bool IsConnected { get; }

        public event Action<string> ImageAdded;

        public abstract void Connect();
        public abstract void Disconnect();
        public IEnumerable<string> GetImages();
        public abstract bool IsAvailable(string id);
        public abstract BitmapImage Load(string id);
        public abstract bool Save(string id, BitmapImage image);
        public abstract bool Remove(string id);
    }
}
