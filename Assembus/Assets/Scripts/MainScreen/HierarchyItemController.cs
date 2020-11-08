using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Services;
using Shared.Toast;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainScreen
{
    /// <summary>
    ///     Manage the behaviour of a hierarchy view item
    /// </summary>
    public class HierarchyItemController : MonoBehaviour
    {
        /// <summary>
        ///     The text view in which the name is shown
        /// </summary>
        public TextMeshProUGUI nameText;

        /// <summary>
        ///     The rect transform of the name
        /// </summary>
        public RectTransform nameRect;

        /// <summary>
        ///     The expand button
        /// </summary>
        public GameObject expandButton, expandDown, expandRight;

        /// <summary>
        ///     The container of the item which contains all children
        /// </summary>
        public GameObject childrenContainer;

        /// <summary>
        ///     Color for a selected item
        /// </summary>
        public Color selectColor;

        /// <summary>
        ///     Normal background color
        /// </summary>
        public Color normalColor;

        /// <summary>
        ///    The Toast controller
        /// </summary>
        public ToastController toast;

        /// <summary>
        ///     Contains the key of the items, mapped to the corresponding view and model object
        /// </summary>
        private Dictionary<string, Tuple<GameObject, GameObject>> _selectedItems;
        
        /// <summary>
        ///     The root element of the hierarchy view
        /// </summary>
        private Transform _rootView;

        /// <summary>
        ///     The root element of the model
        /// </summary>
        private Transform _rootModel;

        /// <summary>
        ///     True if the child elements are expanded in the hierarchy view
        /// </summary>
        private bool _isExpanded = true;

        /// <summary>
        ///     The name of the item
        /// </summary>
        private string _name;

        /// <summary>
        ///     True if the hierarchy view needs to be updated
        /// </summary>
        private bool _updateHierarchy;

        /// <summary>
        ///     True if the item has children
        /// </summary>
        private bool HasChildren => childrenContainer.transform.childCount > 0;
        
        /// <summary>
        ///     The project manager
        /// </summary>
        private readonly ProjectManager _projectManager = ProjectManager.Instance;
        
        

        /// <summary>
        ///     Update the expand button to display the correct icon
        /// </summary>
        private void Start()
        {
            UpdateButton();
        }

        /// <summary>
        ///     Late update of the UI
        /// </summary>
        private void LateUpdate()
        {
            // force update of the hierarchy view if the item expansion changed
            if (_updateHierarchy)
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_rootView);
        }
        
        /// <summary>
        ///     Initialize the hierarchy item
        /// </summary>
        /// <param name="itemName">Name of corresponding GameObject</param>
        /// <param name="indentionDepth">Depth of indentation inside the listview</param>
        /// <param name="mainHierarchyView">Reference to the root of the hierarchy view</param>
        public void Initialize(string itemName, int indentionDepth, GameObject mainHierarchyView)
        {
            // set the name of the item
            nameText.text = itemName;

            // indent the item
            nameRect.offsetMin += new Vector2(indentionDepth, 0);
            expandButton.GetComponent<RectTransform>().anchoredPosition += new Vector2(indentionDepth, 0);

            // set the root hierarchy view
            _rootView = mainHierarchyView.GetComponent<RectTransform>();
        }

        /// <summary>
        ///     Expand the item's content
        /// </summary>
        public void ExpandItem()
        {
            if (!HasChildren) return;
            childrenContainer.SetActive(!_isExpanded);
            _isExpanded = !_isExpanded;
            _updateHierarchy = true;
            UpdateButton();
        }

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
            var currentItem = new Tuple<GameObject, GameObject>(_rootView.FindDeepChild(nameText.text).gameObject, _rootModel.FindDeepChild(nameText.text).gameObject);
            
            // Check if a parent or one/multiple children of the current element are already selected
            var parentToReplace = GetParentInDictionary(currentItem.Item2.transform, _selectedItems);
            var itemsToReplace = HasChildren ? GetChildrenInDictionary(currentItem.Item2.transform, _selectedItems) : new Collection<string>();
            if (null != parentToReplace)
                itemsToReplace.Add(parentToReplace);
            
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
        ///     Removes the previously selected items from the selection and adds the currently selected item to it
        /// </summary>
        /// <param name="currentItem"></param>
        private void NoModSelection(Tuple<GameObject, GameObject> currentItem)
        {
            if (_selectedItems.Count > 0)
                ClearSelectedItems();

            _selectedItems.Add(nameText.text, currentItem);
            _projectManager.LastSelectedItem = nameText.text;
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
            var indexCurrent = _rootView.FindDeepChild(nameText.text).GetSiblingIndex();
            
            // Add all the items in between those indexes to the selected items
            if (indexLast >= indexCurrent)
                for (var i = indexCurrent; i <= indexLast; i++)
                    AddElementOfParent(i, parent);
                
            else
                for (var i = indexLast; i <= indexCurrent; i++)
                    AddElementOfParent(i, parent);
                
            return true;
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
                (_rootView.transform.FindDeepChild(child).gameObject
                , _rootModel.FindDeepChild(child).gameObject);
            
            ClearSelectedItems();
            _selectedItems.Add(child, tempEntry);
        }
        
        /// <summary>
        ///     Deletes the currently stored list and creates a new one based on the strings in the given selection
        /// </summary>
        /// <param name="itemList">The list of key for the Objects</param>
        public void UpdateSelectedItems(Collection<string> itemList)
        {
            ClearSelectedItems();
            foreach (var item in itemList)
            {
                var tempEntry = new Tuple<GameObject, GameObject>
                (_rootView.transform.FindDeepChild(item).gameObject
                    , _rootModel.FindDeepChild(item).gameObject);

                _selectedItems.Add(item, tempEntry);
            }
            foreach (var item in _selectedItems.Values)
                SetSelectableColor(item.Item1.GetComponentInChildren<Button>(), selectColor);
        }

        /// <summary>
        ///     Will set the normalColor of a Selectable to the defined given color
        /// </summary>
        /// <param name="selectable">Selectable to be changed</param>
        /// <param name="color">Color for selectable</param>
        private static void SetSelectableColor(Selectable selectable, Color color)
        {
            var colors = selectable.colors;
            colors.normalColor = color;
            selectable.colors = colors;
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
        
        /// <summary>
        ///     Searches all child elements for elements that already exist in the given dictionary and returns them in
        ///     a collection
        /// </summary>
        /// <param name="parent">The element whose children have to be checked</param>
        /// <param name="items">The dictionary containing the items</param>
        /// <returns></returns>
        private static Collection<string> GetChildrenInDictionary<T>(Transform parent,
            IReadOnlyDictionary<string, T> items)
        {
            var ret = new Collection<string>();
            var children = parent.GetComponentsInChildren<Transform>();
            
            // Loop has to start at 1 because the first array element is the parent
            for (var i = 1; i < children.Length; i++)
            {
                if (items.ContainsKey(children[i].name))
                    ret.Add(children[i].name);
            }
            return ret;
        }
        
        /// <summary>
        ///     Checks if any of a Transform's parent elements are contained in the given dictionary
        /// </summary>
        /// <param name="child">The child element which parents have to be checked</param>
        /// <param name="items">The dictionary containing the items</param>
        /// <returns>String of the parent element that is already in the dictionary</returns>
        private static string GetParentInDictionary<T>(Transform child, IReadOnlyDictionary<string, T> items)
        {
            var parent = child.parent;
            while (parent != null)
            {
                if (items.ContainsKey(parent.name))
                {
                    return parent.name;
                }
                
                parent = parent.parent;
            }
            return null;
        }
        
        /// <summary>
        ///     Update the expand button to display the correct icons
        /// </summary>
        private void UpdateButton()
        {
            // Enable/Disable the button
            expandButton.SetActive(HasChildren);
            if (!HasChildren) return;

            // Update the logos if necessary
            expandDown.SetActive(_isExpanded);
            expandRight.SetActive(!_isExpanded);
        }
    }
}