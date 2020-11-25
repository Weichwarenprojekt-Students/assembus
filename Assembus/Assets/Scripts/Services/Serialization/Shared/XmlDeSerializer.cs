using System.IO;
using System.Xml.Serialization;

namespace Services.Serialization.Shared
{
    public class XmlDeSerializer<T>
    {
        /// <summary>
        ///     Write object of type T to XML file
        /// </summary>
        /// <param name="filePath">The path where XML file should be written</param>
        /// <param name="data">Actual data object instance of type T</param>
        public void SerializeData(string filePath, T data)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));

            using (var tw = new StreamWriter(filePath))
            {
                xmlSerializer.Serialize(tw, data);
            }
        }

        /// <summary>
        ///     Loads and returns data from XML file
        /// </summary>
        /// <param name="filePath">The path where the XML file resides</param>
        /// <returns>Data object of type T</returns>
        public T DeserializeData(string filePath)
        {
            var xmlDeSerializer = new XmlSerializer(typeof(T));

            using (var tr = new StreamReader(filePath))
            {
                return (T) xmlDeSerializer.Deserialize(tr);
            }
        }
    }
}