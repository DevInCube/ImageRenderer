using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using VitML.ImageRenderer.Extensions;

namespace VitML.ImageRenderer.Test
{
    public class ImagePusher
    {

        private string directoryToPull;
        private string directoryToPush;

        private Bitmap[] bitmaps;
        private int indexer = 0;
        private Timer timer;
        private NameMaker nameMaker;

        public long Interval { get; set; }

        public ImagePusher(
                string pull,
                string push
            )
        {
            this.directoryToPull = pull;
            this.directoryToPush = push;
            LoadBitmaps();
            Interval = 100;
            timer = new Timer();
            timer.Interval = Interval;
            timer.Elapsed += timer_Elapsed;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Interval = Interval;
            PushImage();
        }

        void LoadBitmaps()
        {
            DirectoryInfo di = new DirectoryInfo(directoryToPull);
            FileInfo[] files = di.GetFiles("*.bmp");
            bitmaps = new Bitmap[files.Length];
            int i = 0;
            Size imageSize = new Size(20, 20);
            nameMaker = new NameMaker(imageSize.Width, imageSize.Height);
            foreach (FileInfo fi in files)
            {
                Bitmap bmp = (Bitmap)Bitmap.FromFile(fi.FullName);
                bmp = (Bitmap)ImageHelper.ResizeImage(bmp, imageSize);
                bitmaps[i++] = bmp;
            }
        }

        void PushImage()
        {
            Bitmap bmp = bitmaps[indexer];
            Guid guid = Guid.NewGuid();
            string name = nameMaker.Create();
            try
            {
                bmp.Save(directoryToPush + "\\" + name + ".bmp");
            }
            catch { }
            indexer++;
            if (indexer >= bitmaps.Length)
                indexer = 0;
        }

        public void StartPush()
        {
            timer.Start();
        }

        public void StopPush()
        {
            timer.Stop();
        }

    }

    class NameMaker
    {
        private readonly int _width;

        private readonly int _height;

        public NameMaker(int width, int height)
        {
            this._width = width;
            this._height = height;
        }

        public string Create()
        {
            var msCount = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            return string.Format("{0}_{1}_{2}", (Int64)msCount, _width, _height);
        }
    }
}
