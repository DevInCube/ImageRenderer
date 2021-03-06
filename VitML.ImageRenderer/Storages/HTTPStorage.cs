﻿using NLog;
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
    public class HTTPStorage : ImageStorage
    {

        Logger logger = LogManager.GetLogger("HTTPStorage");
        Stopwatch sw = new Stopwatch();

        private WebClient client = new WebClient();

        private string imageUri;
        private string timeUri;
        private long lastImageTime = 0;

        public HTTPStorage(string fileUri, string timeUri)
        {
            this.imageUri = fileUri;
            this.timeUri = timeUri;
        }

        public override ImageItem Load(string id)
        {
            ImageItem item = new ImageItem();
            try
            {
                sw.Reset();
                sw.Start();
                byte[] timeBuf = client.DownloadData(timeUri);
                string timeStr = System.Text.Encoding.UTF8.GetString(timeBuf);
                long timeStamp = 0;
                bool parsed = long.TryParse(timeStr, out timeStamp);
                if (!parsed || timeStamp == lastImageTime)
                    return item;
                lastImageTime = timeStamp;
                byte[] imageBuf = client.DownloadData(imageUri);
                logger.Trace(sw.ElapsedTicks);
                if (imageBuf == null || imageBuf.Length == 0)
                {
                    return item;
                }
                item.ImageData = imageBuf;
                item.Time = timeStamp;
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
