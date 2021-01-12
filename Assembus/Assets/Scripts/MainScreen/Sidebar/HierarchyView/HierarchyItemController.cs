using System;
using System.Collections.Generic;
using System.Linq;
using MainScreen.StationView;
using Models.Project;
using Services;
using Services.Serialization;
using Services.UndoRedo;
using Services.UndoRedo.Commands;
using Services.UndoRedo.Models;
using Shared;
using Shared.Exceptions;
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
        public static bool Dragging;

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
        ///     Is the rename from a new Group
        /// </summary>
        private static bool _isRenameInitial;

        /// <summary>
        ///     Is a Item shifted to a Group
        /// </summary>
        private static bool _isItemShifted;

        /// <summary>
        ///     Reference to ComponentHighlighting script
        /// </summary>
        public ComponentHighlighting componentHighlighting;

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
        ///     The expand button with its logos
        /// </summary>
        public GameObject expandButton, expandDown, expandRight, fusion;

        /// <summary>
        ///     The button for showing a station
        /// </summary>
        public GameObject showStation;

        /// <summary>
        ///     Visualizes current item in the sequence view
        /// </summary>
        public GameObject itemActive;

        /// <summary>
        ///     The controller of the station view
        /// </summary>
        public StationController stationController;

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
        ///     The item of the actual model
        /// </summary>
        [HideInInspector] public ItemInfoController itemInfo;

        /// <summary>
        ///     The camera controller
        /// </summary>
        public CameraController cameraController;

        /// <summary>
        ///     The click detector instance
        /// </summary>
        public DoubleClickDetector doubleClickDetector;

        /// <summary>
        ///     The text colors for visible and invisible items
        /// </summary>
        public Color visibleColor, invisibleColor;

        /// <summary>
        ///     The root element of the hierarchy view
        /// </summary>
        public RectTransform hierarchyView;

        /// <summary>
        ///     The project manager
        /// </summary>
        private readonly ProjectManager _projectManager = ProjectManager.Instance;

        /// <summary>
        ///     The undo redo service
        /// </summary>
        private readonly UndoService _undoService = UndoService.Instance;

        /// <summary>
        ///     True if the item was actually clicked
        /// </summary>
        private bool _clicked;

        /// <summary>
        ///     The CommandGroup instance
        /// </summary>
        private CommandGroup _commandGroup;

        /// <summary>
        ///     True if the child elements are expanded in the hierarchy view
        /// </summary>
        private bool _isExpanded = true;

        /// <summary>
        ///     True if the hierarchy view needs to be updated
        /// </summary>
        private bool _updateHierarchy;

        /// <summary>
        ///     True if the item is a station
        /// </summary>
        public bool IsStation => itemInfo.ItemInfo.isGroup && transform.parent == hierarchyView;

        /// <summary>
        ///     Late update of the UI
        /// </summary>
        private void LateUpdate()
        {
            // force update of the hierarchy view if the item expansion changed
            if (_updateHierarchy)
                LayoutRebuilder.ForceRebuildLayoutImmediate(hierarchyView);

            // Detect double click on list item
            doubleClickDetector.CheckForSecondClick();

            // Scroll if dragging an item up/down
            if (Dragging)
                ScrollOnDrag();

            // Apply rename of component on press enter key
            if (Input.GetKey(KeyCode.Return) && nameInputObject.activeSelf) ApplyRenaming();

            // Cancel renaming a component on press escape key
            if (Input.GetKey(KeyCode.Escape) && nameInputObject.activeSelf)
                CancelRenaming();

            // Check if the object is active and if the mouse is clicked
            if (!Input.GetMouseButtonDown(0) || !nameInputObject.activeSelf) return;

            if (EventSystem.current.currentSelectedGameObject != nameInputObject) ApplyRenaming();
        }

        /// <summary>
        ///     Scroll list view if dragging items to bottom/top
        /// </summary>
        private void ScrollOnDrag()
        {
            // Scroll at bottom 5% of screen
            if (Input.mousePosition.y < Screen.height * 0.05f)
            {
                // Stop at top of scroll rect
                if (scrollRect.normalizedPosition.y > 0.00001)
                    // Scroll down
                    scrollRect.normalizedPosition -= new Vector2(
                        0.0f,
                        GetScrollSpeedFromPosition(Math.Abs(Screen.height * 0.05f - Input.mousePosition.y))
                    );
            }
            // Scroll at top 20% of screen
            else if (Input.mousePosition.y > Screen.height * 0.8f)
            {
                // Stop at bottom of scroll rect
                if (scrollRect.normalizedPosition.y < 0.99999)
                    // Scroll down
                    scrollRect.normalizedPosition += new Vector2(
                        0.0f,
                        GetScrollSpeedFromPosition(
                            Math.Abs(Screen.height * 0.8f - Input.mousePosition.y)
                        )
                    );
            }
        }

        /// <summary>
        ///     Function to calculate dragging scroll speed
        /// </summary>
        /// <param name="val">Scroll intensity between [0, 70]</param>
        /// <returns>Scroll speed value</returns>
        private float GetScrollSpeedFromPosition(float val)
        {
            if (val >= 70)
                return 0.0005f;

            if (val <= 0)
                return 0.0001f;

            return (float) (0.0005 * Math.Pow(Math.E, -0.0008 * Math.Pow(val - 80, 2)));
        }

        /// <summary>
        ///     Initialize the hierarchy item
        /// </summary>
        /// <param name="modelItem">The item of the actual model</param>
        /// <param name="indentionDepth">Depth of indentation inside the listview</param>
        public void Initialize(GameObject modelItem, float indentionDepth)
        {
            // Save the actual item
            item = modelItem;
            itemInfo = item.GetComponent<ItemInfoController>();

            // set the name of the item
            nameText.text = itemInfo.ItemInfo.displayName;

            // indent the item
            IndentItem(indentionDepth);

            // Show the item
            ShowItem(true);

            // Add the double click detector
            doubleClickDetector.DoubleClickOccured += () =>
            {
                // Focus on component group only when there are children in group
                if (item.transform.childCount > 0)
                    cameraController.ZoomOnObject(item, false);

                // Focus on single component and make sure we have no empty group!
                else if (itemInfo.ItemInfo.isGroup == false)
                    cameraController.UpdateCameraFocus(item);
            };

            // Update the button
            UpdateVisuals();
        }

        /// <summary>
        ///     Change the text color of an item
        /// </summary>
        /// <param name="show">True if the item should be shown</param>
        public void ShowItem(bool show)
        {
            nameText.color = show ? visibleColor : invisibleColor;
            if (item == null) return;
            item.SetActive(show);
            if (itemInfo.ItemInfo.isFused) Utility.ToggleVisibility(childrenContainer.transform, show);
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
            if (itemInfo.ItemInfo.isGroup)
            {
                childrenContainer.SetActive(expand);
                _isExpanded = expand;
                _updateHierarchy = true;
            }

            UpdateVisuals();
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
        ///     Update the visuals of the item
        /// </summary>
        public void UpdateVisuals()
        {
            // Check if the item is fused
            var fused = itemInfo.ItemInfo.isFused;

            // Enable/Disable the button
            expandButton.SetActive(itemInfo.ItemInfo.isGroup || fused);

            // Update the logos if necessary (hide fusion if group is station)
            fused &= !IsStation;
            expandDown.SetActive(_isExpanded && !fused);
            expandRight.SetActive(!_isExpanded && !fused);
            fusion.SetActive(fused);

            // Show/Hide the station button
            showStation.SetActive(IsStation);

            // Hide dot icon
            itemActive.SetActive(false);
        }

        /// <summary>
        ///     Shows/hide the visualisation of an currently active item in the sequence view
        /// </summary>
        /// <param name="isActive"></param>
        public void SetItemActive(bool isActive)
        {
            // Skip if item is a station
            if (IsStation) return;

            // Show/hide dot icon
            itemActive.SetActive(isActive);
        }

        /// <summary>
        ///     Show a station in the station view
        /// </summary>
        public void ShowStation()
        {
            stationController.ShowStation(this);
        }

        /// <summary>
        ///     Handle clicks on the item
        /// </summary>
        /// <param name="data">The event data</param>
        public void ItemClick(BaseEventData data)
        {
            // Set the clicked flag
            _clicked = true;

            // Select the item 
            var pointerData = (PointerEventData) data;
            if (!hierarchyViewController.IsSelected(this))
            {
                SelectItem();
                _clicked = false;
            }

            // Check what type of click happened
            if (pointerData.button == PointerEventData.InputButton.Left) doubleClickDetector.Click();
        }

        /// <summary>
        ///     Handle click release
        /// </summary>
        /// <param name="data">The event data</param>
        public void ClickRelease(BaseEventData data)
        {
            var pointerData = (PointerEventData) data;
            switch (pointerData.button)
            {
                case PointerEventData.InputButton.Left when _clicked:
                    // Only perform selection if no double click occured before
                    if (doubleClickDetector.doubleClickOccured && hierarchyViewController.IsSelected(this)) break;
                    SelectItem();
                    break;
                case PointerEventData.InputButton.Right:
                    ShowContextMenu();
                    break;
            }

            doubleClickDetector.ClickRelease();

            _clicked = false;
        }

        /// <summary>
        ///     Open the context menu on right click
        /// </summary>
        private void ShowContextMenu()
        {
            var multiple = hierarchyViewController.GetSelectedItems().Count > 1;

            var entries = new List<ContextMenuController.Item>();

            var isGroup = itemInfo.ItemInfo.isGroup;
            if (!multiple)
            {
                entries.Add(
                    new ContextMenuController.Item
                    {
                        Icon = contextMenu.edit,
                        Name = "Rename",
                        Action = () => RenameItem()
                    }
                );

                var visible = item.activeSelf;
                entries.Add(
                    new ContextMenuController.Item
                    {
                        Icon = visible ? contextMenu.hide : contextMenu.show,
                        Name = visible ? "Hide Item" : "Show Item",
                        Action = () => ShowItem(!visible)
                    }
                );

                if (isGroup)
                    entries.Add(
                        new ContextMenuController.Item
                        {
                            Icon = contextMenu.show,
                            Name = "Show All",
                            Action = ShowGroup
                        }
                    );
            }

            if (hierarchyViewController.SelectedItems.Contains(this))
                entries.Add(
                    new ContextMenuController.Item
                    {
                        Icon = contextMenu.folder,
                        Name = "Group Selected",
                        Action = MoveToNewGroup
                    }
                );

            if (!multiple)
            {
                if (isGroup)
                {
                    if (!IsStation)
                        entries.Add(
                            new ContextMenuController.Item
                            {
                                Icon = itemInfo.ItemInfo.isFused ? contextMenu.defuse : contextMenu.fuse,
                                Name = itemInfo.ItemInfo.isFused ? "Split Group" : "Fuse Group",
                                Action = FuseGroup
                            }
                        );


                    entries.Add(
                        new ContextMenuController.Item
                        {
                            Icon = contextMenu.add,
                            Name = "Add Group",
                            Action = AddGroup
                        }
                    );

                    entries.Add(
                        new ContextMenuController.Item
                        {
                            Icon = contextMenu.delete,
                            Name = "Delete",
                            Action = DeleteGroup
                        }
                    );
                }

                if (stationController.IsOpen)
                    entries.Add(
                        new ContextMenuController.Item
                        {
                            Icon = contextMenu.skipTo,
                            Name = "Skip To",
                            Action = SequenceViewSkipToItem
                        }
                    );
            }

            contextMenu.Show(entries);
        }

        /// <summary>
        ///     Skips to the selected item in the sequence view.
        ///     Only skip inside the same assembly station.
        /// </summary>
        private void SequenceViewSkipToItem()
        {
            // Skip if station view not open
            if (!stationController.IsOpen) return;

            try
            {
                // Get station index of the currently (this) selected GameObject component
                var index = Utility.GetIndexForStation(stationController.station, item);

                stationController.sequenceController.SkipToItem(index);
            }
            catch (ComponentNotFoundException)
            {
                toast.Error(Toast.Short, "Could not skip to component!");
            }
        }

        /// <summary>
        ///     Start a renaming action
        /// </summary>
        public void RenameItem(bool isInitial = false, CommandGroup commandGroup = null)
        {
            _commandGroup = commandGroup;
            _isRenameInitial = isInitial;
            nameInput.text = nameText.text;
            showStation.SetActive(false);
            nameInputObject.SetActive(true);
            nameInput.Select();
            nameTextObject.SetActive(false);
        }

        /// <summary>
        ///     Fuse a group
        /// </summary>
        private void FuseGroup()
        {
            _undoService.AddCommand(new FuseCommand(itemInfo.ItemInfo.isFused, item.name));
        }

        /// <summary>
        ///     Delete a group
        /// </summary>
        private void DeleteGroup()
        {
            //Check if the group isn't empty
            if (item.transform.childCount > 0)
                toast.Error(Toast.Short, "Only empty groups can be deleted!");
            else
                // Add the new creation command to the undo redo service
                _undoService.AddCommand(new CreateCommand(false, new ItemState(this)));
        }

        /// <summary>
        ///     Show a whole group
        /// </summary>
        private void ShowGroup()
        {
            ShowItem(true);
            Utility.ToggleVisibility(childrenContainer.transform, true);
        }

        /// <summary>
        ///     Add new group
        /// </summary>
        private void AddGroup()
        {
            // Save the item state
            var state = new ItemState(
                _projectManager.GetNextGroupID(),
                "Group",
                item.name,
                ItemState.Last
            );

            // Add the new action to the undo redo service
            var createCommand = new CreateCommand(true, state);
            _commandGroup = new CommandGroup();
            _undoService.AddCommand(_commandGroup);
            _commandGroup.AddToGroup(createCommand);
            createCommand.Redo();

            // Scroll to the created group in the group
            var groupItem = childrenContainer.transform.GetChild(childrenContainer.transform.childCount - 1);
            hierarchyViewController.ScrollToItem(groupItem.GetComponent<RectTransform>());
            groupItem.GetComponent<HierarchyItemController>().RenameItem(true, _commandGroup);
        }

        /// <summary>
        ///     Create a new group and move the items into the group
        /// </summary>
        private void MoveToNewGroup()
        {
            // Create the group
            var state = new ItemState(
                _projectManager.GetNextGroupID(),
                "Group",
                item.transform.parent.name,
                Utility.GetNeighbourID(item.transform)
            );
            var createCommand = new CreateCommand(true, state);
            _commandGroup = new CommandGroup();
            _undoService.AddCommand(_commandGroup);
            _commandGroup.AddToGroup(createCommand);
            createCommand.Redo();

            // Move the items
            _dragItem =
                Utility.FindChild(hierarchyView.transform, state.ID).GetComponent<HierarchyItemController>();
            _insertion = true;
            _selectedItems = hierarchyViewController.GetSelectedItems();
            InsertItems();

            // Scroll to First selected Game object
            var groupItem = gameObject.transform.parent.parent;
            hierarchyViewController.ScrollToItem(groupItem.GetComponent<RectTransform>());
            groupItem.GetComponent<HierarchyItemController>().RenameItem(true, _commandGroup);
        }

        /// <summary>
        ///     Cancel a rename action
        /// </summary>
        private void CancelRenaming()
        {
            nameInputObject.SetActive(false);
            nameTextObject.SetActive(true);
        }

        /// <summary>
        ///     Apply a rename action
        /// </summary>
        private void ApplyRenaming()
        {
            // Check if there's a name given
            var newName = nameInput.text;
            if (newName == "")
            {
                toast.Error(Toast.Short, "Name cannot be empty!");
                
                // Reset name if empty
                nameInput.text = nameText.text;
                CancelRenaming();
                
                return;
            }

            // Hide the input field an show the name field
            nameInputObject.SetActive(false);
            nameTextObject.SetActive(true);
            showStation.SetActive(IsStation);

            // Check if nothing is changed
            if (nameInput.text == nameText.text) return;

            var renameCommand = new RenameCommand(item.name, nameText.text, newName);

            // Add the Command to the group if there is "Group Selected"
            if (_isRenameInitial)
            {
                _commandGroup.AddToGroup(renameCommand);
                renameCommand.Redo();
                _isRenameInitial = false;
            }
            else
            {
                _undoService.AddCommand(renameCommand);
            }
        }

        /// <summary>
        ///     Start dragging one or multiple items
        /// </summary>
        /// <param name="data">Event data</param>
        public void StartDraggingItem(BaseEventData data)
        {
            // Set the clicked flag to false
            _clicked = false;

            // Get the selected items
            _selectedItems = hierarchyViewController.GetSelectedItems();
            if (_selectedItems.Count == 0 || !_selectedItems.Contains(this)) return;

            // Show which items are dragged
            var firstName = itemInfo.ItemInfo.displayName;
            dragPreviewText.text = _selectedItems.Count > 1 ? "Multiple Items" : firstName;
            dragPreview.transform.position = ((PointerEventData) data).position;
            dragPreview.SetActive(true);
            Dragging = true;
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
            Dragging = false;
            _isItemShifted = true;
            // Insert the items (Only if the dragged item was selected)
            if (_selectedItems.Count != 0 && hierarchyViewController.IsSelected(this)) InsertItems();
        }

        /// <summary>
        ///     Insert the currently selected items into a group or put them above another item
        /// </summary>
        private void InsertItems()
        {
            // Hide the insertion area of the station view
            stationController.HideInsertionArea();

            // Check if the drag leads to a change
            if (_dragItem == null) return;

            // Get the new parent and the new neighbour id
            var parent = _insertion ? _dragItem.gameObject.name : _dragItem.item.transform.parent.name;
            var neighbourID = _insertion ? ItemState.Last : Utility.GetNeighbourID(_dragItem.transform);

            if (_selectedItems.Count == 1 && neighbourID == _selectedItems.First().name) return;

            // Create the item states
            List<ItemState> oldStates = new List<ItemState>(), newStates = new List<ItemState>();
            for (var i = 0; i < _selectedItems.Count; i++)
            {
                // Check whether a group is dragged into itself
                if (Utility.IsParent(_dragItem.item.transform, _selectedItems[i].item.name))
                {
                    toast.Error(Toast.Short, "Cannot make a group a child of its own!");
                    return;
                }

                // Create the old state
                oldStates.Add(new ItemState(_selectedItems[i]));

                // Create the new state
                newStates.Add(
                    new ItemState(oldStates[i]) {ParentID = parent, NeighbourID = neighbourID}
                );
                if (neighbourID != ItemState.Last) neighbourID = newStates[i].ID;
            }

            // Add the new action to the undo redo service
            var moveCommand = new MoveCommand(oldStates, newStates);
            if (_isItemShifted)
            {
                _undoService.AddCommand(moveCommand);
                _isItemShifted = false;
            }
            else
            {
                _commandGroup.AddToGroup(moveCommand);
                moveCommand.Redo();
            }
        }

        /// <summary>
        ///     Start hovering over the area to put an item above this item
        /// </summary>
        /// <param name="data">Event data</param>
        public void StartHoveringOverPutAboveArea(BaseEventData data)
        {
            // Change the color
            var selected = hierarchyViewController.IsSelected(this);
            background.color = Dragging && !selected ? normalColor : highlightedColor;

            // Show the moving indicator
            movingIndicator.SetActive(Dragging && !selected);

            // Highlight hovered object
            HighlightHover();

            // Save the item and the action
            _insertion = false;
            _dragItem = selected ? null : this;
        }

        /// <summary>
        ///     Stop hovering over the area to put an item above this item
        /// </summary>
        /// <param name="data">Event data</param>
        public void StopHoveringOverPutAboveArea(BaseEventData data)
        {
            movingIndicator.SetActive(false);
            if (!hierarchyViewController.IsSelected(this)) background.color = normalColor;
            _dragItem = null;
        }

        /// <summary>
        ///     Start hovering over the insertion area to insert another item into this item
        ///     (only works with if this item is a group)
        /// </summary>
        /// <param name="data">Event data</param>
        public void StartHoveringOverInsertingArea(BaseEventData data)
        {
            // Change the color
            var isGroup = itemInfo.ItemInfo.isGroup;
            var selected = hierarchyViewController.IsSelected(this);
            background.color = Dragging && !isGroup && !selected ? normalColor : highlightedColor;

            // Highlight hovered object
            HighlightHover();

            // Save the item and the action if item is compatible
            _insertion = true;
            _dragItem = isGroup ? this : null;
        }

        /// <summary>
        ///     Stop hovering over the insertion area of this item
        /// </summary>
        /// <param name="data">Event data</param>
        public void StopHoveringOverInsertingArea(BaseEventData data)
        {
            _dragItem = null;
            if (!hierarchyViewController.IsSelected(this)) background.color = normalColor;
        }

        /// <summary>
        ///     Highlights currently hovered items in the editor
        /// </summary>
        private void HighlightHover()
        {
            var parent = _projectManager.CurrentProject.ObjectModel.transform;
            var hoveredObject = Utility.FindChild(parent, name).gameObject;
            componentHighlighting.HighlightHoverFromList(hoveredObject);
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