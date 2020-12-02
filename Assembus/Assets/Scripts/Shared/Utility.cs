using System;
using System.Collections.Generic;
using System.Linq;
using MainScreen.Sidebar.HierarchyView;
using Models.Project;
using Models.Project;
using Shared.Exceptions;
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
            return children.FirstOrDefault(child => child.name == name);
        }

        /// <summary>
        ///     Toggle the visibility of an object group
        ///     (The parent should be the item's children container
        ///     if you call this function)
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
            foreach (Transform child in inputObject.transform) GetAllChildren(child.gameObject, list);
            return list;
        }

        /// <summary>
        ///     Returns all components of the given hierarchy structure
        ///     Components are all leaves of the tree excluding the children of fused groups as they behave like an leave
        /// </summary>
        /// <param name="inputObject"></param>
        /// <returns>Returns components of hierarchy</returns>
        public static List<HierarchyItemController> GetAllComponents(GameObject inputObject)
        {
            var list = new List<HierarchyItemController>();

            // Recursively get children/leaves
            GetAllComponents(inputObject.gameObject, list);

            return list;
        }

        /// <summary>
        ///     Traverse through the entire GameObject hierarchy recursively and return all component GameObjects
        /// </summary>
        /// <param name="inputObject">The input GameObject instance where the components/leaves should be extracted from</param>
        /// <param name="outputData">Contains all child GameObjects of provided input GameObject</param>
        private static void GetAllComponents(GameObject inputObject, ICollection<HierarchyItemController> outputData)
        {
            // Recursively traverse to children
            foreach (Transform child in inputObject.transform)
            {
                var itemController = child.GetComponent<HierarchyItemController>();

                // Get item info containing further information of the hierarchy element
                var itemInfo = itemController.item.GetComponent<ItemInfoController>().ItemInfo;
                if (itemInfo.isFused || !itemInfo.isGroup)
                    // Add element to return list if is a leaf or fused group
                    outputData.Add(itemController);
                else
                    // Recursive get children/leaves
                    GetAllComponents(itemController.childrenContainer, outputData);
            }
        }

        /// <summary>
        ///     Returns index of passed component inside the specified station
        /// </summary>
        /// <param name="station">Input station</param>
        /// <param name="indexedObject">Component which should be searched in passed station</param>
        /// <returns>Returns index of passed component in station</returns>
        /// <exception cref="ComponentNotFoundException">Throws exception when component not existent</exception>
        public static int GetIndexForStation(HierarchyItemController station, GameObject indexedObject)
        {
            // Get all components of the current station
            var itemList = GetAllComponents(station.childrenContainer);

            // Search for the indexed object in the station
            for (var i = 0; i < itemList.Count; i++)
                if (itemList[i].item.name == indexedObject.name)
                    return i;

            throw new ComponentNotFoundException();
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

        /// <summary>
        ///     Apply an action recursively to a GameObject and its children
        /// </summary>
        /// <param name="gameObject">The GameObject the action is applied to</param>
        /// <param name="action">The action that gets applied</param>
        /// <param name="applyToGroups">True if action should also be applied to groups</param>
        public static void ApplyRecursively(GameObject gameObject, Action<GameObject> action, bool applyToGroups)
        {
            var itemInfoController = gameObject.GetComponent<ItemInfoController>();
            
            if (itemInfoController == null || itemInfoController.ItemInfo.isGroup)
            {
                for (var i = 0; i < gameObject.transform.childCount; i++)
                    ApplyRecursively(gameObject.transform.GetChild(i).gameObject, action, applyToGroups);

                if (itemInfoController != null && applyToGroups) action.Invoke(gameObject);
            }
            else
            {
                // If game object is no group, apply the action
                action.Invoke(gameObject);
            }
        }

        /// <summary>
        ///     Check whether an item is a parent of a given model
        ///     (The parameter model should be an actual model game object)
        /// </summary>
        /// <param name="model">The model that should be checked</param>
        /// <param name="name">The name of the possible parent</param>
        /// <returns>True if the item is a parent</returns>
        public static bool IsParent(Transform model, string name)
        {
            while (model != null)
            {
                if (model.name == name) return true;
                model = model.parent;
            }

            return false;
        }
    }
}