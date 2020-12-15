using System.Collections.Generic;
using MainScreen.Sidebar.HierarchyView;
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
        ///     Amount of items assigned to current station
        /// </summary>
        private int _numberOfItems;

        /// <summary>
        ///     The hierarchy item controller
        /// </summary>
        private HierarchyItemController _station;


        /// <summary>
        ///     Method to react on changing stations
        /// </summary>
        public void OnStationUpdate(HierarchyItemController station)
        {
            // Reset item dot indicator
            OnStationLeave();

            // Get the hierarchy item controller reference
            _station = station;

            // Extract all leaves from the children container
            _itemList = Utility.GetAllComponents(station.childrenContainer);

            _numberOfItems = _itemList.Count;

            // Hide all items of the component group
            for (var i = 1; i < _itemList.Count; i++) SetItemVisibility(i, false);

            _currentIndex = 0;

            SkipToItem(0);
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

                // Set items from current index to new index to invisible
                for (var i = _currentIndex; i > index; i--)
                    SetItemVisibility(i, false);

                // Set items visible from current index to index
                for (var i = _currentIndex; i <= index; i++)
                    SetItemVisibility(i, true);

                _currentIndex = index;

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