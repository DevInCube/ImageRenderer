using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using VitML.ImageRenderer.Core;

namespace VitML.ImageRenderer.Storages
{
    public class FileStorage : ImageStorage
    {

        private FileSystemWatcher watcher;

        public FileStorage(string dirName)
        {
            watcher = new FileSystemWatcher(dirName);
            watcher.Created += watcher_Created;
        }

        void watcher_Created(object sender, FileSystemEventArgs e)
        {
            NotifyAdded(e.Name);
        }


        public override void Connect()
        {
            watcher.EnableRaisingEvents = true;
        }

        public override void Disconnect()
        {
            watcher.EnableRaisingEvents = false;
        }

        public override bool IsAvailable(string id)
        {
            return true;
        }

        public override BitmapImage Load(string id)
        {
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(Path.Combine(this.watcher.Path, id), UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            src.Freeze();
            return src;
        }

        public override bool Save(string id, System.Windows.Media.Imaging.BitmapImage image)
        {
            return true;// throw new NotImplementedException();
        }

        public override bool Remove(string id)
        {
            return true;//throw new NotImplementedException();
        }

        public override IEnumerable<string> GetImages()
        {
            string[] paths = Directory.GetFiles(this.watcher.Path);
            string[] names = new string[paths.Length];
            int i = 0;
            foreach(var path in paths)
                names[i++] = Path.GetFileName(path);
            return names;
        }
    }
}
