using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Xml.Linq;

namespace VitML.ImageRenderer.Loaders
{
    public class XMLConfigLoader : IConfigLoader
    {
        private string filePath;
        private FileSystemWatcher watcher;

        public event Action SourceChanged;

        public XMLConfigLoader(string configFile)
        {
            filePath = configFile;
            try
            {
                string dir = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileName(filePath);
                watcher = new FileSystemWatcher(dir);
                watcher.Filter = fileName;
                watcher.EnableRaisingEvents = true;
                watcher.Changed += watcher_Changed;
            }
            catch
            {
                //
            }
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (SourceChanged != null)
                SourceChanged();
        }

        public T Load<T>()
        {
            ConstructorInfo ci = typeof(T).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                    null, new Type[0], null);
            T ins = (T)ci.Invoke(new object[0]);
            string content = "";
            try
            {
                content = File.ReadAllText(filePath, Encoding.Default);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            if (!string.IsNullOrEmpty(content))
            {
                XElement xEl = XElement.Parse(content);
                SerializationInfo info = SerializationInfo.Parse(xEl);
                if (ins is IDynamicSerializable)
                    (ins as IDynamicSerializable).OnDeserialize(info);
            }
            return ins;
        }

        public void Save(object ins)
        {
            SerializationInfo info = new SerializationInfo(ins.GetType().Name.ToString());
            if (ins is IDynamicSerializable)
                (ins as IDynamicSerializable).OnSerialize(info);
            string content = info.Serialize();
            try
            {
                File.WriteAllText(filePath, content, Encoding.Default);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
