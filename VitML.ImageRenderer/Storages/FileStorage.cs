using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using VitML.ImageRenderer.Core;
using VitML.ImageRenderer.Extensions;

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

        public override ImageItem Load(string id)
        {
            ImageItem item = new ImageItem();
            try
            {
                string fullPath = Path.Combine(this.watcher.Path, id);
                while (IOHelper.IsFileLocked(fullPath)) ;
                item.ImageData = File.ReadAllBytes(fullPath);
            }
            catch (IOException)
            {
                //
            }
            return item;
        }

        public override bool Save(string id, ImageItem image)
        {
            return Save(id, image.ImageData);
        }

        public override bool Save(string id, byte[] data)
        {
            bool saved = false;
            try
            {
                string path = Path.Combine(this.watcher.Path, id);
                File.WriteAllBytes(path, data);
                saved = true;
            }
            catch (Exception)
            {
                //
            }
            return saved;
        }

        public override bool Remove(string id)
        {
            string fullPath = Path.Combine(this.watcher.Path, id);
            while (IOHelper.IsFileLocked(fullPath)) ;
            File.Delete(fullPath);
            return !File.Exists(fullPath);
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
