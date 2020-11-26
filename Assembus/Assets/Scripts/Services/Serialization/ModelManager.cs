using System;
using System.Collections.Generic;
using Dummiesman;
using Models.Project;
using Services.Serialization.Models;
using Services.Serialization.Shared;
using Shared;
using UnityEngine;

namespace Services.Serialization
{
    public static class ModelManager
    {
        /// <summary>
        ///     Constant string which specifies the name of the source/default assembly station
        /// </summary>
        private const string DefaultStationName = "Default Station";

        /// <summary>
        ///     The project manager
        /// </summary>
        private static readonly ProjectManager ProjectManager = ProjectManager.Instance;

        /// <summary>
        ///     The XML-Serializer instance
        /// </summary>
        private static readonly XmlDeSerializer<SerializableItem> XMLDeSerializer
            = new XmlDeSerializer<SerializableItem>();

        /// <summary>
        ///     Responsible for loading an object model into the currently active
        ///     scene
        /// </summary>
        /// <param name="objPath">The import path</param>
        /// <param name="configPath">The path to the model config</param>
        /// <param name="firstTime">True if the model is loaded for the first time</param>
        /// <returns>
        ///     Returns parent of the object model
        /// </returns>
        public static GameObject Load(string objPath, string configPath, bool firstTime)
        {
            // Load the object
            var parent = LoadObjectModel(objPath);

            // Prepare the children
            PrepareChildren(parent);

            // Load the model information
            if (firstTime) InitializeModel(parent);
            else DeserializeGameObject(configPath, parent.transform);

            return parent;
        }

        /// <summary>
        ///     Loads the object model into the scene
        /// </summary>
        /// <returns>Parent element of the loaded object model hierarchy</returns>
        private static GameObject LoadObjectModel(string objPath)
        {
            return new OBJLoader().Load(objPath);
        }

        /// <summary>
        ///     Collects the elements, that the object model contains
        /// </summary>
        /// <returns>List of all game objects inside the object model</returns>
        private static void PrepareChildren(GameObject parent)
        {
            for (var i = 0; i < parent.transform.childCount; i++)
            {
                var child = parent.transform.GetChild(i).gameObject;

                // Add a mesh collider
                child.AddComponent<MeshCollider>();

                // Add the item information
                child.AddComponent<ItemInfoController>();
                child.GetComponent<ItemInfoController>().ItemInfo = new ItemInfo
                {
                    displayName = child.name,
                    isGroup = false,
                    isFused = false
                };
                child.name = ProjectManager.CreateID(i);
            }
        }

        /// <summary>
        ///     Loads GameObject configuration + hierarchy from XMl file and applies changes to passed instance.
        ///     Please make a deep-copy of the passed GameObject if you need an untouched inputData instance.
        /// </summary>
        /// <param name="filePath">The path where the GameObject config is stored as a XML file</param>
        /// <param name="rootObject">The actual GameObject instance which should be returned modified</param>
        private static void DeserializeGameObject(string filePath, Transform rootObject)
        {
            // Read in the data
            var rootItem = XMLDeSerializer.DeserializeData(filePath);

            // Get all the existent items
            var children = Utility.GetAllChildren(rootObject.gameObject);

            // Move the items and fill them with information
            InsertItemData(rootItem, rootObject, children);
        }

        /// <summary>
        ///     Move the items and insert the necessary data
        /// </summary>
        /// <param name="parentItem">The serializable parent item</param>
        /// <param name="parent">The actual parent</param>
        /// <param name="children">All the actual items</param>
        private static void InsertItemData(SerializableItem parentItem, Transform parent, List<GameObject> children)
        {
            foreach (var child in parentItem.children)
            {
                // Try to find the item
                var item = Utility.FindChildLinear(children, child.id);

                // Create the group if it doesn't exist
                if (item == null) item = new GameObject {name = child.id};

                // Add the item info if necessary
                if (item.GetComponent<ItemInfoController>() == null) item.AddComponent<ItemInfoController>();
                item.GetComponent<ItemInfoController>().ItemInfo = child.itemInfo;

                // Move the item
                item.transform.SetParent(parent);

                // Continue with the child
                InsertItemData(child, item.transform, children);
            }
        }

        /// <summary>
        ///     Initialize the model and insert a default station
        /// </summary>
        /// <param name="rootObject">The root object</param>
        private static void InitializeModel(GameObject rootObject)
        {
            // Add the default assembly station
            var defaultStation = new GameObject {name = ProjectManager.GetNextGroupID()};
            var info = new ItemInfo {displayName = DefaultStationName, isGroup = true, isFused = false};
            defaultStation.AddComponent<ItemInfoController>();
            defaultStation.GetComponent<ItemInfoController>().ItemInfo = info;

            // Move all children to the default assembly station
            var children = Utility.GetAllChildren(rootObject);
            foreach (var child in children) child.transform.SetParent(defaultStation.transform);

            // Append the default station to the model
            defaultStation.transform.SetParent(rootObject.transform);
        }

        /// <summary>
        ///     Saves GameObject configuration + hierarchy of the passed GameObject instance to a XML file.
        /// </summary>
        /// <param name="filePath">The path where the GameObject config should be stored as a XML file</param>
        /// <param name="rootObject">The actual GameObject instance whose configuration should be saved</param>
        public static void SerializeModel(string filePath, Transform rootObject)
        {
            // Create the serializable for the item
            var rootItem = new SerializableItem(rootObject.name);

            // Extract the serializable data from the root model
            ExtractItemData(rootItem, rootObject, true);

            // Write ModelComponent-List to XML file
            XMLDeSerializer.SerializeData(filePath, rootItem);
        }

        /// <summary>
        ///     Extract the file data from the given object
        /// </summary>
        /// <param name="parentItem">The serializable parent item</param>
        /// <param name="parent">The actual parent object</param>
        /// <param name="topLevel">True if the method is called for the top level of the model</param>
        private static void ExtractItemData(SerializableItem parentItem, Transform parent, bool topLevel)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                var childItem = new SerializableItem(child);

                if (topLevel && !childItem.itemInfo.isGroup)
                    throw new ToplevelComponentException {Component = childItem.itemInfo.displayName};

                ExtractItemData(childItem, child, false);
                parentItem.children.Add(childItem);
            }
        }

        /// <summary>
        ///     Exception for when a single, non-group component is on the top level of the model
        /// </summary>
        public class ToplevelComponentException : Exception
        {
            /// <summary>
            ///     The name of the component
            /// </summary>
            public string Component;
        }
    }
}