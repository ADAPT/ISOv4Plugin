using System;
using System.IO;

namespace AgGateway.ADAPT.ISOv4Plugin
{
    public interface ISerializer
    {
        T Deserialize<T>(String text);
    }

    public class XmlSerializer : ISerializer
    {
        public T Deserialize<T>(String text)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(new StringReader(text));
        }
    }
}