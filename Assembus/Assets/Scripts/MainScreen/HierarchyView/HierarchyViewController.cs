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
        ///     Dictionary of hierarchy items mapped to a bool that indicates if the item should be highlighted
        /// </summary>
        private readonly Dictionary<GameObject, bool> _hierarchyItems = new Dictionary<GameObject, bool>();
        
        /// <summary>
        ///     Names of the items mapped to their GameObject
        /// </summary>
        private readonly Dictionary<string, GameObject> _hierarchyItemNames = new Dictionary<string, GameObject>();

        /// <summary>
        ///     The color for an unselected button
        /// </summary>
        private readonly Color32 _unselectedColor = new Color32(53, 73, 103, 255);
        
        /// <summary>
        ///     The root element of the hierarchy view
        /// </summary>
        private Transform _rootView;

        /// <summary>
        ///     The boolean value that indicates a selected field
        /// </summary>
        private const bool IsSelected = true;

        /// <summary>
        ///     The boolean value that indicates a unselected field
        /// </summary>
        private const bool IsUnselected = false;
        
        /// <summary>
        ///     The project manager
        /// </summary>
        private readonly ProjectManager _projectManager = ProjectManager.Instance;

        /// <summary>
        ///     Update the expand button to display the correct icon
        /// </summary>
        private void Start()
        {
        }
        
        /// <summary>
        ///     Add one item to the hierarchy list
        /// </summary>
        /// <param name="item">The item which is going to be added</param>
        public void AddItem(GameObject item)
        {
            if (!_hierarchyItems.ContainsKey(item));
                _hierarchyItems.Add(item, IsUnselected);
                
            if(! _hierarchyItemNames.ContainsKey(item.name))
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
            
            Update();
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
        private void Update()
        {
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
            colors.normalColor = selected ? colors.highlightedColor : (Color)_unselectedColor;
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

            Update();
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
            Collection<GameObject> itemsToReplace = GetItemsOnDifHierarchyLevel(item);
            foreach (var replaceItem in itemsToReplace)
            {
                ToggleItemStatus(replaceItem);
            }
            
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
            var itemsToReplace = item.transform.childCount > 0 ? 
                GetChildrenInDictionary(item.transform) : new Collection<GameObject>();
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
        
        /*
        
        /// <summary>
        ///     Handle button selection
        /// </summary>
        public void SelectItem()
        {
            // A Dictionary which links the id of the currently selected items
            // to the corresponding listview and model item
            _selectedItems = _projectManager.SelectedItems;
            _rootModel = _projectManager.CurrentProject.ObjectModel.transform;
            
            // A Tuple consisting of the button and the corresponding Model Object
            var currentItem = new Tuple<GameObject, GameObject>(FindDeepChild(_rootView, nameText.text).gameObject, 
                FindDeepChild(_rootModel, nameText.text).gameObject.gameObject);
            

            
            // Remove already selected child/parent elements from the selection
            RemoveItems(itemsToReplace);
            
            // Item selection if the left control key is used
            if (Input.GetKey(KeyCode.LeftControl))
                ControlSelection(currentItem);

            // Item selection if the left shift key is used
            else if (Input.GetKey(KeyCode.LeftShift) && _projectManager.LastSelectedItem != null)
            {
                if (!ShiftSelection(currentItem))
                    toast.Error(Toast.Short, "Please stay on the same hierarchy level");
            }
            // Item Selection if No modifier is used
            else
                    NoModSelection(currentItem);

            // Update the Highlighting color in the listview
            foreach (var item in _selectedItems.Values)
                SetSelectableColor(item.Item1.GetComponentInChildren<Button>(), selectColor);
        }

        
        /// <summary>
        ///     
        /// </summary>
        /// <param name="currentItem"></param>
        private void ControlSelection(Tuple<GameObject, GameObject> currentItem)
        {
            if (_selectedItems.ContainsKey(nameText.text))
                RemoveItem(nameText.text);
            
            else
                _selectedItems.Add(nameText.text, currentItem);
            
            _projectManager.LastSelectedItem = nameText.text;
        }
        
        /// <summary>
        ///     Selects all the items between the last and the current selected item
        /// </summary>
        /// <param name="currentItem">Tuple consisting of the current view and model object</param>
        /// <returns>False if these items are not on the same hierarchy level</returns>
        private bool ShiftSelection(Tuple<GameObject, GameObject> currentItem)
        {
            var lastItem = _selectedItems[_projectManager.LastSelectedItem];

            // Return false if the parents of the last and current selected item don't match
            if (!lastItem.Item1.transform.parent.Equals(currentItem.Item1.transform.parent)) return false;
            
            // Remove all items from Selection
            ClearSelectedItems();
            
            // Get the indexes and parent of last and current item
            var indexLast = lastItem.Item1.transform.GetSiblingIndex();
            var parent = currentItem.Item1.transform.parent;
            var indexCurrent = FindDeepChild(_rootView, nameText.text).GetSiblingIndex();
            
            // Add all the items in between those indexes to the selected items
            if (indexLast >= indexCurrent)
                for (var i = indexCurrent; i <= indexLast; i++)
                    AddElementOfParent(i, parent);
                
            else
                for (var i = indexLast; i <= indexCurrent; i++)
                    AddElementOfParent(i, parent);
                
            return true;
        }

        private Transform FindDeepChild(Transform parent, string searchValue)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                if (searchValue.Equals(parent.GetChild(i).name))
                    return parent.GetChild(i);
                
                var childTransform = FindDeepChild(parent.GetChild(i), searchValue);
                if (!(childTransform is null))
                    return childTransform;
            }

            return null;
        }
        
        /// <summary>
        ///     Removes multiple items from the selected items Dictionary
        /// </summary>
        /// <param name="itemsToReplace"></param>
        private void RemoveItems(IReadOnlyCollection<string> itemsToReplace)
        {
            if (itemsToReplace.Count <= 0) return;
            
            foreach (var obj in itemsToReplace)
                RemoveItem(obj);
        }

        /// <summary>
        ///     Removes the item with the given string 
        /// </summary>
        /// <param name="itemToReplace"></param>
        private void RemoveItem(string itemToReplace)
        {
            SetSelectableColor(_selectedItems[itemToReplace].Item1.GetComponentInChildren<Button>(), normalColor);
            _selectedItems.Remove(itemToReplace);
        }

        /// <summary>
        ///     Adds the child/grandchild element of a parent with the given index to the selected items
        /// </summary>
        /// <param name="index">Index of the child element to add</param>
        /// <param name="parent">Parent of the child object to add</param>
        private void AddElementOfParent(int index, Transform parent)
        {
            var child = parent.GetChild(index).name;
            var tempEntry = new Tuple<GameObject, GameObject>
                (FindDeepChild(_rootView, child).gameObject
                , FindDeepChild(_rootModel, child).gameObject);
            
            ClearSelectedItems();
            _selectedItems.Add(child, tempEntry);
        }
        
        /// <summary>
        ///     Deletes the currently stored list and creates a new one based on the strings in the given selection
        /// </summary>
        /// <param name="itemList">The list of key for the Objects</param>
        public void UpdateSelectedItems(IEnumerable<string> itemList)
        {
            _selectedItems = _projectManager.SelectedItems;
            ClearSelectedItems();
            foreach (var item in itemList)
            {
                var tempEntry = new Tuple<GameObject, GameObject>
                (FindDeepChild(_rootView, item).gameObject
                    , FindDeepChild(_rootView, item).gameObject);

                _selectedItems.Add(item, tempEntry);
            }
            foreach (var item in _selectedItems.Values)
                SetSelectableColor(item.Item1.GetComponentInChildren<Button>(), selectColor);
        }

        /// <summary>
        ///     Removes all items from the _selectedItems Dictionary and
        ///     resets their button color to the unselected color
        /// </summary>
        private void ClearSelectedItems()
        {
            foreach (var item in _selectedItems.Values)
                SetSelectableColor(item.Item1.GetComponentInChildren<Button>(), normalColor);
            
            _selectedItems.Clear();
        }
    
        */
    }
}