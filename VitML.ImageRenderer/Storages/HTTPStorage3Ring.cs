using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using VitML.ImageRenderer.Core;
using VitML.ImageRenderer.Extensions;

namespace VitML.ImageRenderer.Storages
{
    public class HTTPStorage3Ring : ImageStorage
    {

        static Logger logger = LogManager.GetLogger("HTTPStorage3Ring");

        private WebClient client = new WebClient();
        private Stopwatch timer = new Stopwatch();

        private string dirUri;
        private string timeUri;

        private long lastTime = 0;
        private long lastPreTime = 0;

        public HTTPStorage3Ring(string dirUri, string testUri)
        {
            this.dirUri = dirUri;
            this.timeUri = testUri;
        }

        public override ImageItem Load(string id)
        {
            ImageItem item = new ImageItem();
            string testStr = null;
            try
            {
                /*
                byte[] data = File.ReadAllBytes("C:/Users/user/Desktop/test.jpeg");
                if (IsValidImage(data))
                {
                    item.ImageData = data;
                    item.Time = DateTime.Now.Ticks;
                }
                System.Threading.Thread.Sleep(100);
                return item;*/
                do
                {
                    byte[] testBuf = client.DownloadData(timeUri);
                    testStr = System.Text.Encoding.UTF8.GetString(testBuf);                    
                } 
                while (testStr.Equals("0"));
                long time = 0;
                bool parsed = long.TryParse(testStr, out time);
                if (parsed && time > 0)
                {
                    logger.Info("Wait {0}", DateTime.Now.Millisecond - lastPreTime);
                    lastPreTime = DateTime.Now.Millisecond;
                    string imageUri = String.Format("{0}{1}.jpeg", dirUri, time);
                    timer.Reset();
                    timer.Start();
                    byte[] imageBuf = client.DownloadData(imageUri);
                    if (!IsValidImage(imageBuf)) return item;          
                    long elapsed = timer.ElapsedMilliseconds;
                    item.ImageData = imageBuf;
                    item.Time = time;

                    long showTime = (int)((time - lastTime)/(double)TimeSpan.TicksPerMillisecond);
                    logger.Info("Loaded at {0} ImgTime {1} Ticks:{2}  DownloadTime:{3} [m] ShowTime: {4} [m]", DateTime.Now, new DateTime(time), time, elapsed, showTime);
                    lastTime = time;
                }
                else
                {
                    logger.Error("{0} not parsed to long or is `0`", testStr);
                }
            }
            catch (WebException)
            {
                logger.Error("404 for {0}", testStr);
            }
            catch (Exception e2)
            {
                logger.Error(e2.ToString());
            }
            return item;
        }

        public static bool IsValidImage(byte[] bytes)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                    Image.FromStream(ms);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        public override void Connect()
        {
            //
        }

        public override void Disconnect()
        {
            //
        }

        public override IEnumerable<string> GetImages()
        {
            return new string[0];
        }

        public override bool IsAvailable(string id)
        {
            return true;
        }

        public override bool Save(string id, ImageItem image)
        {
            return true;
        }

        public override bool Remove(string id)
        {
            return true;
        }

        public override bool Save(string id, byte[] data)
        {
            return true;
        }
    }
}
