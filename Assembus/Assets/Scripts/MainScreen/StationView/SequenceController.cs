using MainScreen.Sidebar.HierarchyView;
using Shared;
using TMPro;
using UnityEngine;

namespace MainScreen.StationView
{
    public class SequenceController : MonoBehaviour
    {
        /// <summary>
        ///     The root of the hierarchy view
        /// </summary>
        public Transform stationView;

        /// <summary>
        ///     The navigation buttons
        /// </summary>
        public SwitchableButton previousButton, nextButton, skipToStartButton, skipToEndButton;

        /// <summary>
        ///     Text to show current item and number of items
        /// </summary>
        public TextMeshProUGUI itemIndexText;

        /// <summary>
        ///     Index of current item
        /// </summary>
        private int _currentIndex;

        /// <summary>
        ///     Amount of items assigned to current station
        /// </summary>
        private int _numberOfItems;

        /// <summary>
        ///     Station controller script handles station view
        /// </summary>
        private StationController _stationController;

        /// <summary>
        ///     Override start method to get the station controller on initialization
        /// </summary>
        public void Start()
        {
            // Get the StationController component assigned to the game object station view
            _stationController = stationView.GetComponent<StationController>();

            // Register event on station update
            _stationController.StationUpdateOccured += OnStationUpdate;

            // Trigger station update on open as event not registered before
            OnStationUpdate();
        }

        /// <summary>
        ///     Method to react on changing stations
        /// </summary>
        public void OnStationUpdate()
        {
            // Get number of items assigned to current station
            _numberOfItems = _stationController.station.childrenContainer.transform.childCount;
            
            // Reset current item index to 0
            _currentIndex = 0;
            
            // Set first element in hierarchy as active
            setActiveHierarchyItem(_currentIndex, true);
            
            // Update the shown current item index of the controls
            UpdateItemIndexText();
        }

        /// <summary>
        ///     Shows or hides the hierarchy item indicator dot at given index
        /// </summary>
        /// <param name="idx">Position of the item in the hierarchy</param>
        /// <param name="visible">Visibility of the dot icon</param>
        private void setActiveHierarchyItem(int idx, bool visible)
        {
            _stationController.station.childrenContainer.transform.GetChild(idx).GetComponent<HierarchyItemController>()
                .SetItemActive(visible);
        }

        /// <summary>
        ///     Show previous item
        /// </summary>
        public void PreviousItem()
        {
            // Skip if no previous item available
            if (_currentIndex <= 0) return;

            // Hide dot icon on current item
            setActiveHierarchyItem(_currentIndex, false);
            
            // Decrement current index
            _currentIndex--;

            // Show dot icon on previous item
            setActiveHierarchyItem(_currentIndex, true);

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
            setActiveHierarchyItem(_currentIndex, false);

            // Increment current index
            _currentIndex++;

            // Show dot icon on next item
            setActiveHierarchyItem(_currentIndex, true);
            
            // Update the shown current item index of the controls
            UpdateItemIndexText();
        }

        /// <summary>
        ///     Show last item
        /// </summary>
        public void SkipToLastItem()
        {
            // Hide dot icon on current item
            setActiveHierarchyItem(_currentIndex, false);
            
            // Set current index to last item
            _currentIndex = _numberOfItems - 1;
            
            // Show dot icon on next item
            setActiveHierarchyItem(_currentIndex, true);
            
            // Update the shown current item index of the controls
            UpdateItemIndexText();
        }

        /// <summary>
        ///     Show first item
        /// </summary>
        public void SkipToFirstItem()
        {
            // Hide dot icon on current item
            setActiveHierarchyItem(_currentIndex, false);
            
            // Set current index to last item
            _currentIndex = 0;
            
            // Show dot icon on next item
            setActiveHierarchyItem(_currentIndex, true);
            
            // Update the shown current item index of the controls
            UpdateItemIndexText();
        }

        /// <summary>
        ///     Shows the current item number in the controls
        /// </summary>
        public void UpdateItemIndexText()
        {
            itemIndexText.SetText(_currentIndex + 1 + " / " + _numberOfItems);
        }
    }
}