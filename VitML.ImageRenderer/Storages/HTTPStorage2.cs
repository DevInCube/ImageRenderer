using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class HTTPStorage2 : ImageStorage
    {

        static Logger logger = LogManager.GetLogger("HTTPStorage2");

        private WebClient client = new WebClient();

        private string imageUri;
        private string testUri;

        public HTTPStorage2(string fileUri, string testUri)
        {
            this.imageUri = fileUri;
            this.testUri = testUri;
        }

        public override ImageItem Load(string id)
        {
            Stopwatch sw = new Stopwatch();
            ImageItem item = new ImageItem();
            try
            {
                string testStr = null;
                sw.Reset();
                sw.Start();
                do
                {
                    byte[] testBuf = client.DownloadData(testUri);
                    testStr = System.Text.Encoding.UTF8.GetString(testBuf);
                } 
                while (!testStr.Equals("Yes"));
                long reqElapsed = sw.ElapsedMilliseconds;
                sw.Reset();
                sw.Start();
                byte[] imageBuf = client.DownloadData(imageUri);
                long elapsed = sw.ElapsedMilliseconds;
                item.ImageData = imageBuf;
                item.Time = DateTime.Now.Ticks;
                logger.Info("Request: {0} [millis]; Size: {1} [KB]; Time: {2} [millis]", reqElapsed, (imageBuf.Length / 1024), elapsed);
            }
            catch (WebException)
            {
                //
            }
            return item;
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
