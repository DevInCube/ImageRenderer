using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using VitML.ImageRenderer.Extensions;

namespace VitML.ImageRenderer.Core
{
    public class ImageItem
    {

        public string Name { get; set; }
        public byte[] ImageData { get; set; }
        public BitmapImage Image { get; private set; }
        public long ShowTime { get; set; }
        public long Time { get; set; }

        public void Convert()
        {
            if (Image == null && ImageData != null)
                Image = ImageHelper.ToImage(ImageData);
        }

        public override string ToString()
        {
            return String.Format("{0} : {1}", Name, ShowTime);
        }
    }
}
