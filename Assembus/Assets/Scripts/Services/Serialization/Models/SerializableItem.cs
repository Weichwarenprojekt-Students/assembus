using System;
using System.Collections.Generic;
using Models.Project;
using UnityEngine;

namespace Services.Serialization.Models
{
    /// <summary>
    ///     This data structure defines the content of the XML file
    ///     and stores all data which is required to save/restore the
    ///     hierarchy and the configuration parameters of the model's GameObjects
    /// </summary>
    [Serializable]
    public class SerializableItem
    {
        /// <summary>
        ///     The fixed id (original name from OBJ file, if not a group) of the GameObject
        /// </summary>
        public string id;

        /// <summary>
        ///     Additional information about the GameObject which cannot be stored in the GameObject directly
        /// </summary>
        public ItemInfo itemInfo;

        /// <summary>
        ///     The children of an item
        /// </summary>
        public List<SerializableItem> children;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="identifier">The identifier</param>
        public SerializableItem(string identifier)
        {
            id = identifier;
            children = new List<SerializableItem>();
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="item">The item to be serialized</param>
        public SerializableItem(Transform item)
        {
            id = item.name;
            itemInfo = item.GetComponent<ItemInfoController>().ItemInfo;
            children = item.childCount > 0 ? new List<SerializableItem>() : null;
        }

        /// <summary>
        ///     Default constructor for XmlSerializer
        /// </summary>
        public SerializableItem()
        {
        }
    }
}