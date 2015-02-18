using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

        private string _FPS;
        private BitmapImage _Image;

        private ImageStorage imageStorage;
        private RendererConfig config;

        private Queue<ImageItem> loadItems = new Queue<ImageItem>();
        private Queue<ImageItem> renderItems = new Queue<ImageItem>();
        private Queue<ImageItem> removeItems = new Queue<ImageItem>();
        private List<long> timeItems = new List<long>();

        private readonly object loadLock = new object();
        private readonly object renderLock = new object();
        private readonly object removeLock = new object();
        private readonly object timeLock = new object();

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
        public string FPS
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
            int avg = (int)e.UserState;
            this.FPS = avg.ToString();
        }

        void fpsUpdater_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending)
            {
                int avg = 0;
                if (useSourceFPS)
                {
                    Thread.Sleep(500);
                }
                lock (timeLock)
                {
                    if (!useSourceFPS)
                    {
                        while (timeItems.Count < config.UpdateFrequency)
                        {
                            Monitor.Wait(timeLock);
                        }
                    }
                    if (timeItems.Count > 0)
                    {
                        avg = (int)(1000 / (Math.Ceiling(timeItems.Average()) / TimeSpan.TicksPerMillisecond));
                        timeItems.Clear();
                    }
                }
                worker.ReportProgress(0, avg);
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
                sw.Stop();
                item.ShowTime -= sw.ElapsedMilliseconds;
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
                item.Image = imageStorage.Load(item.Name);
                sw.Stop();
                item.ShowTime -= sw.ElapsedMilliseconds;
                worker.ReportProgress(0, item);
            }     
        }

        public void Setup(RendererConfig config)
        {
            this.config = config;
            imageStorage = new FileStorage(config.Directory);
            useSourceFPS = (config.UpdateFrequency <= 0);
            //frameTime =  //@todo
            //useSourceFPS = //@todo 
        }

        public void Start()
        {
            imageLoader.RunWorkerAsync();
            imageRenderer.RunWorkerAsync();
            imageRemover.RunWorkerAsync();
            fpsUpdater.RunWorkerAsync();
            gcCleaner.RunWorkerAsync();

            imageStorage.Connect();
            IEnumerable<string> items = imageStorage.GetImages();
            foreach (var item in items)
            {
                if (config.Cleanup)
                    Remove(item);
                else
                    ProcessItem(item);
            }
            imageStorage.ImageAdded += imageStorage_ImageAdded;
        }

        public void Stop()
        {
            imageLoader.CancelAsync();
            imageRenderer.CancelAsync();
            imageRemover.CancelAsync();
            fpsUpdater.CancelAsync();
            gcCleaner.CancelAsync();

            imageStorage.Disconnect();
            imageStorage.ImageAdded -= imageStorage_ImageAdded;
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
            if (!config.DeleteImages) return;
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
            long itemTime = long.Parse(id.Split('.')[0]);
            long lastProcessTime = ((this.lastTime > 0) ? this.lastTime : (itemTime - 1));
            long diffTime = itemTime - lastProcessTime;
            if (!useSourceFPS)
            {
                if (diffTime < frameTime)
                {
                    Remove(id);
                    return;
                }
            }
            long showTime = diffTime;
            if (!useSourceFPS)
                showTime = frameTime;
            ImageItem item = new ImageItem();
            item.Name = id;
            item.Ticks = itemTime;
            item.ShowTime = showTime;
            this.lastTime = itemTime;
            Load(item);
        }
    }
}
