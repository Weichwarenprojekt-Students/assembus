using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Models.Project;
using Services.Serialization;
using Services.UndoRedo;
using Services.UndoRedo.Commands;
using Services.UndoRedo.Models;
using Shared;
using Shared.Toast;
using UnityEngine;
using UnityEngine.UI;

namespace MainScreen.Sidebar.HierarchyView
{
    public class HierarchyViewController : MonoBehaviour
    {
        /// <summary>
        ///     The indention of the items
        /// </summary>
        public const float Indention = 16f;

        /// <summary>
        ///     The component highlighting
        /// </summary>
        public ComponentHighlighting componentHighlighting;

        /// <summary>
        ///     The toast controller
        /// </summary>
        public ToastController toast;

        /// <summary>
        ///     The hierarchy view
        /// </summary>
        public GameObject hierarchyView;

        /// <summary>
        ///     A default hierarchy view item
        /// </summary>
        public GameObject defaultHierarchyViewItem;

        /// <summary>
        ///     The colors for the item
        /// </summary>
        public Color selectedColor, normalColor;

        /// <summary>
        ///     The context menu controller
        /// </summary>
        public ContextMenuController contextMenu;

        /// <summary>
        ///     RectTransform from Content
        /// </summary>
        public RectTransform contentPanel;

        /// <summary>
        ///     RectTransform from Scroll View
        /// </summary>
        public RectTransform scrollRectTrans;

        /// <summary>
        ///     ScrollRect from Scroll View
        /// </summary>
        public ScrollRect scrollRect;

        /// <summary>
        ///     The project manager
        /// </summary>
        private readonly ProjectManager _projectManager = ProjectManager.Instance;

        /// <summary>
        ///     The undo redo service
        /// </summary>
        private readonly UndoService _undoService = UndoService.Instance;

        /// <summary>
        ///     A list of the GameObject in Insertion Order
        /// </summary>
        public readonly List<HierarchyItemController> SelectedItems = new List<HierarchyItemController>();

        /// <summary>
        ///     The CommandGroup instance
        /// </summary>
        private CommandGroup _commandGroup;

        /// <summary>
        ///     The item selected before the currently selected one
        /// </summary>
        private HierarchyItemController _lastSelectedItem;

        /// <summary>
        ///     Gets true if the user scrolls manually while auto scroll to stop auto scroll
        /// </summary>
        private bool _stopAutoScroll;

        /// <summary>
        ///     True if hierarchy view needs to be updated
        /// </summary>
        private bool _updateHierarchyView;

        /// <summary>
        ///     Late update of the UI
        /// </summary>
        private void LateUpdate()
        {
            if (_updateHierarchyView)
                LayoutRebuilder.ForceRebuildLayoutImmediate(hierarchyView.GetComponent<RectTransform>());

            // Stop autoscroll if user manually scrolls
            if (Input.mouseScrollDelta != Vector2.zero) _stopAutoScroll = true;
        }

        /// <summary>
        ///     Clear the selections
        /// </summary>
        private void OnEnable()
        {
            SelectedItems.Clear();

            // Initialize the command executor
            Command.Initialize(
                _projectManager.CurrentProject.ObjectModel,
                hierarchyView,
                this
            );

            // Load the model
            LoadModelIntoHierarchyView();
        }

        /// <summary>
        ///     Open the context menu for the list view
        /// </summary>
        public void ShowContextMenu()
        {
            var entries = new List<ContextMenuController.Item>();

            entries.Add(
                new ContextMenuController.Item
                {
                    Icon = contextMenu.add,
                    Name = "Create Station",
                    Action = CreateAssemblyStation
                }
            );

            entries.Add(
                new ContextMenuController.Item
                {
                    Icon = contextMenu.show,
                    Name = "Show All",
                    Action = () => SetObjectVisibility(true)
                }
            );

            entries.Add(
                new ContextMenuController.Item
                {
                    Icon = contextMenu.hide,
                    Name = "Hide All",
                    Action = () => SetObjectVisibility(false)
                }
            );

            contextMenu.Show(entries);
        }

        /// <summary>
        ///     Load the object model into the hierarchy view
        /// </summary>
        private void LoadModelIntoHierarchyView()
        {
            defaultHierarchyViewItem.SetActive(true);

            // Get the root element of the object model
            var parent = _projectManager.CurrentProject.ObjectModel;

            // Remove the old children
            RemoveElementWithChildren(hierarchyView.transform, true);

            // Execute the recursive loading of game objects
            LoadElementWithChildren(hierarchyView, parent);

            // Force hierarchy view update
            _updateHierarchyView = true;

            defaultHierarchyViewItem.SetActive(false);
        }

        /// <summary>
        ///     Remove all previous list view items
        /// </summary>
        /// <param name="parent">The parent of the children that shall be removed</param>
        /// <param name="first">True if it is the first (to make sure that the default item isn't deleted)</param>
        private static void RemoveElementWithChildren(Transform parent, bool first)
        {
            for (var i = first ? 1 : 0; i < parent.childCount; i++)
            {
                RemoveElementWithChildren(parent.GetChild(i).transform, false);
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        /// <summary>
        ///     Load all elements of the game object and add them to the list
        /// </summary>
        /// <param name="containingListView">The container of the list view</param>
        /// <param name="parent">The parent item on the actual model</param>
        /// <param name="depth">The margin to the left side</param>
        private void LoadElementWithChildren(GameObject containingListView, GameObject parent, float depth = 0)
        {
            for (var i = 0; i < parent.transform.childCount; i++)
            {
                var child = parent.transform.GetChild(i).gameObject;

                // Add list item
                var itemController = AddListItem(containingListView, child, depth);

                // Fill the new item recursively with children
                LoadElementWithChildren(
                    itemController.childrenContainer,
                    child,
                    depth + Indention
                );
            }
        }

        /// <summary>
        ///     Add a list view item
        /// </summary>
        /// <param name="parent">The parent container</param>
        /// <param name="item">The actual model</param>
        /// <param name="depth">The depth of indention</param>
        /// <returns>The newly created item controller</returns>
        private HierarchyItemController AddListItem(GameObject parent, GameObject item, float depth)
        {
            // generate a new hierarchy item in the hierarchy view
            var newHierarchyItem = Instantiate(
                defaultHierarchyViewItem,
                parent.transform,
                true
            );

            newHierarchyItem.name = item.name;

            // get the script of the new item
            var itemController = newHierarchyItem.GetComponent<HierarchyItemController>();

            // initialize the item
            itemController.Initialize(item, depth, hierarchyView);

            return itemController;
        }

        /// <summary>
        ///     Add a single list view item
        /// </summary>
        /// <param name="parent">The parent container</param>
        /// <param name="item">The actual model</param>
        /// <param name="depth">The depth of indention</param>
        public void AddSingleListItem(GameObject parent, GameObject item, float depth)
        {
            defaultHierarchyViewItem.SetActive(true);
            AddListItem(parent, item, depth);
            defaultHierarchyViewItem.SetActive(false);
        }

        /// <summary>
        ///     Remove a game object
        /// </summary>
        /// <param name="item">The game object that shall be removed</param>
        public void DeleteObject(GameObject item)
        {
            Destroy(item);
        }

        /// <summary>
        ///     Create an assembly station
        /// </summary>
        private void CreateAssemblyStation()
        {
            var state = new ItemState(
                _projectManager.GetNextGroupID(),
                "Assembly Station",
                _projectManager.CurrentProject.ObjectModel.name,
                ItemState.Last
            );

            // Add the new action to the undo redo service
            var createCommand = new CreateCommand(true, state);
            _commandGroup = new CommandGroup();
            _undoService.AddCommand(_commandGroup);
            _commandGroup.AddToGroup(createCommand);
            createCommand.Redo();

            // Scroll to the created station
            var stationItem = hierarchyView.transform.GetChild(hierarchyView.transform.childCount - 1);
            ScrollToItem(stationItem.GetComponent<RectTransform>());
            stationItem.GetComponent<HierarchyItemController>().RenameItem(true, _commandGroup);
        }

        /// <summary>
        ///     Show or hide the model
        /// </summary>
        /// <param name="visible">True if the model shall be shown</param>
        private void SetObjectVisibility(bool visible)
        {
            Utility.ToggleVisibility(hierarchyView.transform, visible);
        }

        /// <summary>
        ///     Set the status of all the given items in a list
        /// </summary>
        /// <param name="items">The item names</param>
        public void SetItemStatusFromList(IEnumerable<string> items)
        {
            DeselectItems();

            // Set the ones contained in the given list to highlighted
            foreach (var item in items)
                SelectItem(Utility.FindChild(hierarchyView.transform, item).GetComponent<HierarchyItemController>());
        }

        /// <summary>
        ///     Adds the given item to the list and highlights it
        /// </summary>
        /// <param name="item">The given item</param>
        private void SelectItem(HierarchyItemController item)
        {
            SelectedItems.Add(item);
            SetColor(item, true);
        }

        /// <summary>
        ///     Removes the given item from the list and removes the highlighting
        /// </summary>
        /// <param name="item">The given item</param>
        private void DeselectItem(HierarchyItemController item)
        {
            SelectedItems.Remove(item);
            SetColor(item, false);
            _lastSelectedItem = null;
        }

        /// <summary>
        ///     Removes items from list and removes the highlighting
        /// </summary>
        private void DeselectItems()
        {
            foreach (var item in SelectedItems) SetColor(item, false);

            SelectedItems.Clear();
        }

        /// <summary>
        ///     Highlight the matching objects in the model
        /// </summary>
        private void HighlightModel()
        {
            var trans = _projectManager.CurrentProject.ObjectModel.transform;
            var list = SelectedItems.Select(item => Utility.FindChild(trans, item.name).gameObject).ToList();

            componentHighlighting.HighlightGameObjects(list);
        }

        /// <summary>
        ///     Will set the color of the given selectable based on the selected flag given
        /// </summary>
        /// <param name="item"></param>
        /// <param name="selected">The flag that indicates if the item is selected</param>
        private void SetColor(HierarchyItemController item, bool selected)
        {
            item.background.color = selected ? selectedColor : normalColor;
        }

        /// <summary>
        ///     Check if an item is selected
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>True if item is contained</returns>
        public bool IsSelected(HierarchyItemController item)
        {
            return SelectedItems.Contains(item);
        }

        /// <summary>
        ///     Handles the possible OnClick modifiers
        /// </summary>
        /// <param name="item">The selected item</param>
        /// <param name="mod">The modifier that is used</param>
        public void ClickItem(HierarchyItemController item, KeyCode mod)
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

            HighlightModel();
        }

        /// <summary>
        ///     Removes the previously selected items from the selection and adds the currently selected item to it
        /// </summary>
        /// <param name="item">the selected item</param>
        private void NoModSelection(HierarchyItemController item)
        {
            _lastSelectedItem = item;
            var selected = IsSelected(item) && SelectedItems.Count <= 1;
            DeselectItems();
            if (!selected) SelectItem(item);
            else _lastSelectedItem = null;
        }

        /// <summary>
        ///     Adds one item to the currently selected items or removes one of the item is already selected
        /// </summary>
        /// <param name="item">the selected item</param>
        private void ControlSelection(HierarchyItemController item)
        {
            _lastSelectedItem = item;
            var isGroup = item.item.GetComponent<ItemInfoController>().ItemInfo.isGroup;
            if (isGroup) DeselectChildren(item.childrenContainer.transform);
            else DeselectParent(item.transform);

            var selected = SelectedItems.Contains(item);
            if (selected)
            {
                DeselectItem(item);
                if (SelectedItems.Count > 0) _lastSelectedItem = SelectedItems[SelectedItems.Count - 1];
            }
            else
            {
                SelectItem(item);
            }
        }

        /// <summary>
        ///     Selects the items from the last items index to the current items index
        /// </summary>
        /// <param name="item">The selected item</param>
        private void ShiftSelection(HierarchyItemController item)
        {
            if (_lastSelectedItem == null)
            {
                NoModSelection(item);
            }
            else if (_lastSelectedItem.transform.parent != item.transform.parent)
            {
                toast.Error(Toast.Short, "Please stay on the\nsame hierarchy level");
            }
            else
            {
                DeselectItems();
                var parent = item.transform.parent;
                var lastItemIndex = _lastSelectedItem.transform.GetSiblingIndex();
                var currentItemIndex = item.transform.GetSiblingIndex();

                if (lastItemIndex >= currentItemIndex)
                    for (var i = currentItemIndex; i <= lastItemIndex; i++)
                        SelectItem(parent.GetChild(i).GetComponent<HierarchyItemController>());
                else
                    for (var i = lastItemIndex; i <= currentItemIndex; i++)
                        SelectItem(parent.GetChild(i).GetComponent<HierarchyItemController>());
            }
        }

        /// <summary>
        ///     Return the selected GameObjects as a list in the right order
        /// </summary>
        /// <returns></returns>
        public List<HierarchyItemController> GetSelectedItems()
        {
            return SelectedItems.OrderByDescending(item => item.itemContent.position.y).ToList();
        }

        /// <summary>
        ///     Searches all child elements for elements that are selected in the hierarchy View and removes them
        /// </summary>
        /// <param name="parent">The element whose children have to be checked</param>
        private void DeselectChildren(Transform parent)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i).GetComponent<HierarchyItemController>();
                if (SelectedItems.Contains(child)) DeselectItem(child);
                else DeselectChildren(child.childrenContainer.transform);
            }
        }

        /// <summary>
        ///     Checks if any of a Transform's parent elements are selected and removes them from selection
        /// </summary>
        /// <param name="child">The child element which parents have to be checked</param>
        /// <returns>GameObject of the parent element that is already in the dictionary</returns>
        private void DeselectParent(Transform child)
        {
            var parent = child.parent.parent.GetComponent<HierarchyItemController>();
            if (parent == null) return;
            if (SelectedItems.Contains(parent)) DeselectItem(parent);
            else DeselectParent(parent.transform);
        }

        /// <summary>
        ///     Scrolls to the targetItem
        /// </summary>
        /// <param name="targetItem"></param>
        public void ScrollToItem(RectTransform targetItem)
        {
            scrollRect.enabled = false;
            Canvas.ForceUpdateCanvases();
            scrollRect.enabled = true;

            // Top border of the actual viewport
            var topBorder = contentPanel.localPosition.y;

            // Lower border of the actual viewport
            var lowerBorder = topBorder + scrollRectTrans.rect.height;

            // Target item position in the viewport
            var itemPosition = scrollRectTrans.transform.InverseTransformPoint(contentPanel.position).y -
                               scrollRectTrans.transform.InverseTransformPoint(targetItem.position).y;

            // Check if item is outside the borders, if so, scroll to the item
            if (!(itemPosition < lowerBorder && topBorder < itemPosition))
                StartCoroutine(ScrollToTarget(itemPosition - 200));
        }

        /// <summary>
        ///     Coroutine for smoth scrolling to an new item
        /// </summary>
        /// <param name="targetValue"></param>
        /// <returns></returns>
        private IEnumerator ScrollToTarget(float targetValue)
        {
            var interpolatedFloat = new InterpolatedFloat(contentPanel.anchoredPosition.y);
            _stopAutoScroll = false;
            while (!interpolatedFloat.IsAtValue(targetValue) && !_stopAutoScroll)
            {
                interpolatedFloat.ToValue(targetValue);

                contentPanel.anchoredPosition = new Vector2(0, interpolatedFloat.Value);

                // Check, if the scroll view will scroll outside the viewport
                if (scrollRect.normalizedPosition.y < 0)
                {
                    scrollRect.normalizedPosition = scrollRect.viewport.anchorMin;
                    break;
                }

                if (scrollRect.normalizedPosition.y > 1)
                {
                    scrollRect.normalizedPosition = scrollRect.viewport.anchorMax;
                    break;
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }
}