using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;

namespace Services
{
    /// <summary>
    ///     This class serializes the GameObject configuration but not the GameObject itself.
    ///     Only the actual hierarchy of the GameObject will be saved/loaded.
    /// </summary>
    public class GameObjectDeSerializer
    {
        /// <summary>
        ///     The XML-Serializer instance
        /// </summary>
        private readonly XmlDeSerializer<List<ModelComponent>> _xmlDeSerializer =
            new XmlDeSerializer<List<ModelComponent>>();

        /// <summary>
        ///     Saves the configuration/hierarchy of the passed GameObject instance to a XML file.
        ///     This serializer does NOT store anything else than the hierarchy!
        /// </summary>
        /// <param name="filePath">The path where the GameObject config should be stored as a XML file</param>
        /// <param name="rootObject">The actual GameObject instance whose configuration should be saved</param>
        public void SerializeGameObject(string filePath, GameObject rootObject)
        {
            //Extract all child GameObjects/hierarchy from the provided root GameObject recursively
            var allOriginalChildren = new List<GameObject>();
            GetAllGameObjects(rootObject, allOriginalChildren);

            //List which stores the GameObject description of all child GameObjects
            var saveHierarchy = new List<ModelComponent>();

            foreach (var originalChild in allOriginalChildren)
            {
                // Generate a new description
                var newComponent = new ModelComponent {gameObjectName = originalChild.name};

                // Write the string 'null' if there is no parent GameObject
                var parent = originalChild.transform.parent;
                newComponent.parentGameObjectName = parent != null ? parent.name : "null";

                // Add the ModelComponent description to the list
                saveHierarchy.Add(newComponent);
            }

            // Write ModelComponent-List to XML file
            _xmlDeSerializer.SerializeData(filePath, saveHierarchy);
        }

        /// <summary>
        ///     Loads GameObject configuration/hierarchy from XMl file and applies changes to passed instance.
        ///     Please make a deep-copy of the passed GameObject if you need an untouched instance.
        /// </summary>
        /// <param name="filePath">The path where the GameObject config is stored as a XML file</param>
        /// <param name="inputData">The actual GameObject instance which should be returned modified</param>
        public GameObject DeserializeGameObject(string filePath, GameObject inputData)
        {
            // The root reference which will be returned
            var rootObject = new GameObject();

            // Stores correctly ordered GameObject instances
            var outputData = new List<GameObject>();

            // Load all components described in XML file from the original GameObject
            var allOriginalChildren = new List<GameObject>();
            GetAllGameObjects(inputData, allOriginalChildren);

            // Destroy the existing hierarchy to avoid incorrect referencing
            foreach (var go in allOriginalChildren)
                go.transform.parent = null;

            // Load the hierarchy description of all GameObject models
            var loadedHierarchy = _xmlDeSerializer.DeserializeData(filePath);

            // Go through the content of the config file
            foreach (var hierarchyItem in loadedHierarchy)
            {
                // Get the original GameObject by its name.
                var originalObject = GetChildByName(allOriginalChildren, hierarchyItem.gameObjectName);

                // Object not existing in original GameObject list --> New/own grouping
                if (originalObject == null) originalObject = new GameObject {name = hierarchyItem.gameObjectName};

                // Check if we loaded the root object or a child object
                if (hierarchyItem.parentGameObjectName != "null")
                {
                    // Search the new existing parent in the new outputData list
                    var newParent = GetChildByName(outputData, hierarchyItem.parentGameObjectName);

                    // Override original parent with new parent in new outputData data structure
                    originalObject.transform.parent = newParent.transform;
                }
                else
                    // Found root. Store root reference for later return
                {
                    rootObject = originalObject;
                }

                // Add the new GameObject to the outputData list
                outputData.Add(originalObject);
            }

            // Return root node/instance
            return rootObject;
        }

        /// <summary>
        ///     Traverse through the entire GameObject hierarchy recursively and return all child GameObjects
        /// </summary>
        /// <param name="inputObject">The input GameObject instance where the children should be extracted from</param>
        /// <param name="outputData">Contains all child GameObjects of provided input GameObject</param>
        private static void GetAllGameObjects(GameObject inputObject, ICollection<GameObject> outputData)
        {
            // Add new child to the GameObject list
            outputData.Add(inputObject);

            // Recursively traverse to children
            foreach (Transform child in inputObject.transform) GetAllGameObjects(child.gameObject, outputData);
        }

        /// <summary>
        ///     Search for a specific GameObject in the GameObject list by specifying GameObject name
        /// </summary>
        /// <param name="gameObjects">The GameObject instance list where the search should take place</param>
        /// <param name="name">The name of the requested GameObject instance</param>
        /// <returns></returns>
        private static GameObject GetChildByName(IEnumerable<GameObject> gameObjects, string name)
        {
            return gameObjects.FirstOrDefault(gameObject => gameObject.name == name);
        }
    }
}