using System.Collections.Generic;
using System.Linq;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace MainScreen.Sidebar.HierarchyView
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
        ///     The colors for the item
        /// </summary>
        public Color selectedColor, normalColor;

        /// <summary>
        ///     The project manager
        /// </summary>
        private readonly ProjectManager _projectManager = ProjectManager.Instance;

        /// <summary>
        ///     A list of the GameObject in Insertion Order
        /// </summary>
        private List<GameObject> _entriesInOrder;

        /// <summary>
        ///     Names of the items mapped to their GameObject
        /// </summary>
        private Dictionary<string, GameObject> _hierarchyItemNames;

        /// <summary>
        ///     Dictionary of hierarchy items mapped to a bool that indicates if the item should be highlighted
        /// </summary>
        private Dictionary<GameObject, bool> _hierarchyItems;

        /// <summary>
        ///     The item selected before the currently selected one
        /// </summary>
        private GameObject _lastSelectedItem;

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
            _hierarchyItems.Add(item, IsUnselected);
            _hierarchyItemNames.Add(item.name, item);
            _entriesInOrder.Add(item);
        }

        /// <summary>
        ///     Initializes all necessary lists
        /// </summary>
        public void InitializeLists()
        {
            _hierarchyItems = new Dictionary<GameObject, bool>();
            _hierarchyItemNames = new Dictionary<string, GameObject>();
            _entriesInOrder = new List<GameObject>();
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
            var trans = _projectManager.CurrentProject.ObjectModel.transform;
            var list = new List<GameObject>();
            foreach (var item in _hierarchyItems)
                if (item.Value)
                    list.Add(Utility.FindChild(trans, item.Key.name).gameObject);

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
            colors.normalColor = selected ? selectedColor : normalColor;
            selectable.colors = colors;
        }

        /// <summary>
        ///     Toggles the item status
        /// </summary>
        /// <param name="item">The item which status is going to be changed</param>
        private void ToggleItemStatus(GameObject item)
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

        /// <summary>
        ///     Handles the possible OnClick modifiers
        /// </summary>
        /// <param name="item"></param>
        /// <param name="mod"></param>
        public void ClickItem(GameObject item, KeyCode mod)
        {
            switch (mod)
            {
                case KeyCode.LeftShift:
                    ShiftSelection(item);
                    break;
                case KeyCode.LeftControl:
                    ControlSelection(item);
                    _lastSelectedItem = item;
                    break;
                default:
                    NoModSelection(item);
                    _lastSelectedItem = item;
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

        /// <summary>
        ///     Adds one item to the currently selected items or removes one of the item is already selected
        /// </summary>
        /// <param name="item"></param>
        private void ControlSelection(GameObject item)
        {
            if (null != _lastSelectedItem && !item.transform.parent.Equals(_lastSelectedItem.transform.parent))
                ResetAllItemStatus();

            ToggleItemStatus(item);
        }

        /// <summary>
        ///     Selects the items from the last items index to the current items index
        /// </summary>
        /// <param name="item">The currently selected item</param>
        private void ShiftSelection(GameObject item)
        {
            ResetAllItemStatus();
            if (null != _lastSelectedItem && !item.transform.parent.Equals(_lastSelectedItem.transform.parent))
            {
                ToggleItemStatus(item);
            }
            else
            {
                var parent = item.transform.parent;
                var lastItemIndex = _lastSelectedItem.transform.GetSiblingIndex();
                var currentItemIndex = item.transform.GetSiblingIndex();

                if (lastItemIndex >= currentItemIndex)
                    for (var i = currentItemIndex; i <= lastItemIndex; i++)
                        SetItemStatus(parent.GetChild(i).gameObject, IsSelected);
                else
                    for (var i = lastItemIndex; i <= currentItemIndex; i++)
                        SetItemStatus(parent.GetChild(i).gameObject, IsSelected);
            }
        }

        /// <summary>
        ///     Return the selected GameObjects as a list in the right order
        /// </summary>
        /// <returns></returns>
        public List<GameObject> GetSelectedEntriesOrdered()
        {
            return _entriesInOrder.Where(item => _hierarchyItems[item]).ToList();
        }
    }
}