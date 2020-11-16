using System.Collections.Generic;
using System.Linq;
using Models.Project;
using Services;
using Services.UndoRedo;
using Shared.Toast;
using UnityEngine;

namespace MainScreen.Sidebar.HierarchyView
{
    public class HierarchyViewController : MonoBehaviour
    {
        /// <summary>
        ///     The component highlighting
        /// </summary>
        public ComponentHighlighting componentHighlighting;

        /// <summary>
        ///     The toast controller
        /// </summary>
        public ToastController toast;

        /// <summary>
        ///     The colors for the item
        /// </summary>
        public Color selectedColor, normalColor;

        /// <summary>
        ///     The root element of the hierarchy view
        /// </summary>
        public Transform rootView;

        /// <summary>
        ///     The context menu controller
        /// </summary>
        public ContextMenuController contextMenu;

        /// <summary>
        ///     The project manager
        /// </summary>
        private readonly ProjectManager _projectManager = ProjectManager.Instance;

        /// <summary>
        ///     A list of the GameObject in Insertion Order
        /// </summary>
        public readonly List<HierarchyItemController> SelectedItems = new List<HierarchyItemController>();

        /// <summary>
        ///     The item selected before the currently selected one
        /// </summary>
        private HierarchyItemController _lastSelectedItem;

        /// <summary>
        ///     The undo redo service
        /// </summary>
        private readonly UndoService _undoService = UndoService.Instance;

        /// <summary>
        ///     Clear the selections
        /// </summary>
        private void OnEnable()
        {
            SelectedItems.Clear();
        }

        /// <summary>
        ///     Open the context menu for the list view
        /// </summary>
        public void ShowContextMenu()
        {
            var createStation = new ContextMenuController.Item
            {
                Icon = contextMenu.add,
                Name = "Create Station",
                Action = CreateAssemblyStation
            };
            var showAll = new ContextMenuController.Item
            {
                Icon = contextMenu.show,
                Name = "Show All",
                Action = () => SetObjectVisibility(true)
            };
            var hideAll = new ContextMenuController.Item
            {
                Icon = contextMenu.hide,
                Name = "Hide All",
                Action = () => SetObjectVisibility(false)
            };
            contextMenu.Show(new[] {createStation, showAll, hideAll});
        }

        /// <summary>
        ///     Create an assembly station
        /// </summary>
        private void CreateAssemblyStation()
        {
            var oldState = new[]{new ItemState(
                _projectManager.GetNextGroupID(),
                "Assembly Station",
                _projectManager.CurrentProject.ObjectModel.name,
                _projectManager.CurrentProject.ObjectModel.transform.childCount
            )};
            var newState = new[]{new ItemState(oldState[0])};
            _undoService.AddCommand(new Command(newState, oldState, Command.Create));
        }

        /// <summary>
        ///     Show or hide the model
        /// </summary>
        /// <param name="visible">True if the model shall be shown</param>
        private void SetObjectVisibility(bool visible)
        {
            Utility.ToggleVisibility(_projectManager.CurrentProject.ObjectModel.transform, visible);
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
                SelectItem(Utility.FindChild(rootView, item).GetComponent<HierarchyItemController>());
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

        public bool Contains(HierarchyItemController item)
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
                    _lastSelectedItem = item;
                    break;
                default:
                    NoModSelection(item);
                    _lastSelectedItem = item;
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
            DeselectItems();
            SelectedItems.Add(item);
            SetColor(item, true);
        }

        /// <summary>
        ///     Adds one item to the currently selected items or removes one of the item is already selected
        /// </summary>
        /// <param name="item">the selected item</param>
        private void ControlSelection(HierarchyItemController item)
        {
            var isGroup = item.item.GetComponent<ItemInfoController>().ItemInfo.isGroup;
            if (isGroup) DeselectChildren(item.childrenContainer.transform);
            else DeselectParent(item.transform);

            var selected = SelectedItems.Contains(item);
            if (selected) SelectedItems.Remove(item);
            else SelectedItems.Add(item);
            SetColor(item, !selected);
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
    }
}