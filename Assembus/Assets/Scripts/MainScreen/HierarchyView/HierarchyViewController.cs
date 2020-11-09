using System.Collections.Generic;
using System.Collections.ObjectModel;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace MainScreen.HierarchyView
{
    public class HierarchyViewController : MonoBehaviour
    {
        /// <summary>
        ///     The boolean value that indicates a selected field
        /// </summary>
        private const bool IsSelected = true;

        /// <summary>
        ///     The boolean value that indicates a unselected field
        /// </summary>
        private const bool IsUnselected = false;

        /// <summary>
        ///     The component highlighting
        /// </summary>
        public ComponentHighlighting componentHighlighting;

        /// <summary>
        ///     Names of the items mapped to their GameObject
        /// </summary>
        private readonly Dictionary<string, GameObject> _hierarchyItemNames = new Dictionary<string, GameObject>();

        /// <summary>
        ///     Dictionary of hierarchy items mapped to a bool that indicates if the item should be highlighted
        /// </summary>
        private readonly Dictionary<GameObject, bool> _hierarchyItems = new Dictionary<GameObject, bool>();

        /// <summary>
        ///     The project manager
        /// </summary>
        private readonly ProjectManager _projectManager = ProjectManager.Instance;

        /// <summary>
        ///     The color for an unselected button
        /// </summary>
        private readonly Color32 _unselectedColor = new Color32(53, 73, 103, 255);

        /// <summary>
        ///     The root element of the hierarchy view
        /// </summary>
        private Transform _rootView;

        /// <summary>
        ///     Add one item to the hierarchy list
        /// </summary>
        /// <param name="item">The item which is going to be added</param>
        public void AddItem(GameObject item)
        {
            if (!_hierarchyItems.ContainsKey(item));
                _hierarchyItems.Add(item, IsUnselected);

            if (!_hierarchyItemNames.ContainsKey(item.name))
                _hierarchyItemNames.Add(item.name, item);
        }

        /// <summary>
        ///     Set the status of all the given items in a list
        /// </summary>
        /// <param name="items"></param>
        public void SetItemStatusFromList(IEnumerable<string> items)
        {
            ResetAllItemStatus();

            // Set the ones contained in the given list to highlighted
            foreach (var item in items)
                SetItemStatus(_hierarchyItemNames[item], IsSelected);

            UpdateItems();
        }

        /// <summary>
        ///     Resets the status of all the items back to unselected
        /// </summary>
        private void ResetAllItemStatus()
        {
            foreach (var item in _hierarchyItemNames)
                SetItemStatus(item.Value, IsUnselected);
        }

        /// <summary>
        ///     This function is to be called when the hierarchy list has been changed
        /// </summary>
        private void UpdateItems()
        {
            var list = new List<GameObject>();
            foreach (var item in _hierarchyItems)
                if (item.Value)
                    list.Add(_projectManager.CurrentProject.ObjectModel.transform.Find(item.Key.name).gameObject);
            var s = ProjectManager.Instance.CurrentProject.ObjectModel;
            componentHighlighting.HighlightGameObjects(list);
            foreach (var item in _hierarchyItems.Keys)
                HighlightItem(item);
        }

        /// <summary>
        ///     Highlight the given item
        /// </summary>
        /// <param name="item">The item to be highlighted</param>
        private void HighlightItem(GameObject item)
        {
            SetColor(item.GetComponentInChildren<Button>(), _hierarchyItems[item]);
        }

        /// <summary>
        ///     Will set the color of the given selectable based on the selected flag given
        /// </summary>
        /// <param name="selectable"></param>
        /// <param name="selected">The flag that indicates if the item is selected</param>
        private void SetColor(Selectable selectable, bool selected)
        {
            var colors = selectable.colors;
            colors.normalColor = selected ? colors.highlightedColor : (Color) _unselectedColor;
            selectable.colors = colors;
        }

        /// <summary>
        ///     Toggles the item status
        /// </summary>
        /// <param name="item">The item which status is going to be changed</param>
        public void ToggleItemStatus(GameObject item)
        {
            _hierarchyItems[item] = !_hierarchyItems[item];
        }

        /// <summary>
        ///     Sets the status of a given item to a given status
        /// </summary>
        /// <param name="item">the item which status is going to be changed</param>
        /// <param name="status">the status it should be changed to</param>
        private void SetItemStatus(GameObject item, bool status)
        {
            _hierarchyItems[item] = status;
        }

        public void ClickItem(GameObject item, KeyCode mod)
        {
            switch (mod)
            {
                case KeyCode.LeftShift:
                    ShiftSelection(item);
                    break;
                case KeyCode.LeftControl:
                    ControlSelection(item);
                    break;
                default:
                    NoModSelection(item);
                    break;
            }

            UpdateItems();
        }

        /// <summary>
        ///     Removes the previously selected items from the selection and adds the currently selected item to it
        /// </summary>
        /// <param name="item"></param>
        private void NoModSelection(GameObject item)
        {
            ResetAllItemStatus();
            ToggleItemStatus(item);
        }

        private void ControlSelection(GameObject item)
        {
            var itemsToReplace = GetItemsOnDifHierarchyLevel(item);
            foreach (var replaceItem in itemsToReplace) ToggleItemStatus(replaceItem);

            ToggleItemStatus(item);
        }

        private void ShiftSelection(GameObject item)
        {
        }

        /// <summary>
        ///     Returns a list of items that are selected and on a different hierarchy level than the given item
        /// </summary>
        /// <param name="item">The item which parents and children are going to be checked</param>
        /// <returns></returns>
        private Collection<GameObject> GetItemsOnDifHierarchyLevel(GameObject item)
        {
            var parentToReplace = GetParentInHierarchyView(item.transform);
            var itemsToReplace = item.transform.childCount > 0
                ? GetChildrenInDictionary(item.transform)
                : new Collection<GameObject>();
            if (null != parentToReplace)
                itemsToReplace.Add(parentToReplace);
            return itemsToReplace;
        }

        /// <summary>
        ///     Searches all child elements for elements that are selected in the hierarchy View and returns them in
        ///     a collection
        /// </summary>
        /// <param name="parent">The element whose children have to be checked</param>
        /// <returns></returns>
        private Collection<GameObject> GetChildrenInDictionary(Transform parent)
        {
            var ret = new Collection<GameObject>();
            var children = parent.GetComponentsInChildren<Transform>();

            // Loop has to start at 1 because the first array element is the parent
            for (var i = 1; i < children.Length; i++)
                if (_hierarchyItems.ContainsKey(children[i].gameObject) && _hierarchyItems[children[i].gameObject])
                    ret.Add(children[i].gameObject);

            return ret;
        }

        /// <summary>
        ///     Checks if any of a Transform's parent elements are selected
        /// </summary>
        /// <param name="child">The child element which parents have to be checked</param>
        /// <returns>GameObject of the parent element that is already in the dictionary</returns>
        private GameObject GetParentInHierarchyView(Transform child)
        {
            var parent = child.parent;
            while (parent != null)
            {
                if (_hierarchyItems.ContainsKey(parent.gameObject) && _hierarchyItems[parent.gameObject])
                    return parent.gameObject;

                parent = parent.parent;
            }

            return null;
        }
    }
}