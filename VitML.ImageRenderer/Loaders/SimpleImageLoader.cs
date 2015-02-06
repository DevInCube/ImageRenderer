using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Media.Imaging;
using VitML.ImageRenderer.Extensions;
using VitML.ImageRenderer.Models;

namespace VitML.ImageRenderer.Loaders
{
    public class SimpleImageLoader : IImageLoader
    {

        private Timer puller = new Timer();
        private DirectoryInfo dirInfo;
        private bool processed = true;
        private RendererConfig config;

        public event Action<BitmapImage> ImageLoaded;

        public void Init(RendererConfig config)
        {
            this.config = config;
            dirInfo = new DirectoryInfo(config.Directory);
            SetInterval();
            puller.Elapsed += puller_Elapsed;
            puller.Start();
        }

        void SetInterval()
        {
            if (config.UpdateFrequency > 0)
                puller.Interval = 1000 / config.UpdateFrequency;
            else
                puller.Interval = 1;
        }

        void puller_Elapsed(object sender, ElapsedEventArgs e)
        {
            SetInterval();
            if (!processed) return;
            processed = false;
            FileInfo[] files = dirInfo.GetFiles();
            bool delete = false;
            for (int i = files.Length - 1; i >= 0; i--)
            {
                FileInfo file = files[i];
                if (IOHelper.IsFileLocked(file))
                    continue;
                if (delete)
                {
                    try
                    {
                        if (config.DeleteImages)
                            file.Delete();
                    }
                    catch { }
                    continue;
                }
                try
                {
                    LoadImage(file);
                    try
                    {
                        if (config.DeleteImages)
                            file.Delete();
                    }
                    catch { }
                    GC.Collect();
                }
                catch { }
                delete = true;
            }
            processed = true;
        }

        private void LoadImage(FileInfo fi)
        {
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(fi.FullName, UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            src.Freeze();
            if (config.Compress)
                CompressImage(src, fi.Name.Split('.')[0]);
            if (config.Render)
                if (ImageLoaded != null)
                    ImageLoaded(src);
        }

        private void CompressImage(BitmapImage src, string fileName)
        {
            long quality = config.CompressQuality;
            if (quality < 0L) quality = 0L;
            if (quality > 100L) quality = 100L;
            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
            System.Drawing.Imaging.Encoder myEncoder =
                System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, quality);
            myEncoderParameters.Param[0] = myEncoderParameter;
            string path = config.CompressDirectory + "\\" + fileName + ".jpg";
            BitmapImage2Bitmap(src).Save(path, jgpEncoder, myEncoderParameters);
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                    return codec;
            }
            return null;
        }


        public void Close()
        {
            //throw new NotImplementedException();
        }
    }
}
