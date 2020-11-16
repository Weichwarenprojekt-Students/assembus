using System.IO;
using Dummiesman;
using Models.Project;
using UnityEngine;

namespace Services
{
    public static class ObjectLoader
    {
        /// <summary>
        ///     Responsible for loading an object model into the currently active
        ///     scene
        /// </summary>
        /// <param name="objPath">Path to the object model</param>
        /// <returns>
        ///     Returns parent of the object model
        /// </returns>
        public static GameObject LoadObject(string objPath)
        {
            var parent = LoadObjectModel(objPath);
            LoadChildrenGameObjects(parent);

            return parent;
        }

        /// <summary>
        ///     Loads the object model into the scene
        /// </summary>
        /// <returns>Parent element of the loaded object model hierarchy</returns>
        private static GameObject LoadObjectModel(string objPath)
        {
            if (File.Exists(objPath)) return new OBJLoader().Load(objPath);

            Debug.LogError("Path to object model invalid.");
            return null;
        }

        /// <summary>
        ///     Collects the elements, that the object model contains
        /// </summary>
        /// <returns>List of all game objects inside the object model</returns>
        private static void LoadChildrenGameObjects(GameObject parent)
        {
            for (var i = 0; i < parent.transform.childCount; i++)
            {
                var child = parent.transform.GetChild(i).gameObject;

                child.AddComponent<MeshCollider>();

                // Add empty originator for the GameObject data (holds the GameObject memento instance)
                child.AddComponent<ItemInfoController>();

                // Set default memento (display name equals fixed internal id) for project creation
                // Default values will be overwritten when project gets loaded
                var defaultMemento = new ItemInfo
                {
                    isGroup = false,
                    displayName = child.name
                };

                // Add the default additional GameObject configuration to the GameObject's originator
                child.GetComponent<ItemInfoController>().ItemInfo = defaultMemento;
            }
        }
    }
}