using System;
using System.Collections.Generic;
using MainScreen.Sidebar.HierarchyView;
using Services.UndoRedo.Commands;
using Shared;
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
        ///     The currently active item
        /// </summary>
        private HierarchyItemController _activeItem;

        /// <summary>
        ///     The hierarchy item controller
        /// </summary>
        private HierarchyItemController _station;

        /// <summary>
        ///     Method to react on changing stations
        /// </summary>
        public void ActionStationUpdate(HierarchyItemController station, Command command)
        {
            // Load the updated children container of the station into the item list
            _itemList = Utility.GetAllComponents(station.childrenContainer);
            
            // No command executed. Init/switch station occured --> Init StationView!
            if (command == null) 
            {
                // Reset item dot indicator
                HideActiveItem();

                // Get the hierarchy item controller reference
                _station = station;

                // Jump to the end of the current station
                _currentIndex = _itemList.Count - 1;
                SkipToItem(_currentIndex);
            }
            // Action has been performed --> Update StationView!
            else 
            {
                var commandType = command.GetType();
                
                // Find out the right command and react to it
                if (commandType == typeof(FuseCommand)) ReactToFuse((FuseCommand) command);
                else if (commandType == typeof(MoveCommand)) ReactToMove((MoveCommand) command);
                else SkipToActiveItem();
            }
        }

        /// <summary>
        ///     React to fuse command
        /// </summary>
        /// <param name="command">The fuse command</param>
        private void ReactToFuse(FuseCommand command)
        {
            // Check if the active item was included
            var fusedItemIndex = _itemList.Count;
            var activeItemIndex = -1;
            for (var i = 0; i < _itemList.Count; i++)
            {
                if (_itemList[i].name == _activeItem.name)
                {
                    activeItemIndex = i;
                    break;
                }
                if (_itemList[i].name == command.ID) fusedItemIndex = i;
            }
            
            // Skip to the right item
            if (activeItemIndex >= 0) SkipToItem(activeItemIndex); 
            else if (command.IsFused) SkipToItem(_currentIndex);
            else SkipToItem(fusedItemIndex);
        }

        /// <summary>
        ///     React to a move command
        /// </summary>
        /// <param name="command">The move command</param>
        private void ReactToMove(MoveCommand command)
        {
            var activeName = _activeItem == null ? "" : _activeItem.name;
            if (command.ContainsItem(activeName)) SkipToItem(_currentIndex);
            else SkipToActiveItem();
        }

        /// <summary>
        ///     Try to skip to the active item
        /// </summary>
        private void SkipToActiveItem()
        {
            try
            {
                var newIndex = Utility.GetIndexForStation(_station, _activeItem.item);
                SkipToItem(newIndex);
            }
            catch (Exception)
            {
                SkipToItem(_itemList.Count - 1);
            }
        }
        
        /// <summary>
        ///     Hide the current active item
        /// </summary>
        public void HideActiveItem()
        {
            if (_activeItem != null) _activeItem.GetComponent<HierarchyItemController>().SetItemActive(false);
        }

        /// <summary>
        ///     Shows or hides the hierarchy item indicator dot at given index
        /// </summary>
        /// <param name="index">Position of the item in the hierarchy</param>
        /// <param name="visible">Visibility of the dot icon</param>
        private void SetActiveHierarchyItem(int index, bool visible)
        {
            if (_itemList.Count > 0) _itemList[index].SetItemActive(visible);
        }

        /// <summary>
        ///     Change visibility of an item in the 3D editor
        /// </summary>
        /// <param name="index">Index of item</param>
        /// <param name="visible">Visibility of item</param>
        private void SetItemVisibility(int index, bool visible)
        {
            if (_itemList.Count > 0) _itemList[index].ShowItem(visible);
        }

        /// <summary>
        ///     Jump to an item of the station
        /// </summary>
        /// <param name="index">The index of the item</param>
        public void SkipToItem(int index)
        {
            // Make sure the index is in the bounds
            index = Mathf.Clamp(index, 0, _itemList.Count - 1);
            
            // Update the control visibility
            previousButton.Enable(index > 0);
            nextButton.Enable(index < _itemList.Count - 1);
            skipFirstButton.Enable(index > 0);
            skipLastButton.Enable(index < _itemList.Count - 1);

            // Hide old items and switch to the next
            HideActiveItem();
            for (var i = _itemList.Count - 1; i > index; i--) SetItemVisibility(i, false);
            for (var i = 0; i <= index; i++) SetItemVisibility(i, true);
            _currentIndex = index;
            if (_itemList.Count > 0) _activeItem = _itemList[_currentIndex];
            else _currentIndex = -1;

            // Show the new item
            SetActiveHierarchyItem(_currentIndex, true);
            UpdateItemIndexText();
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
            SkipToItem(_itemList.Count - 1);
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
            itemIndexText.SetText(_currentIndex + 1 + " / " + _itemList.Count);
        }
    }
}