using System;
using System.IO;
using Models.AppConfiguration;
using UnityEngine;

namespace Services.Serialization
{
    public class ConfigurationManager
    {
        /// <summary>
        ///     Config file name
        /// </summary>
        private const string FileName = "assembus.xml";

        /// <summary>
        ///     The XML writer/reader instance
        /// </summary>
        private readonly XmlDeSerializer<Configuration> _xmlDeSerializer = new XmlDeSerializer<Configuration>();

        /// <summary>
        ///     ConfigFileStruct instance which stores the content of the XML file
        /// </summary>
        public Configuration Config = new Configuration();


        /// <summary>
        ///     Private constructor to comply with the singleton pattern
        /// </summary>
        private ConfigurationManager()
        {
            LoadConfig();
        }

        /// <summary>
        ///     ConfigurationManager singleton
        /// </summary>
        public static ConfigurationManager Instance { get; } = new ConfigurationManager();

        /// <summary>
        ///     Loads ConfigFileStruct FileStruct from XML file.
        ///     If reading fails, return false and write to debug log
        /// </summary>
        private void LoadConfig()
        {
            if (!File.Exists(FileName)) Debug.Log("No XML config file existing!");

            try
            {
                // Read object from XML
                Config = _xmlDeSerializer.DeserializeData(FileName);
            }
            catch (Exception e)
            {
                Debug.Log("Loading XML config file failed: " + e.Message);
            }
        }

        /// <summary>
        ///     Writes ConfigFileStruct FileStruct to XML file.
        ///     If writing fails, return false and write to debug log
        /// </summary>
        /// <returns>True if the saving was successful</returns>
        public bool SaveConfig()
        {
            try
            {
                _xmlDeSerializer.SerializeData(FileName, Config);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}