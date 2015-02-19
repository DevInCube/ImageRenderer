using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace VitML.ImageRenderer.Core
{
    public abstract class ImageStorage
    {

        public event Action<string> ImageAdded;

        public abstract void Connect();
        public abstract void Disconnect();
        public abstract IEnumerable<string> GetImages();
        public abstract bool IsAvailable(string id);
        public abstract BitmapImage Load(string id);
        public abstract bool Save(string id, BitmapImage image);
        public abstract bool Save(string id, byte[] data);
        public abstract bool Remove(string id);

        protected void NotifyAdded(string id)
        {
            if (this.ImageAdded != null)
            {
                this.ImageAdded.Invoke(id);
            }
        }
    }
}
