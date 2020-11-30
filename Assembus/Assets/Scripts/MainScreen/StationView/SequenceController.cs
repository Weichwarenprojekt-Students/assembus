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
            // Get the hierarchy item controller reference
            _station = station;

            // Extract all leaves from the children container
            _itemList = Utility.GetAllComponents(station.childrenContainer);

            // Get number of items assigned to current station
            _numberOfItems = _itemList.Count;
            
            _currentIndex = 0;

            // Set first element in hierarchy as active
            SetActiveHierarchyItem(_currentIndex, true);

            // Hide all items of the component group
            for (var i = 1; i < _itemList.Count; i++)
                SetItemVisibility(i, false);

            // Update the shown current item index of the controls
            UpdateItemIndexText();
        }

        /// <summary>
        ///     Shows or hides the hierarchy item indicator dot at given index
        /// </summary>
        /// <param name="index">Position of the item in the hierarchy</param>
        /// <param name="visible">Visibility of the dot icon</param>
        private void SetActiveHierarchyItem(int index, bool visible)
        {
            _itemList[index].SetItemActive(visible);
        }

        /// <summary>
        ///     Change visibility of a item in the 3D editor
        /// </summary>
        /// <param name="index">Index of item</param>
        /// <param name="visible">Visibility of item</param>
        private void SetItemVisibility(int index, bool visible)
        {
            _itemList[index].ShowItem(visible);
        }

        /// <summary>
        ///     Show previous item
        /// </summary>
        public void PreviousItem()
        {
            // Skip if no previous item available
            if (_currentIndex <= 0) return;

            // Hide dot icon on current item
            SetActiveHierarchyItem(_currentIndex, false);

            // Show the current item in the 3D editor
            SetItemVisibility(_currentIndex, false);

            _currentIndex--;

            // Show dot icon on previous item
            SetActiveHierarchyItem(_currentIndex, true);

            // Update the shown current item index of the controls
            UpdateItemIndexText();
        }

        /// <summary>
        ///     Show next item
        /// </summary>
        public void NextItem()
        {
            // Skip if no further item available
            if (_currentIndex >= _numberOfItems - 1) return;

            // Hide dot icon on current item
            SetActiveHierarchyItem(_currentIndex, false);
            
            _currentIndex++;

            // Show dot icon on next item
            SetActiveHierarchyItem(_currentIndex, true);

            // Show the current item in the 3D editor
            SetItemVisibility(_currentIndex, true);

            // Update the shown current item index of the controls
            UpdateItemIndexText();
        }

        /// <summary>
        ///     Show last item
        /// </summary>
        public void SkipToLastItem()
        {
            // Hide dot icon on current item
            SetActiveHierarchyItem(_currentIndex, false);

            // Set all items to visible
            for (int i = _currentIndex; i < _numberOfItems; i++) 
                SetItemVisibility(i, true);
            
            // Set current index to last item
            _currentIndex = _numberOfItems - 1;

            // Show dot icon on next item
            SetActiveHierarchyItem(_currentIndex, true);
            
            // Update the shown current item index of the controls
            UpdateItemIndexText();
        }

        /// <summary>
        ///     Show first item
        /// </summary>
        public void SkipToFirstItem()
        {
            // Hide dot icon on current item
            SetActiveHierarchyItem(_currentIndex, false);

            // Set all items to not visible
            for (int i = _currentIndex; i > 0; i--)
                SetItemVisibility(i, false);
            
            // Set current index to last item
            _currentIndex = 0;

            // Show dot icon on next item
            SetActiveHierarchyItem(_currentIndex, true);

            // Update the shown current item index of the controls
            UpdateItemIndexText();
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