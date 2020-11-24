using System.Collections.Generic;
using MainScreen.Sidebar.HierarchyView;
using UnityEngine;

namespace Shared
{
    public static class Utility
    {
        /// <summary>
        ///     Recursively searches for a child by its name
        /// </summary>
        /// <param name="parent">The parent transform (where the search begins) </param>
        /// <param name="name">The name of the searched child</param>
        /// <returns></returns>
        public static Transform FindChild(Transform parent, string name)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                if (name.Equals(parent.GetChild(i).name))
                    return parent.GetChild(i);

                var childTransform = FindChild(parent.GetChild(i), name);
                if (!(childTransform is null))
                    return childTransform;
            }

            return null;
        }

        /// <summary>
        ///     Toggle the visibility of an object group
        /// </summary>
        /// <param name="parent">The parent of the object group</param>
        /// <param name="visible">True if the group should be shown</param>
        public static void ToggleVisibility(Transform parent, bool visible)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var item = parent.GetChild(i).GetComponent<HierarchyItemController>();
                if (item == null) continue;
                item.ShowItem(visible);
                ToggleVisibility(item.childrenContainer.transform, visible);
            }
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
        ///     Get all children of an GameObject
        /// </summary>
        /// <param name="inputObject">The input GameObject instance where the children should be extracted from</param>
        /// <returns>Returns list of all child GameObjects of provided input GameObject</returns>
        public static List<GameObject> GetAllGameObjects(GameObject inputObject)
        {
            var list = new List<GameObject>();
            GetAllGameObjects(inputObject, list);

            return list;
        }
    }
}