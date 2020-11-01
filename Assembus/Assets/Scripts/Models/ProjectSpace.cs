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
        ///     correct hierarchy which is loaded from the XMl config file.
        ///     This model will be serialized to a separate file
        /// </summary>
        [XmlIgnoreAttribute] public GameObject ObjectModel;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="name">The project's name</param>
        /// <param name="objectFile">The name of the project's object file</param>
        /// <param name="importModel">The imported GameObject with default hierarchy</param>
        public ProjectSpace(string name, string objectFile, GameObject importModel)
        {
            Name = name;
            ObjectFile = objectFile;
            ObjectModel = importModel;
        }

        /// <summary>
        ///     Default constructor for XmlSerializer
        /// </summary>
        public ProjectSpace()
        {
        }
    }
}