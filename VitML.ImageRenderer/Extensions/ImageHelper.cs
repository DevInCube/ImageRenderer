using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media.Imaging;

namespace VitML.ImageRenderer.Extensions
{
    public static class ImageHelper
    {

        static Logger logger = LogManager.GetLogger("ImageHelper");
        static Stopwatch sw = new Stopwatch();

        public static Image ResizeImage(Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return (Image)b;
        }

        public static BitmapSource ByteArrayToBitmapSource(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            return System.Windows.Media.Imaging.BitmapFrame.Create(stream);
        }

        public static BitmapImage ByteArrayToBitmapImage(byte[] bytes)
        {
            BitmapImage bImg = null;
            try
            {
               System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
               {
                   try
                   {
                       using (MemoryStream memoryStream = new MemoryStream(bytes))
                       {
                           bImg = new BitmapImage();
                           bImg.BeginInit();
                           bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
                           bImg.EndInit();
                           bImg.Freeze();
                       }
                   }
                   catch (Exception e1) { }
               }));         
            }
            catch(Exception e2) { }
            return bImg;
        }

        public static System.Windows.Media.Imaging.BitmapImage ToImage(byte[] bytes)
        {
            try
            {
                using (Stream ms = new System.IO.MemoryStream(bytes))
                {
                    System.Windows.Media.Imaging.BitmapImage image = null;
                    try
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            try
                            {
                                sw.Reset();
                                sw.Start();
                                image = new System.Windows.Media.Imaging.BitmapImage();
                                image.BeginInit();
                                image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                                image.StreamSource = ms;
                                image.EndInit();
                                image.Freeze();
                            }
                            catch (Exception)
                            {
                                //
                            }
                            //logger.Trace("Convert: " + sw.ElapsedTicks);
                        }));
                    }
                    catch (NullReferenceException)
                    {
                        //
                    }
                    //logger.Trace(sw.ElapsedTicks);
                    return image;
                }
            }
            catch (Exception e)
            {
                //
            }
            return null;
        }

        public static System.Windows.Media.Imaging.BitmapImage ToImageUnsafe(byte[] bytes)
        {
            System.Windows.Media.Imaging.BitmapImage image = null;
            using (Stream ms = new System.IO.MemoryStream(bytes))
            {                
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    sw.Reset();
                    sw.Start();
                    image = new System.Windows.Media.Imaging.BitmapImage();
                    image.BeginInit();
                    image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    image.Freeze();
                }));
            }
            return image;
        }             
    }
}
