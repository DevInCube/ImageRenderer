using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Media.Imaging;
using VitML.ImageRenderer.Core;
using VitML.ImageRenderer.Extensions;

namespace VitML.ImageRenderer.Storages
{
    public class HTTPStorage : ImageStorage
    {

        private string imageUri;
        private string timeUri;

        public HTTPStorage(string fileUri, string timeUri)
        {
            this.imageUri = fileUri;
            this.timeUri = timeUri;
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

        private long prevTime = 0;

        public override ImageItem Load(string id)
        {
            ImageItem item = new ImageItem();
            try
            {
                if (true)
                {
                    long time = File.GetLastWriteTime(imageUri).Ticks;
                    if (time <= prevTime)
                        return item;
                    byte[] data = File.ReadAllBytes(imageUri);
                    item.Image = ImageHelper.ToImage(data);
                    item.Time = time;
                    prevTime = item.Time;
                }
                else
                {
                    WebClient myWebClient = new WebClient();
                    byte[] imageBuf = myWebClient.DownloadData(imageUri);
                    byte[] timeBuf = myWebClient.DownloadData(timeUri);
                    string timeStr = System.Text.Encoding.UTF8.GetString(timeBuf);
                    long timeStamp = long.Parse(timeStr);
                    item.Image = ImageHelper.ToImage(imageBuf);
                    item.Time = timeStamp;
                }
            }
            catch (WebException)
            {
                //
            }
            catch (IOException)
            {
                //
            }
            return item;
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
