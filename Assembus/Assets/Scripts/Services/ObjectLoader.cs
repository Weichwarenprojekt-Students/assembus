using System.Collections.Generic;
using System.IO;
using Dummiesman;
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
        ///     Returns a tuple consisting of the parent of the object models hierarchy and a list of all the elements inside
        ///     the parent
        /// </returns>
        public static (GameObject, List<GameObject>) LoadObject(string objPath)
        {
            var parent = LoadObjectModel(objPath);
            var children = LoadChildrenGameObjects(parent);

            return (parent, children);
        }

        /// <summary>
        ///     Loads the object model into the scene
        /// </summary>
        /// <returns>Parent element of the loaded object model hierarchy</returns>
        private static GameObject LoadObjectModel(string objPath)
        {
            if (File.Exists(objPath))
            {
                return new OBJLoader().Load(objPath);
            }
            
            Debug.LogError("Path to object model invalid.");
            return null;
        }

        /// <summary>
        ///     Collects the elements, that the object model contains
        /// </summary>
        /// <returns>List of all game objects inside the object model</returns>
        private static List<GameObject> LoadChildrenGameObjects(GameObject parent)
        {
            var children = new List<GameObject>();

            for (var i = 0; i < parent.transform.childCount; i++)
            {
                var child = parent.transform.GetChild(i).gameObject;

                child.AddComponent<MeshCollider>();

                // Apply shader to material
                var childMaterial = child.GetComponent<Renderer>().material;
                childMaterial.shader = Shader.Find("Universal Render Pipeline/Lit");
                childMaterial.color = Color.grey;

                children.Add(child);
            }

            return children;
        }
    }
}