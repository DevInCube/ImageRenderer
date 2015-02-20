using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using VitML.ImageRenderer.Core;
using VitML.ImageRenderer.Extensions;

namespace VitML.ImageRenderer.Storages
{
    public class FileStoragePassive : ImageStorage
    {

        private string imageUri;
        private long prevTime = 0;

        public FileStoragePassive(string fileUri)
        {
            this.imageUri = fileUri;
        }

        public override ImageItem Load(string id)
        {
            ImageItem item = new ImageItem();
            try
            {
                long time = File.GetLastWriteTime(imageUri).Ticks;
                if (time <= prevTime)
                    return item;
                item.ImageData = File.ReadAllBytes(imageUri);
                item.Time = time;
            }
            catch (IOException)
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
