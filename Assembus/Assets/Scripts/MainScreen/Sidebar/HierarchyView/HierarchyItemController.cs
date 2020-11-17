using System.Collections.Generic;
using Models.Project;
using Services;
using Services.UndoRedo;
using Shared.Toast;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MainScreen.Sidebar.HierarchyView
{
    /// <summary>
    ///     Manage the behaviour of a hierarchy view item
    /// </summary>
    public class HierarchyItemController : MonoBehaviour
    {
        /// <summary>
        ///     True if the user is currently dragging an item
        /// </summary>
        private static bool _dragging;

        /// <summary>
        ///     True if the user wants to insert an item (otherwise it will be put above)
        /// </summary>
        private static bool _insertion;

        /// <summary>
        ///     The item on which the drag ended
        /// </summary>
        private static HierarchyItemController _dragItem;

        /// <summary>
        ///     The selected items before starting a drag
        /// </summary>
        private static List<HierarchyItemController> _selectedItems = new List<HierarchyItemController>();

        /// <summary>
        ///     The colors for the item
        /// </summary>
        public Color highlightedColor, normalColor;

        /// <summary>
        ///     The hierarchy view controller
        /// </summary>
        public HierarchyViewController hierarchyViewController;

        /// <summary>
        ///     The text view in which the name is shown
        /// </summary>
        public TextMeshProUGUI nameText;

        /// <summary>
        ///     The input field for renaming an item
        /// </summary>
        public TMP_InputField nameInput;

        /// <summary>
        ///     The matching game objects for the name label and input
        /// </summary>
        public GameObject nameTextObject, nameInputObject;

        /// <summary>
        ///     The rect transform of the item's content
        /// </summary>
        public RectTransform itemContent;

        /// <summary>
        ///     The expand button
        /// </summary>
        public GameObject expandButton, expandDown, expandRight;

        /// <summary>
        ///     The container of the item which contains all children
        /// </summary>
        public GameObject childrenContainer;

        /// <summary>
        ///     The context menu controller
        /// </summary>
        public ContextMenuController contextMenu;

        /// <summary>
        ///     The indicators for moving or putting an item
        /// </summary>
        public GameObject movingIndicator;

        /// <summary>
        ///     The background of the item
        /// </summary>
        public Image background;

        /// <summary>
        ///     The text for the preview of a list drag
        /// </summary>
        public TextMeshProUGUI dragPreviewText;

        /// <summary>
        ///     The preview object for a list drag
        /// </summary>
        public GameObject dragPreview;

        /// <summary>
        ///     The toast controller
        /// </summary>
        public ToastController toast;

        /// <summary>
        ///     The scroll view
        /// </summary>
        public ScrollRect scrollRect;

        /// <summary>
        ///     The item of the actual model
        /// </summary>
        [HideInInspector] public GameObject item;

        /// <summary>
        ///     The camera controller
        /// </summary>
        public CameraController cameraController;

        /// <summary>
        ///     The click detector instance
        /// </summary>
        public DoubleClickDetector clickDetector;

        /// <summary>
        ///     The project manager
        /// </summary>
        private readonly ProjectManager _projectManager = ProjectManager.Instance;

        /// <summary>
        ///     The undo redo service
        /// </summary>
        private readonly UndoService _undoService = UndoService.Instance;

        /// <summary>
        ///     The root element of the hierarchy view
        /// </summary>
        private RectTransform _hierarchyView;

        /// <summary>
        ///     True if the child elements are expanded in the hierarchy view
        /// </summary>
        private bool _isExpanded = true;

        /// <summary>
        ///     True if the hierarchy view needs to be updated
        /// </summary>
        private bool _updateHierarchy;

        /// <summary>
        ///     True if the item has children
        /// </summary>
        private bool HasChildren => childrenContainer.transform.childCount > 0;

        /// <summary>
        ///     Update the expand button to display the correct icon
        /// </summary>
        private void Start()
        {
            UpdateButton();

            clickDetector.DoubleClickOccured += () => cameraController.ZoomOnObject(item);
        }

        /// <summary>
        ///     Late update of the UI
        /// </summary>
        private void LateUpdate()
        {
            // force update of the hierarchy view if the item expansion changed
            if (_updateHierarchy)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_hierarchyView);

            clickDetector.CheckForSecondClick();
        }

        /// <summary>
        ///     Initialize the hierarchy item
        /// </summary>
        /// <param name="modelItem">The item of the actual model</param>
        /// <param name="indentionDepth">Depth of indentation inside the listview</param>
        /// <param name="mainHierarchyView">Reference to the root of the hierarchy view</param>
        public void Initialize(GameObject modelItem, float indentionDepth, GameObject mainHierarchyView)
        {
            // Save the actual item
            item = modelItem;

            // set the name of the item
            nameText.text = item.GetComponent<ItemInfoController>().ItemInfo.displayName;

            // indent the item
            IndentItem(indentionDepth);

            // set the root hierarchy view
            _hierarchyView = mainHierarchyView.GetComponent<RectTransform>();
        }

        /// <summary>
        ///     Indent the item by a given depth
        /// </summary>
        /// <param name="indentionDepth">Depth of indentation inside the listview</param>
        public void IndentItem(float indentionDepth)
        {
            itemContent.offsetMin = new Vector2(indentionDepth, 0);
        }

        /// <summary>
        ///     Return the indention depth of the item
        /// </summary>
        /// <returns>The indention depth</returns>
        public float GetIndention()
        {
            return itemContent.offsetMin.x;
        }

        /// <summary>
        ///     Expand the item's content
        /// </summary>
        public void ExpandItem()
        {
            ExpandItem(!_isExpanded);
        }

        /// <summary>
        ///     Expand the item's content
        /// </summary>
        /// <param name="expand">True if the item shall be expanded</param>
        public void ExpandItem(bool expand)
        {
            if (HasChildren)
            {
                childrenContainer.SetActive(expand);
                _isExpanded = expand;
                _updateHierarchy = true;
            }

            UpdateButton();
        }

        /// <summary>
        ///     OnClick Method for the Selection of an item
        /// </summary>
        private void SelectItem()
        {
            // Item Selection if left control is used
            if (Input.GetKey(KeyCode.LeftControl))
                hierarchyViewController.ClickItem(this, KeyCode.LeftControl);

            // Item selection if the left shift key is used
            else if (Input.GetKey(KeyCode.LeftShift))
                hierarchyViewController.ClickItem(this, KeyCode.LeftShift);

            // Item Selection if No modifier is used
            else
                hierarchyViewController.ClickItem(this, KeyCode.None);
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

        /// <summary>
        ///     Handle clicks on the item
        /// </summary>
        /// <param name="data">The event data</param>
        public void ItemClick(BaseEventData data)
        {
            // Select the item 
            var pointerData = (PointerEventData) data;
            if (!hierarchyViewController.Contains(this) ||
                pointerData.button != PointerEventData.InputButton.Right)
                SelectItem();

            // Check what type of click happened
            switch (pointerData.button)
            {
                case PointerEventData.InputButton.Right:
                    ShowContextMenu();
                    break;
                case PointerEventData.InputButton.Left:
                    clickDetector.Click();
                    break;
            }
        }

        /// <summary>
        ///     Open the context menu on right click
        /// </summary>
        private void ShowContextMenu()
        {
            var entries = new List<ContextMenuController.Item>();
            
            entries.Add(new ContextMenuController.Item
            {
                Icon = contextMenu.edit,
                Name = "Rename",
                Action = RenameItem
            });

            var visible = item.activeSelf;
            entries.Add(new ContextMenuController.Item
            {
                Icon = visible ? contextMenu.hide : contextMenu.show,
                Name = visible ? "Hide Item" : "Show Item",
                Action = () => item.SetActive(!visible)
            });
            
            if (hierarchyViewController.SelectedItems.Contains(this))
            {
                entries.Add(new ContextMenuController.Item
                {
                    Icon = contextMenu.folder,
                    Name = "Group Selected",
                    Action = MoveToNewGroup
                });
            }
            
            if (item.GetComponent<ItemInfoController>().ItemInfo.isGroup)
            {
                entries.Add(new ContextMenuController.Item
                {
                    Icon = contextMenu.add,
                    Name = "Add Group",
                    Action = AddGroup
                });
                
                entries.Add(new ContextMenuController.Item
                {
                    Icon = contextMenu.delete,
                    Name = "Delete",
                    Action = DeleteItem
                });
            }

            contextMenu.Show(entries);
        }

        /// <summary>
        ///     Start a renaming action
        /// </summary>
        private void RenameItem()
        {
            nameInput.text = nameText.text;
            nameInputObject.SetActive(true);
            nameInput.Select();
            nameTextObject.SetActive(false);
        }

        /// <summary>
        ///     Item deletion
        /// </summary>
        private void DeleteItem()
        {
            //Check if the group isn't empty
            if (item.transform.childCount > 0)
            {
                toast.Error(Toast.Short, "Only empty groups can be deleted!");
            }
            else
            {
                // Save the old item state
                var oldState = new[] {ItemState.FromListItem(this)};

                // Create the new item state
                var newState = new[] {new ItemState(oldState[0])};

                // Add the new action to the undo redo service
                _undoService.AddCommand(new Command(newState, oldState, Command.Delete));
            }
        }

        /// <summary>
        ///     Add new group
        /// </summary>
        private void AddGroup()
        {
            // Save the old item state
            var oldState = new[]
            {
                new ItemState(
                    _projectManager.GetNextGroupID(),
                    "Group",
                    item.name,
                    item.transform.childCount
                )
            };

            // Create the new item state
            var newState = new[] {new ItemState(oldState[0])};

            // Add the new action to the undo redo service
            _undoService.AddCommand(new Command(newState, oldState, Command.Create));
        }

        /// <summary>
        ///     Create a new group and move the items into the group
        /// </summary>
        private void MoveToNewGroup()
        {
            // Create the group
            var oldState = new[]
            {
                new ItemState(
                    _projectManager.GetNextGroupID(),
                    "Group",
                    item.transform.parent.name,
                    item.transform.GetSiblingIndex()
                )
            };
            var newState = new[] {new ItemState(oldState[0])};
            _undoService.AddCommand(new Command(newState, oldState, Command.Create));

            // Move the items
            _dragItem =
                Utility.FindChild(_hierarchyView.transform, newState[0].ID).GetComponent<HierarchyItemController>();
            _insertion = true;
            _selectedItems = hierarchyViewController.GetSelectedItems();
            InsertItems();
        }

        /// <summary>
        ///     Cancel a rename action
        /// </summary>
        public void CancelRenaming()
        {
            nameInputObject.SetActive(false);
            nameTextObject.SetActive(true);
        }

        /// <summary>
        ///     Apply a rename action
        /// </summary>
        public void ApplyRenaming()
        {
            // Check if there's a name given
            var newName = nameInput.text;
            if (newName == "")
            {
                toast.Error(Toast.Short, "Name cannot be empty!");
                return;
            }

            // Save the old item state
            var oldState = new[] {ItemState.FromListItem(this)};

            // Create the new item state
            var newState = new[] {new ItemState(oldState[0]) {Name = newName}};

            // Hide the input field an show the name field
            nameInputObject.SetActive(false);
            nameTextObject.SetActive(true);

            // Add the new action to the undo redo service
            _undoService.AddCommand(new Command(newState, oldState, Command.Rename));
        }

        /// <summary>
        ///     Start dragging one or multiple items
        /// </summary>
        /// <param name="data">Event data</param>
        public void StartDraggingItem(BaseEventData data)
        {
            // Get the selected items
            _selectedItems = hierarchyViewController.GetSelectedItems();
            if (_selectedItems.Count == 0 || !_selectedItems.Contains(this)) return;

            // Show which items are dragged
            var firstName = item.GetComponent<ItemInfoController>().ItemInfo.displayName;
            dragPreviewText.text = _selectedItems.Count > 1 ? "Multiple Items" : firstName;
            dragPreview.transform.position = ((PointerEventData) data).position;
            dragPreview.SetActive(true);
            _dragging = true;
        }

        /// <summary>
        ///     Drag one or multiple items
        /// </summary>
        /// <param name="data">Event data</param>
        public void DragItem(BaseEventData data)
        {
            // Get the pointer position
            Vector3 position = ((PointerEventData) data).position;
            dragPreview.transform.position = new Vector3(position.x + 5, position.y - 5, 0);
        }

        /// <summary>
        ///     Stop dragging
        /// </summary>
        /// <param name="data">Event data</param>
        public void StopDraggingItem(BaseEventData data)
        {
            // Reset the drag event
            dragPreview.SetActive(false);
            _dragging = false;

            // Insert the items (Only if the dragged item was selected)
            if (_selectedItems.Count != 0 && hierarchyViewController.Contains(this)) InsertItems();
        }

        /// <summary>
        ///     Insert the currently selected items into a group
        /// </summary>
        private void InsertItems()
        {
            // Check if the drag leads to a change
            if (_dragItem == null) return;

            // Get the new parent and the new sibling id
            var parent = _insertion ? _dragItem.gameObject.name : _dragItem.item.transform.parent.name;
            var siblingIndex = _insertion
                ? _dragItem.childrenContainer.transform.childCount
                : _dragItem.item.transform.GetSiblingIndex();

            // Save the old item states
            var oldStates = new ItemState[_selectedItems.Count];
            for (var i = 0; i < oldStates.Length; i++) oldStates[i] = ItemState.FromListItem(_selectedItems[i]);

            // Create the new item states
            var newStates = new ItemState[_selectedItems.Count];
            var newParent =
                _insertion ? _dragItem.childrenContainer.transform : _dragItem.gameObject.transform.parent;
            var offset = 0;
            for (var i = 0; i < newStates.Length; i++)
            {
                var sameParent = newParent == _selectedItems[i].transform.parent;
                var smallerIndex = _dragItem.transform.GetSiblingIndex() >
                                   _selectedItems[i].transform.GetSiblingIndex();
                if (sameParent && smallerIndex) offset--;
                newStates[i] = new ItemState(oldStates[i])
                    {ParentID = parent, SiblingIndex = siblingIndex + i + offset};
            }

            // Add the new action to the undo redo service
            _undoService.AddCommand(new Command(newStates, oldStates, Command.Move));
        }

        /// <summary>
        ///     Put an item above this item
        /// </summary>
        /// <param name="data">Event data</param>
        public void PutAbove(BaseEventData data)
        {
            // Change the color
            var selected = hierarchyViewController.Contains(this);
            background.color = _dragging && !selected ? normalColor : highlightedColor;


            // Show the moving indicator
            movingIndicator.SetActive(_dragging && !selected);

            // Save the item and the action
            _insertion = false;
            _dragItem = selected ? null : this;
        }

        /// <summary>
        ///     Stop putting an item above this item
        /// </summary>
        /// <param name="data">Event data</param>
        public void StopPuttingAbove(BaseEventData data)
        {
            movingIndicator.SetActive(false);
            if (!hierarchyViewController.Contains(this)) background.color = normalColor;
            _dragItem = null;
        }

        /// <summary>
        ///     Insert an item into this item
        ///     (only works with if this item is a group)
        /// </summary>
        /// <param name="data">Event data</param>
        public void InsertItem(BaseEventData data)
        {
            // Change the color
            var isGroup = item.GetComponent<ItemInfoController>().ItemInfo.isGroup;
            var selected = hierarchyViewController.Contains(this);
            background.color = _dragging && !isGroup && !selected ? normalColor : highlightedColor;

            // Save the item and the action if item is compatible
            _insertion = true;
            _dragItem = isGroup && !selected ? this : null;
        }

        /// <summary>
        ///     Stop inserting an item into this item
        /// </summary>
        /// <param name="data">Event data</param>
        public void StopInsertingItem(BaseEventData data)
        {
            _dragItem = null;
            if (!hierarchyViewController.Contains(this)) background.color = normalColor;
        }

        /// <summary>
        ///     Forward the scroll data
        /// </summary>
        /// <param name="data">Event data</param>
        public void OnScroll(BaseEventData data)
        {
            scrollRect.OnScroll((PointerEventData) data);
        }
    }
}