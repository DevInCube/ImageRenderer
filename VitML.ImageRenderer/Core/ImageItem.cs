using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace VitML.ImageRenderer.Core
{
    public class ImageItem
    {

        public BitmapImage Image { get; set; }
        public string Name { get; set; }
        public long ShowTime { get; set; }
        public long Time { get; set; }

        public override string ToString()
        {
            return String.Format("{0} : {1}", Name, ShowTime);
        }
    }
}
