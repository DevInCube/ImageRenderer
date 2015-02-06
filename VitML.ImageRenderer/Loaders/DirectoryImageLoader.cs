using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VitML.ImageRenderer.Extensions;
using VitML.ImageRenderer.Models;

namespace VitML.ImageRenderer.Loaders
{
    public class DirectoryImageLoader : IImageLoader
    {

        public class FileWrap : IComparable<FileWrap>
        {

            public FileInfo FileInfo { get; private set; }

            public long Id { get; private set; }
            public bool IsLoaded { get; private set; }
            public bool IsLocked { get; set; }

            public event Action<FileWrap> Loaded;

            public FileWrap(FileInfo fi)
            {
                this.FileInfo = fi;
                Id = long.Parse(fi.Name.Split('_')[0]);
            }

            public void WaitLoaded()
            {
                while (IOHelper.IsFileLocked(this.FileInfo)) ;
                IsLoaded = true;
            }

            public int CompareTo(FileWrap other)
            {
                if (other == null) return 1;
                return this.Id.CompareTo(other.Id);
            }
        }

        private RendererConfig config;
        private string directoryPath;
        private FileSystemWatcher watcher;
        private List<FileWrap> imgFiles = new List<FileWrap>();
        private FileWrap currentImageFile = null;
        private object imgLoadLock = new object();
        private System.Timers.Timer timer;

        public event Action<BitmapImage> ImageLoaded;

        void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            FileWrap fw = null;
            lock (imgFiles)
            {
                foreach (var f in imgFiles)
                    if (f.FileInfo.FullName.Equals(e.FullPath))
                    {
                        fw = f;
                        break;
                    }
                imgFiles.Remove(fw);
            }
        }

        void watcher_Created(object sender, FileSystemEventArgs e)
        {
            FileInfo fi = new FileInfo(e.FullPath);
            FileWrap fw = new FileWrap(fi);
            lock (imgFiles)
            {
                imgFiles.Add(fw);
                imgFiles.Sort();
            }
            fw.WaitLoaded();
            if (config.UpdateFrequency == 0)
                LoadLastImageAnddeleteUnused();
        }

        FileWrap GetLastLoaded()
        {
            lock (imgFiles)
            {
                for (int i = imgFiles.Count - 1; i >= 0; i--)
                {
                    var ffw = imgFiles[i];
                    if (ffw.IsLoaded)
                        return ffw;
                }
            }
            return null;
        }

        void DeleteUnusedImages()
        {
            if (currentImageFile == null) return;
            lock (imgFiles)
            {
                if (config.DeleteImages)
                {
                    foreach (FileWrap f in imgFiles)
                    {
                        if (f.Id >= currentImageFile.Id) break;
                        if (f.IsLoaded)
                        {
                            try
                            {
                                f.FileInfo.Delete();
                            }
                            catch
                            {
                                MessageBox.Show("Cannot delete file");
                            }
                        }
                    }
                }
                else
                {
                    imgFiles.RemoveRange(0, imgFiles.IndexOf(currentImageFile));
                }
            }
        }

        private void LoadImage(FileInfo fi)
        {
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(fi.FullName, UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            src.Freeze();
            if (ImageLoaded != null)
                ImageLoaded(src);
        }

        public void Init(RendererConfig config)
        {
            this.config = config;
            this.directoryPath = config.Directory;
            try
            {
                watcher = new FileSystemWatcher(directoryPath);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
            watcher.Filter = "*.bmp";
            watcher.EnableRaisingEvents = true;
            watcher.Created += watcher_Created;
            watcher.Deleted += watcher_Deleted;
            if (config.UpdateFrequency == 0)
                config.UpdateFrequency = 100;
            timer = new System.Timers.Timer(1000/config.UpdateFrequency);
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (config.UpdateFrequency > 0)
            {
                timer.Interval = 1000 / config.UpdateFrequency;
                LoadLastImageAnddeleteUnused();
            }
        }

        void LoadLastImageAnddeleteUnused()
        {
            FileWrap fw = GetLastLoaded();
            if (fw != null && fw != currentImageFile)
            {
                currentImageFile = fw;
                lock (imgFiles)
                {
                    try
                    {
                        LoadImage(fw.FileInfo);
                    }
                    catch(Exception e)
                    {
                        MessageBox.Show("Cannot load file: " + e.ToString());
                    }
                }
                try
                {
                    GC.Collect();
                }
                catch
                {
                    MessageBox.Show("GC Error");
                }
            }
            try
            {
                DeleteUnusedImages();
            }
            catch
            {
                MessageBox.Show("delete Images Error");
            }
        }


        public void Close()
        {
            //throw new NotImplementedException();
        }
    }
}
