using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MainScreen.Sidebar.HierarchyView;
using Models.Project;
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
        /// <returns>The searched child</returns>
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
        ///     Perform a linear search on a given list
        /// </summary>
        /// <param name="children">The children</param>
        /// <param name="name">The name of the searched child</param>
        /// <returns>The searched child</returns>
        public static GameObject FindChildLinear(List<GameObject> children, string name)
        {
            return (from child in children where child.name == name select child).FirstOrDefault();
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
        private static void GetAllChildren(GameObject inputObject, ICollection<GameObject> outputData)
        {
            // Add new child to the GameObject list
            outputData.Add(inputObject);

            // Recursively traverse to children
            foreach (Transform child in inputObject.transform) GetAllChildren(child.gameObject, outputData);
        }

        /// <summary>
        ///     Get all children of an GameObject
        /// </summary>
        /// <param name="inputObject">The input GameObject instance where the children should be extracted from</param>
        /// <returns>Returns list of all child GameObjects of provided input GameObject</returns>
        public static List<GameObject> GetAllChildren(GameObject inputObject)
        {
            var list = new List<GameObject>();
            
            // Recursively get all children
            GetAllChildren(inputObject, list);
            
            return list;
        }

        /// <summary>
        ///     Returns all components of the given hierarchy structure
        ///
        ///     Components are all leaves of the tree excluding the children of fused groups as they behave like an leave
        /// </summary>
        /// <param name="inputObject"></param>
        /// <returns></returns>
        public static List<GameObject> GetAllComponents(GameObject inputObject)
        {
            var list = new List<GameObject>();
            
            // Recursively get children/leaves
            GetAllComponents(inputObject.gameObject, list);   
            
            return list;
        }
        
        /// <summary>
        ///     Traverse through the entire GameObject hierarchy recursively and return all component GameObjects
        /// </summary>
        /// <param name="inputObject">The input GameObject instance where the components/leaves should be extracted from</param>
        /// <param name="outputData">Contains all child GameObjects of provided input GameObject</param>
        private static void GetAllComponents(GameObject inputObject, ICollection<GameObject> outputData)
        {
            // Recursively traverse to children
            foreach (Transform child in inputObject.transform)
            {
                // Get item info containing further information of the hierarchy element
                ItemInfo itemInfo = child.GetComponent<ItemInfoController>().ItemInfo;
                if (itemInfo.isFused || !itemInfo.isGroup)
                {
                    // Add element to return list if is a leaf or fused group
                    outputData.Add(inputObject);
                }
                else
                {
                    // Recursive get children/leaves
                    GetAllComponents(child.gameObject, outputData);   
                }
            }
        }
        
        /// <summary>
        ///     Get the id of the lower neighbour of a given item
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>The neighbour id</returns>
        public static string GetNeighbourID(Transform item)
        {
            var neighbourID = "";
            if (item.GetSiblingIndex() - 1 >= 0) neighbourID = item.parent.GetChild(item.GetSiblingIndex() - 1).name;
            return neighbourID;
        }
    }
}