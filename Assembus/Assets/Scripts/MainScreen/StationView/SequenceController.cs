using System.Collections.Generic;
using MainScreen.Sidebar.HierarchyView;
using Services.UndoRedo.Commands;
using Shared;
using Shared.Exceptions;
using TMPro;
using UnityEngine;

namespace MainScreen.StationView
{
    public class SequenceController : MonoBehaviour
    {
        /// <summary>
        ///     The navigation buttons
        /// </summary>
        public SwitchableButton previousButton, nextButton, skipFirstButton, skipLastButton;

        /// <summary>
        ///     Text to show current item and number of items
        /// </summary>
        public TextMeshProUGUI itemIndexText;

        /// <summary>
        ///     Index of current item
        /// </summary>
        private int _currentIndex;

        /// <summary>
        ///     Leaves of the component group
        /// </summary>
        private List<HierarchyItemController> _itemList;

        /// <summary>
        ///     Amount of items assigned to current station
        /// </summary>
        private int _numberOfItems;

        /// <summary>
        ///     Stores the previously selected GameObject item
        /// </summary>
        private GameObject _prevItem;

        /// <summary>
        ///     The hierarchy item controller
        /// </summary>
        private HierarchyItemController _station;

        /// <summary>
        ///     Method to react on changing stations
        /// </summary>
        public void ActionStationUpdate(HierarchyItemController station, Command command)
        {
            if (command == null) // No command executed. Init/switch station occured --> Init StationView!
            {
                // Reset item dot indicator
                OnStationLeave();

                // Get the hierarchy item controller reference
                _station = station;

                // Extract all leaves from the children container
                _itemList = Utility.GetAllComponents(station.childrenContainer);
                _numberOfItems = _itemList.Count;

                // Jump to the end of the current station
                _currentIndex = _numberOfItems - 1;
                SkipToItem(_currentIndex);
            }
            else // Action has been performed --> Update StationView!
            {
                // Load the updated children container of the station into the item list
                _itemList = Utility.GetAllComponents(station.childrenContainer);
                _numberOfItems = _itemList.Count;

                // Find out which Undo/Redo-Command has been executed
                var commandType = command.GetType();
                if (commandType == typeof(FuseCommand)) // Made group fused
                {
                    var partOfFuse = true;
                    for (var i = 0; i < _itemList.Count; i++)
                        if (_itemList[i].item == _prevItem)
                        {
                            partOfFuse = false;
                            break;
                        }

                    if (partOfFuse)
                    {
                        var fuseCommand = (FuseCommand) command;

                        // Find fused group in children container
                        var groupIndex = -1;
                        for (var i = 0; i < _itemList.Count; i++)
                            if (_itemList[i].item.name == fuseCommand.ID)
                            {
                                groupIndex = i;
                                break;
                            }

                        if (fuseCommand.IsFused == false) // Previously not fused --> Fuse
                        {
                            // Jump on fused group
                            SetActiveHierarchyItem(_currentIndex, false); // TODO: Why required?
                            SkipToItem(groupIndex);
                        }
                        else // Previously fused --> Unfuse
                        {
                            if (groupIndex + 1 < _itemList.Count)
                                SkipToItem(groupIndex + 1);
                        }
                    }
                }
                else if (commandType == typeof(CommandGroup)) // Moved selected components to newly created group
                {
                    var newIndex = Utility.GetIndexForStation(_station, _prevItem);
                    SkipToItem(newIndex);
                }
                else if (commandType == typeof(MoveCommand)) // Only moved components
                {
                    var moveCommand = (MoveCommand) command;

                    if (moveCommand.ContainsItem(_prevItem.name)) // Currently selected item part of move command
                    {
                        try
                        {
                            // Check if the selected item is still in the station
                            Utility.GetIndexForStation(_station, _prevItem);
                            SkipToItem(_currentIndex); // Remain at the current position
                        }
                        catch (ComponentNotFoundException ex)
                        {
                            // Moved selected item out of station
                            SkipToItem(0);
                            //TODO: Implement better skip
                        }
                    }
                    else // Other item, not the currently selected one, is moved
                    {
                        var newIndex = Utility.GetIndexForStation(_station, _prevItem);
                        SkipToItem(newIndex);
                    }
                }
            }
        }

        /// <summary>
        ///     Method to react to closing the station
        /// </summary>
        public void OnStationLeave()
        {
            if (_station != null)
                // Hide the item dot when leaving SequenceView
                SetActiveHierarchyItem(_currentIndex, false);
        }

        /// <summary>
        ///     Shows or hides the hierarchy item indicator dot at given index
        /// </summary>
        /// <param name="index">Position of the item in the hierarchy</param>
        /// <param name="visible">Visibility of the dot icon</param>
        private void SetActiveHierarchyItem(int index, bool visible)
        {
            if (_itemList.Count > 0)
                _itemList[index].SetItemActive(visible);
        }

        /// <summary>
        ///     Change visibility of an item in the 3D editor
        /// </summary>
        /// <param name="index">Index of item</param>
        /// <param name="visible">Visibility of item</param>
        private void SetItemVisibility(int index, bool visible)
        {
            if (_itemList.Count > 0)
                _itemList[index].ShowItem(visible);
        }

        /// <summary>
        ///     Jump to an item of the station
        /// </summary>
        /// <param name="index">The index of the item</param>
        public void SkipToItem(int index)
        {
            // If there are no components, disable all buttons and set display text accordingly
            if (_itemList.Count < 1)
            {
                previousButton.Enable(false);
                skipFirstButton.Enable(false);
                nextButton.Enable(false);
                skipLastButton.Enable(false);

                // Update the shown current item index of the controls
                UpdateItemIndexText();
            }
            else // At least one component available
            {
                // Update the control visibility
                if (index == 0) // Start of list
                {
                    previousButton.Enable(false);
                    skipFirstButton.Enable(false);

                    nextButton.Enable(true);
                    skipLastButton.Enable(true);
                }
                else if (index == _numberOfItems - 1) // End of list
                {
                    nextButton.Enable(false);
                    skipLastButton.Enable(false);

                    previousButton.Enable(true);
                    skipFirstButton.Enable(true);
                }
                else // In-between start and end
                {
                    previousButton.Enable(true);
                    nextButton.Enable(true);
                    skipFirstButton.Enable(true);
                    skipLastButton.Enable(true);
                }

                if (index < 0 || index >= _numberOfItems) return;

                // Hide dot icon on current item
                SetActiveHierarchyItem(_currentIndex, false);

                // Set all items succeeding the index to invisible
                for (var i = _numberOfItems - 1; i > index; i--)
                    SetItemVisibility(i, false);

                // Set all items to visible until index
                for (var i = 0; i <= index; i++)
                    SetItemVisibility(i, true);

                _currentIndex = index;

                // Remember the previously selected GameObject
                _prevItem = _itemList[_currentIndex].item;

                // Show dot icon on current index
                SetActiveHierarchyItem(_currentIndex, true);

                // Update the shown current item index of the controls
                UpdateItemIndexText();
            }
        }

        /// <summary>
        ///     Show next item
        /// </summary>
        public void NextItem()
        {
            SkipToItem(_currentIndex + 1);
        }

        /// <summary>
        ///     Show previous item
        /// </summary>
        public void PreviousItem()
        {
            SkipToItem(_currentIndex - 1);
        }

        /// <summary>
        ///     Show last item
        /// </summary>
        public void SkipToLastItem()
        {
            SkipToItem(_numberOfItems - 1);
        }

        /// <summary>
        ///     Show first item
        /// </summary>
        public void SkipToFirstItem()
        {
            SkipToItem(0);
        }

        /// <summary>
        ///     Shows the current item number in the controls
        /// </summary>
        private void UpdateItemIndexText()
        {
            itemIndexText.SetText(_currentIndex + 1 + " / " + _numberOfItems);
        }
    }
}