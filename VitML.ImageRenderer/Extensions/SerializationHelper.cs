using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace VitML.ImageRenderer.Extensions
{
    public static class SerializationHelper
    {
        public static string SerializeObject<T>(T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        public static T DeserializeObject<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            return (T)serializer.Deserialize(memStream);
        }
    }
}
