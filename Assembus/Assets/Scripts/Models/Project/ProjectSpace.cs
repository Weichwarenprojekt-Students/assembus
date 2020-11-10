using System.Xml.Serialization;
using UnityEngine;

namespace Models
{
    /// <summary>
    ///     This class holds all project configuration data including the 3D model
    /// </summary>
    public class ProjectSpace
    {
        /// <summary>
        ///     The project's name
        /// </summary>
        public string Name;

        /// <summary>
        ///     The name of the object file
        /// </summary>
        public string ObjectFile;

        /// <summary>
        ///     The actual GameObject which stores the imported OBJ file with the
        ///     correct hierarchy and additional GameObject parameters
        ///     which are loaded from the XMl config file.
        ///     This model will be serialized to a separate file
        /// </summary>
        [XmlIgnoreAttribute] public GameObject ObjectModel;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="name">The project's name</param>
        /// <param name="objectFile">The name of the project's object file</param>
        public ProjectSpace(string name, string objectFile)
        {
            Name = name;
            ObjectFile = objectFile;
        }

        /// <summary>
        ///     Default constructor for XmlSerializer
        /// </summary>
        public ProjectSpace()
        {
        }
    }
}