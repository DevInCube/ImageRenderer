using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VitML.ImageRenderer.Models;

namespace VitML.ImageRenderer.Core
{
    public class ImageBuffer
    {

        private bool useSourceFPS = false;
        private long frameTime = 1;
        private List<ImageItem> RenderItems;
        private List<string> DeleteItems;
        private ImageStorage imageStorage;

        public ImageBuffer(ImageStorage storage)
        {
            this.RenderItems = new List<ImageItem>();
            DeleteItems = new List<string>();
            this.imageStorage = storage;
        }

        public void Setup(RendererConfig config)
        {
            //frameTime =  //@todo
            //useSourceFPS = //@todo 
        }

        public void Start()
        {
            imageStorage.Connect();
            List<string> items = imageStorage.GetImages().ToList();
            foreach (var item in items)
                ProcessItem(item);
            imageStorage.ImageAdded += imageStorage_ImageAdded;
        }

        public void Stop()
        {
            imageStorage.Disconnect();
            imageStorage.ImageAdded -= imageStorage_ImageAdded;
        }

        void imageStorage_ImageAdded(string obj)
        {
            ProcessItem(obj);
        }

        private void ProcessItem(string id)
        {
            long itemTime = long.Parse(id.Split('.')[0]);
            long lastTime = 0;
            if (RenderItems.Count > 0)
                lastTime = RenderItems.Last().Ticks;
            long diffTime = itemTime - lastTime;
            if (diffTime < frameTime)
            {
                lock(DeleteItems)
                {
                    DeleteItems.Add(id);
                }
                return;
            }
            long showTime = diffTime;
            if (!useSourceFPS)
                showTime = frameTime;
            ImageItem item = new ImageItem();
            item.Name = id;
            item.Ticks = itemTime;
            item.ShowTime = showTime;
            lock (DeleteItems)
            {
                RenderItems.Add(item);
            }
        }
    }
}
