using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using VitML.ImageRenderer.Models;

namespace VitML.ImageRenderer.Loaders
{

    public interface IDynamicSerializable
    {
        void OnSerialize(SerializationInfo info);
        void OnDeserialize(SerializationInfo info);
    }

    public class SerializationInfo
    {

        private string name;
        private Dictionary<string, object> dict = new Dictionary<string, object>();

        public SerializationInfo(string name)
        {
            this.name = name;
        }

        private SerializationInfo(XElement el)
        {
            Deserialize(el);
        }

        public static SerializationInfo Parse(XElement el)
        {
            SerializationInfo info = new SerializationInfo(el);
            return info;
        }

        public void Add(string name, object obj)
        {
            dict.Add(name, obj);
        }

        public string Get(string name)
        {
            return dict[name].ToString();
        }

        private void Deserialize(XElement element)
        {
            foreach (XElement el in element.Elements())
                dict.Add(el.Name.ToString(), el.Value);
        }

        public string Serialize()
        {
            XElement element = new XElement(name);
            foreach (var key in dict.Keys)
            {
                object value = dict[key];
                element.Add(new XElement(key, value));
            }
            return element.ToString();
        }
    }

    public interface IConfigLoader
    {

        event Action SourceChanged;

        T Load<T>();
        void Save(object config);
    }
}
