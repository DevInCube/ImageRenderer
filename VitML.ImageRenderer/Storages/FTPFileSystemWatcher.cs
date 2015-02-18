using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace VitML.ImageRenderer.Storages
{
    public class FTPFileSystemWatcher
    {

        private FTPStorage storage;

        public event FileSystemEventHandler FTPFileCreated;
        public event FileSystemEventHandler FTPFileDeleted;
        public event FileSystemEventHandler FTPFileChanged;

        private readonly object locker = new object();
        private HashSet<string> files = new HashSet<string>();

        private BackgroundWorker watcher;

        public FTPFileSystemWatcher(FTPStorage storage)
        {
            this.storage = storage;
            watcher = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            watcher.DoWork += watcher_DoWork;
            watcher.ProgressChanged += watcher_ProgressChanged;
        }

        void watcher_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string name = (string)e.UserState;
            if (this.FTPFileCreated != null)
                FTPFileCreated.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Created, storage.Directory, name));
        }

        void watcher_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending)
            {
                lock(locker)
                {
                    IEnumerable<string> items = storage.GetImages();
                    foreach(var file in items)
                        if(!files.Contains(file))
                        {
                            files.Add(file);
                            worker.ReportProgress(0, file);
                        }
                    files.RemoveWhere((item)=>!items.Contains(item));
                }
            }
        }

        public void Start()
        {
            files.Clear();
            IEnumerable<string> items = storage.GetImages();
            foreach (var file in items)
                files.Add(file);
            watcher.RunWorkerAsync();
        }

        public void Stop()
        {
            watcher.CancelAsync();
        }

       
    }

}
