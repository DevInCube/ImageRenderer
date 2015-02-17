using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;
using VitML.ImageRenderer.Extensions;
using VitML.ImageRenderer.Models;

namespace VitML.ImageRenderer.Loaders
{
    public class TimedImageLoader :  IImageLoader
    {

        Logger logger = LogManager.GetLogger("TimedImageLoader");

        public class TimedImage
        {

            public BitmapImage Image { get; set; }
            public string Name { get; set; }
            public long ShowTime { get; set; }

            public override string ToString()
            {
                return String.Format("{0} : {1}", Name, ShowTime);
            }
        }

        private RendererConfig config;
        private long frameTime = 1;
        private bool running = true;
        private DirectoryInfo dirInfo;
        private System.Threading.Thread renderThread, pullThread;
        private List<TimedImage> imgBuffer = new List<TimedImage>();

        public event Action<BitmapImage> ImageLoaded;

        public void Init(RendererConfig config)
        {
            this.config = config;
            dirInfo = new DirectoryInfo(config.Directory);
            if (config.Cleanup)
            {
                try
                {
                    foreach (FileInfo file in dirInfo.GetFiles())
                    {
                        file.Delete();
                    }
                }
                catch (Exception e)
                {
                    //
                }
            }
            if (config.UpdateFrequency > 0)
                frameTime = 1000 / config.UpdateFrequency;
            pullThread = new System.Threading.Thread(() => { PullRun(); });
            renderThread = new System.Threading.Thread(() => { RenderRun(); });
            pullThread.Start();
            renderThread.Start();
        }

        public void Close()
        {
            running = false;
            pullThread.Interrupt();
            renderThread.Interrupt();
        }

        void RenderRun()
        {
            try
            {
                long withinFramesFactor = 0L;
                while (running)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    withinFramesFactor = frameTime;
                    lock (imgBuffer)
                    {
                        if (imgBuffer.Count > 0)
                        {
                            TimedImage ti = imgBuffer.First();
                            if (ti.ShowTime > withinFramesFactor)
                            {
                                withinFramesFactor = ti.ShowTime;
                            }
                            BitmapImage src = ti.Image;
                            if (config.Render)
                            {
                                //logger.Trace("Render > {0}", ti);
                                if (ImageLoaded != null)
                                    ImageLoaded(src);
                            }
                            if (config.Compress)
                            {
                                //logger.Trace("Compress > {0}", ti);
                                CompressImage(src, ti.Name);
                            }
                            imgBuffer.Remove(ti);
                        }
                    }
                    long elapsed = sw.ElapsedMilliseconds;
                    sw.Reset();
                    int delay = (int)(frameTime - elapsed);
                    if (delay > 1)
                    {
                       
                        System.Threading.Thread.Sleep(delay);
                    }
                }
            }
            catch (System.Threading.ThreadInterruptedException)
            {
                //
            }
            catch (Exception e)
            {
                MessageBox.Show("Render: " + e.Message);
                running = false;
            }
        }

        void PullRun()
        {
            try
            {
                long lastLoadedFrameTime = 0L;
                while (running)
                {
                    long sourceFrameTime = 0;
                    long targetFrameTime = frameTime;
                    FileInfo[] files = dirInfo.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        if (IOHelper.IsFileLocked(file))
                            continue;
                        string timeStr = file.Name.Split('.')[0];
                        long currentFrameTime = long.Parse(timeStr);
                        long timePassedFromLast = currentFrameTime - lastLoadedFrameTime;
                        bool render = timePassedFromLast >= frameTime;
                        if (render)
                        {
                            if (lastLoadedFrameTime == 0)
                                lastLoadedFrameTime = currentFrameTime - 10;
                            sourceFrameTime = timePassedFromLast;
                            lastLoadedFrameTime = currentFrameTime;
                            long showTime;
                            if (sourceFrameTime > targetFrameTime)
                                showTime = sourceFrameTime;
                            else
                                showTime = targetFrameTime;
                            try
                            {
                                LoadImage(file, showTime);
                            }
                            catch { continue; }
                        }
                        if (config.DeleteImages)
                        {
                            //logger.Trace("Delete > {0}", file);
                            try
                            {
                                file.Delete();
                            }
                            catch { continue; }
                        }
                        if (render) break;
                    }
                }
            }
            catch (System.Threading.ThreadInterruptedException)
            {
                //
            }
            catch (Exception e)
            {
                MessageBox.Show("Pull: "+e.Message +" :"+e.GetType());
                running = false;
            }
        }

        private void LoadImage(FileInfo file, long time)
        {
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(file.FullName, UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            src.Freeze();
            lock (imgBuffer)
            {
                imgBuffer.Add(new TimedImage() { 
                    Image = src, 
                    Name =  file.Name.Split('.')[0],
                    ShowTime = time 
                });
            }
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


    }
}
