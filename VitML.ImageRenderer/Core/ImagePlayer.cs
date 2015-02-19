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
using VitML.ImageRenderer.Models;
using VitML.ImageRenderer.Storages;
using VitML.ImageRenderer.ViewModels;

namespace VitML.ImageRenderer.Core
{
    public class ImagePlayer : ObservableObject
    {

        private bool useSourceFPS = false;
        private long frameTime = 1;
        private long lastTime = 0;

        private int _FPS;
        private BitmapImage _Image;

        private ImageStorage signalStorage;
        private ImageStorage imageStorage;
        private PlayerConfiguration config;

        private Queue<ImageItem> loadItems = new Queue<ImageItem>();
        private Queue<ImageItem> renderItems = new Queue<ImageItem>();
        private Queue<ImageItem> removeItems = new Queue<ImageItem>();
        private List<long> timeItems = new List<long>();

        private readonly object loadLock = new object();
        private readonly object renderLock = new object();
        private readonly object removeLock = new object();
        private readonly object timeLock = new object();

        private BackgroundWorker imagePuller;
        private BackgroundWorker imageLoader;
        private BackgroundWorker imageRenderer;
        private BackgroundWorker imageRemover;
        private BackgroundWorker fpsUpdater;
        private BackgroundWorker gcCleaner;

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
        }

        public void Setup(PlayerConfiguration config)
        {
            this.config = config;
            imageStorage = CreateStorage(config.Source.Storage);
            signalStorage = CreateStorage(config.Signal.Storage);

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
                ImageItem item;
                sw.Reset();
                sw.Start();
                item = imageStorage.Load(null);
                if (item.Image != null)
                    worker.ReportProgress(0, item);
                sw.Stop();
                int fps = (int)((this.useSourceFPS) ? 25 : config.Render.FPS);
                int delay = (int)(1000 / (double)fps - sw.ElapsedMilliseconds);
                if (delay > 1000) delay = 1000;
                if (delay > 0)
                    Thread.Sleep(delay);
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
                if (useSourceFPS)
                {
                    Thread.Sleep(500);
                }
                lock (timeLock)
                {
                    if (!useSourceFPS)
                    {
                        while (timeItems.Count < config.Render.FPS)
                        {
                            Monitor.Wait(timeLock);
                        }
                    }
                    if (timeItems.Count > 0)
                    {
                        double avg = timeItems.Average();
                        fps = (avg == 0) ? 0 : (int)(1000 / (Math.Ceiling(avg) / TimeSpan.TicksPerMillisecond));
                        timeItems.Clear();
                    }
                }
                worker.ReportProgress(0, fps);
            }
        }

        void imageRenderer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BitmapImage image = e.UserState as BitmapImage;
            this.Image = image;
        }

        void imageRenderer_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            BackgroundWorker worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending)
            {
                ImageItem item;
                sw.Reset();
                sw.Start();
                lock (renderLock)
                {
                    while (renderItems.Count == 0)
                    {
                        Monitor.Wait(renderLock);
                    }
                    item = renderItems.Dequeue();
                    Monitor.PulseAll(renderLock);
                }
                worker.ReportProgress(0, item.Image);
                if (config.Render.DeleteImages)
                    Remove(item.Name);
                sw.Stop();
                item.ShowTime -= sw.ElapsedTicks;
                long showTime = item.ShowTime;
                if (showTime > 100 * TimeSpan.TicksPerMillisecond)
                    showTime = 100 * TimeSpan.TicksPerMillisecond;
                AddTime(showTime);
                int delayTime = (int)Math.Floor(showTime / (double)TimeSpan.TicksPerMillisecond);
                if (delayTime > 0)
                    Thread.Sleep(delayTime);
            }
        }

        void imageLoader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var item = e.UserState as ImageItem;
            this.Render(item);
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
                lock (loadLock)
                {
                        while (loadItems.Count == 0)
                        {
                            Monitor.Wait(loadLock);
                        }
                    item = loadItems.Dequeue();
                }
                lock (renderLock)
                {
                    while (renderItems.Count > 60)
                    {
                        Monitor.Wait(renderLock);
                    }
                }
                ImageItem ii = imageStorage.Load(item.Name);
                item.Image = ii.Image;
                sw.Stop();
                item.ShowTime -= sw.ElapsedTicks;
                worker.ReportProgress(0, item);
            }     
        }

        public void Start()
        {
            if (config.Source.DoPull)
                imagePuller.RunWorkerAsync();
            imageLoader.RunWorkerAsync();
            imageRenderer.RunWorkerAsync();
            imageRemover.RunWorkerAsync();
            fpsUpdater.RunWorkerAsync();
            gcCleaner.RunWorkerAsync();

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
            signalStorage.Connect();
            signalStorage.Save(config.Signal.FileName, new byte[0]);
        }

        public void Stop()
        {
            if(config.Source.DoPull)
                imagePuller.CancelAsync();
            imageLoader.CancelAsync();
            imageRenderer.CancelAsync();
            imageRemover.CancelAsync();
            fpsUpdater.CancelAsync();
            gcCleaner.CancelAsync();

            imageStorage.Disconnect();
            imageStorage.ImageAdded -= imageStorage_ImageAdded;

            signalStorage.Remove(config.Signal.FileName);
            signalStorage.Disconnect();
        }

        protected void Load(ImageItem item)
        {
            lock (loadLock)
            {
                loadItems.Enqueue(item);
                Monitor.PulseAll(loadLock);
            }
        }

        protected void Render(ImageItem item)
        {
            lock (renderLock)
            {
                renderItems.Enqueue(item);
                Monitor.PulseAll(renderLock);
            }
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
                timeItems.Add(time);
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
            Render(item);
        }
    }
}
