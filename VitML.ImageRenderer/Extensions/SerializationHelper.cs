using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace VitML.ImageRenderer.Extensions
{
    public static class SerializationHelper
    {
        public static string SerializeObject<T>(T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());   
            MemoryStream ms = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            XmlWriter writer = XmlWriter.Create(ms, settings);
            xmlSerializer.Serialize(writer, toSerialize);
            return Encoding.UTF8.GetString(ms.ToArray());           
        }

        public static T DeserializeObject<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            return (T)serializer.Deserialize(memStream);
        }
    }
}
