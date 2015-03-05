using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using VitML.ImageRenderer.Extensions;
using VitML.ImageRenderer.Models;
using VitML.ImageRenderer.Storages;
using VitML.ImageRenderer.ViewModels;

namespace VitML.ImageRenderer.Core
{
    public class ImagePlayer : ObservableObject
    {

        Logger logger = LogManager.GetLogger("ImagePlayer");

        private bool useSourceFPS = false;
        private long frameTime = 1;
        private long lastTime = 0;
        private static long lastImageUpdatedTime = 0;

        private int _FPS;
        private BitmapImage _Image;

        private ImageStorage imageStorage;
        private ImageStorage compressStorage;
        private PlayerConfiguration config;

        private Queue<ImageItem> loadItems = new Queue<ImageItem>();
        private Queue<ImageItem> convertItems = new Queue<ImageItem>();
        private Queue<ImageItem> renderItems = new Queue<ImageItem>();
        private Queue<ImageItem> compressItems = new Queue<ImageItem>();
        private Queue<ImageItem> removeItems = new Queue<ImageItem>();
        private List<long> timeItems = new List<long>();

        private readonly object loadLock = new object();
        private readonly object renderLock = new object();
        private readonly object convertLock = new object();
        private readonly object removeLock = new object();
        private readonly object compressLock = new object();
        private readonly object timeLock = new object();

        private BackgroundWorker imagePuller;
        private BackgroundWorker imageLoader;
        private BackgroundWorker imageConverter;
        private BackgroundWorker imageRenderer;
        private BackgroundWorker imageRemover;
        private BackgroundWorker imageCompressor;
        private BackgroundWorker fpsUpdater;
        private BackgroundWorker gcCleaner;

        private List<BackgroundWorker> workers = new List<BackgroundWorker>();

        public int LoadCount { get { return loadItems.Count; } }
        public int ConvertCount { get { return convertItems.Count; } }
        public int RenderCount { get { return renderItems.Count; } }      

        public BitmapImage Image
        {
            get { return _Image; }
            private set
            {
                _Image = value;
                OnPropertyChanged("Image");
            }
        }
        public int FPS
        {
            get { return _FPS; }
            private set
            {
                _FPS = value;
                OnPropertyChanged("FPS");
            }
        }

        public ImagePlayer()
        {
            imagePuller = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            imagePuller.DoWork += imagePuller_DoWork;
            imagePuller.ProgressChanged += imagePuller_ProgressChanged;

            imageLoader = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            imageLoader.DoWork += imageLoader_DoWork;
            imageLoader.ProgressChanged += imageLoader_ProgressChanged;

            imageConverter = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            imageConverter.DoWork += imageConverter_DoWork;
            imageConverter.ProgressChanged += imageConverter_ProgressChanged;

            imageRenderer = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            imageRenderer.DoWork += imageRenderer_DoWork;
            imageRenderer.ProgressChanged += imageRenderer_ProgressChanged;

            imageRemover = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            imageRemover.DoWork += imageRemover_DoWork;

            imageCompressor = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            imageCompressor.DoWork += imageCompressor_DoWork;

            fpsUpdater = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            fpsUpdater.DoWork += fpsUpdater_DoWork;
            fpsUpdater.ProgressChanged += fpsUpdater_ProgressChanged;

            gcCleaner = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            gcCleaner.DoWork += gcCleaner_DoWork;

            workers.Add(imageLoader);
            workers.Add(imageConverter);
            workers.Add(imageRenderer);
            workers.Add(imageRemover);
            workers.Add(fpsUpdater);
            workers.Add(gcCleaner);
        }

        public void Setup(PlayerConfiguration config)
        {
            this.config = config;
            if (config.Source.DoPull)
                workers.Add(imagePuller);
            if (config.Compress.IsEnabled)
            {
                workers.Add(imageCompressor);
                compressStorage = CreateStorage(config.Compress.Storage);
            }

            imageStorage = CreateStorage(config.Source.Storage);

            useSourceFPS = (config.Render.FPS <= 0);
            if (!useSourceFPS)
                frameTime = (int)Math.Floor(1000 / (double)config.Render.FPS * TimeSpan.TicksPerMillisecond);
        }        

        void imagePuller_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ImageItem item = (ImageItem)e.UserState;
            ProcessItem(item);
        }

        void imagePuller_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            BackgroundWorker worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending)
            {
                ImageItem item = imageStorage.Load(null);
                if (item.ImageData != null)
                    worker.ReportProgress(0, item);
            }     
        }

        private ImageStorage CreateStorage(StorageConfiguration storage)
        {
            ImageStorage st = null;
            if (storage.Connection is FileConnectionConfiguration)
            {
                var con = storage.Connection as FileConnectionConfiguration;
                st = new FileStorage(Path.Combine(con.Disk, storage.Directory));
            }
            else if (storage.Connection is FilePassiveConnectionConfiguration)
            {
                var con = storage.Connection as FilePassiveConnectionConfiguration;
                st = new FileStoragePassive(con.ImageUri);
            }
            else if (config.Source.Storage.Connection is FtpConnectionConfiguration)
            {
                var con = storage.Connection as FtpConnectionConfiguration;
                st = new FTPStorage(con.Host, storage.Directory, con.UserName, con.Password);
            }
            else if (config.Source.Storage.Connection is HttpConnectionConfiguration)
            {
                var con = storage.Connection as HttpConnectionConfiguration;
                st = new HTTPStorage(con.ImageUri, con.TimeUri);
            }
            else if (config.Source.Storage.Connection is HttpConnection2Configuration)
            {
                var con = storage.Connection as HttpConnection2Configuration;
                st = new HTTPStorage2(con.ImageUri, con.TestUri);
            }
            else if (config.Source.Storage.Connection is HttpConnection3RingConfiguration)
            {
                var con = storage.Connection as HttpConnection3RingConfiguration;
                st = new HTTPStorage3Ring(con.DirUri, con.TimeUri);
            }
            return st;
        }

        void gcCleaner_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending)
            {
                Thread.Sleep(10000);
                GC.Collect();
            }
        }

        void imageRemover_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending)
            {
                lock (removeLock)
                {
                    while (removeItems.Count == 0)
                    {
                        Monitor.Wait(removeLock);
                    }
                    if(imageStorage.Remove(removeItems.Peek().Name))
                        removeItems.Dequeue();
                }
            } 
        }

        void imageCompressor_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending)
            {
                ImageItem item = null;
                lock (compressLock)
                {
                    while (compressItems.Count == 0)
                    {
                        Monitor.Wait(compressLock);
                    }
                    item = compressItems.Dequeue();
                }
                compressStorage.Save(item.Time + ".jpg", item);
            }
        }

        void fpsUpdater_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.FPS = (int)e.UserState;
        }

        void fpsUpdater_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending)
            {
                int fps = 0;
                double avg = 0D;
                if (useSourceFPS)
                {
                    Thread.Sleep(2000);
                }
                lock (timeLock)
                {
                    if (!useSourceFPS)
                    {
                        while (timeItems.Count < config.Render.FPS)
                            Monitor.Wait(timeLock);
                    }
                    if (timeItems.Count > 0)
                    {
                        avg = timeItems.Average();
                        timeItems.Clear();
                    }
                }
                fps = (avg == 0) ? 0 : (int)(1000 / (Math.Ceiling(avg) / TimeSpan.TicksPerMillisecond));
                worker.ReportProgress(0, fps);
            }
        }

        void imageConverter_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ImageItem item = e.UserState as ImageItem;
            Render(item);
        }

        void imageConverter_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;

            while (!worker.CancellationPending)
            {                
                ImageItem item = GetConvertItem();
                item.Convert();
                worker.ReportProgress(0, item);
            }
        }


        void imageRenderer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            AddTime(DateTime.Now.Ticks);
            BitmapImage image = e.UserState as BitmapImage;
            this.Image = image;
        }

        void imageRenderer_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            BackgroundWorker worker = (BackgroundWorker)sender;

            //throw new Exception();//@test

            while (!worker.CancellationPending)
            {
                ImageItem item;
                sw.Reset();
                sw.Start();
                item = GetRenderItem();
                worker.ReportProgress(0, item.Image);
                if (config.Render.DeleteImages)
                    Remove(item.Name);
                if (config.Compress.IsEnabled)
                    Compress(item);
                item.ShowTime -= sw.ElapsedTicks;
                long showTime = item.ShowTime;
                if (showTime > 100 * TimeSpan.TicksPerMillisecond)
                    showTime = 100 * TimeSpan.TicksPerMillisecond;
                
                int delayTime = (int)Math.Floor(showTime / (double)TimeSpan.TicksPerMillisecond);
                if (delayTime > 0)
                    Thread.Sleep(delayTime);

                //Thread.Sleep(500);//@test
            }
        }


        void imageLoader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var item = e.UserState as ImageItem;
            this.Convert(item);
        }

        void imageLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            BackgroundWorker worker = (BackgroundWorker)sender;

            while (!worker.CancellationPending)
            {
                ImageItem item;
                sw.Reset();
                sw.Start();
                item = GetLoadItem();
                lock (renderLock)
                {
                    while (renderItems.Count > 60)
                    {
                        Monitor.Wait(renderLock);
                    }
                }
                ImageItem ii = imageStorage.Load(item.Name);
                item.ImageData = ii.ImageData;
                sw.Stop();
                worker.ReportProgress(0, item);
            }     
        }

        public void Start()
        {
            workers.ForEach(w => w.RunWorkerAsync());

            imageStorage.Connect();
            IEnumerable<string> items = imageStorage.GetImages();
            foreach (var item in items)
            {
                if (config.Render.CleanUp)
                    Remove(item);
                else
                    ProcessItem(item);
            }
            imageStorage.ImageAdded += imageStorage_ImageAdded;
           
        }

        public void Stop()
        {
            workers.ForEach(w => w.CancelAsync());

            imageStorage.Disconnect();
            imageStorage.ImageAdded -= imageStorage_ImageAdded;

        }

        protected void Load(ImageItem item)
        {
            lock (loadLock)
            {
                loadItems.Enqueue(item);
                OnPropertyChanged("LoadCount");

                Monitor.PulseAll(loadLock);
            }
        }

        private ImageItem GetLoadItem()
        {
            ImageItem item = null;
            lock (loadLock)
            {
                while (loadItems.Count == 0)
                {
                    Monitor.Wait(loadLock);
                }
                item = loadItems.Dequeue();
            }
            return item;
        }

        protected void Convert(ImageItem item)
        {
            lock (convertLock)
            {

                convertItems.Enqueue(item);
                OnPropertyChanged("ConvertCount");

                Monitor.PulseAll(convertLock);
            }
        }

        private ImageItem GetConvertItem()
        {
            ImageItem item = null;
            lock (convertLock)
            {
                while (convertItems.Count == 0)
                {
                    Monitor.Wait(convertLock);
                }
                item = convertItems.Dequeue();
                Monitor.PulseAll(convertLock);
            }
            return item;
        }

        protected void Render(ImageItem item)
        {
            lock (renderLock)
            {

                renderItems.Enqueue(item);
                OnPropertyChanged("RenderCount");

                Monitor.PulseAll(renderLock);
            }
        }

        protected void Compress(ImageItem item)
        {
            lock (compressLock)
            {
                compressItems.Enqueue(item);
                Monitor.PulseAll(compressLock);
            }
        }

        private ImageItem GetRenderItem()
        {
            ImageItem item = null;
            lock (renderLock)
            {
                while (renderItems.Count == 0)
                {
                    Monitor.Wait(renderLock);
                }
                item = renderItems.Dequeue();
                Monitor.PulseAll(renderLock);
            }
            return item;
        }

        protected void Remove(string name)
        {
            lock (removeLock)
            {
                ImageItem item = new ImageItem() { Name = name };
                removeItems.Enqueue(item);
                Monitor.PulseAll(removeLock);
            }
        }

        protected void AddTime(long time)
        {
            if (time < 0) time = 0;
            lock (timeLock)
            {
                long diff = time - lastImageUpdatedTime;
                lastImageUpdatedTime = time;
                if (diff > 0)
                    timeItems.Add(diff);
                Monitor.PulseAll(timeLock);
            }
        }

        void imageStorage_ImageAdded(string obj)
        {
            ProcessItem(obj);
        }

        private void ProcessItem(string id)
        {
            long itemTime = 0;
            bool res = long.TryParse(id.Split('.')[0], out itemTime);
            if (!res || itemTime <= 0)
                return;
            long lastProcessTime = ((this.lastTime > 0) ? this.lastTime : (itemTime - frameTime - 1));
            long diffTime = itemTime - lastProcessTime;
            if (!useSourceFPS)
            {
                if (diffTime < frameTime)
                {
                    if (config.Render.DeleteImages)
                        Remove(id);
                    return;
                }
            }
            long showTime = diffTime;
            if (!useSourceFPS)
                showTime = frameTime;
            ImageItem item = new ImageItem();
            item.Name = id;
            item.Time = itemTime;
            item.ShowTime = showTime;
            this.lastTime = itemTime;
            Load(item);
        }

        public void ProcessItem(ImageItem item)
        {
            long lastProcessTime = ((this.lastTime > 0) ? this.lastTime : (item.Time - frameTime - 1));
            long diffTime = item.Time - lastProcessTime;
            long showTime = diffTime;
            if (!useSourceFPS)
                showTime = frameTime;
            item.ShowTime = showTime;
            this.lastTime = item.Time;
            Convert(item);
        }
    }
}
