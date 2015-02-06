using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using VitML.ImageRenderer.Models;

namespace VitML.ImageRenderer.Loaders
{

    public class JSONConfigLoader : IConfigLoader
    {

        private string filePath;

        public JSONConfigLoader(string configFile)
        {
            filePath = configFile;
        }

        public T Load<T>()
        {
            throw new NotImplementedException();
            /*
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
                dynamic info = JObject.Parse(content);
                if (ins is IDynamicSerializable)
                    (ins as IDynamicSerializable).OnDeserialize(info);
            }
            return ins;
             */
        }

        public void Save(object ins)
        {
            throw new NotImplementedException();
            /*
            string content = JObject.FromObject(ins).ToString();
            try
            {
                File.WriteAllText(filePath, content, Encoding.Default);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }*/
        }



        public event Action SourceChanged;
    }
    
}
