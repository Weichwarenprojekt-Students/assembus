using System;
using MainScreen.Sidebar.HierarchyView;
using UnityEngine;
using Shared;
using TMPro;

namespace MainScreen.StationView
{
    public class SequenceController : MonoBehaviour
    {
        /// <summary>
        ///     The root of the hierarchy view
        /// </summary>
        public Transform stationView;

        /// <summary>
        ///     Station controller script handles station view
        /// </summary>
        private StationController _stationController;
        
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
        private int _currentIndex = 0;

        /// <summary>
        ///     Amount of items assigned to current station
        /// </summary>
        private int _numberOfItems = 0;
        
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
            
            // Update the shown current item index of the controls
            UpdateItemIndexText();

            
            // TODO: Outsource in methode
            // Set first element in hierarchy as active
            _stationController.station.childrenContainer.transform.GetChild(0).GetComponent<HierarchyItemController>().SetItemActive(true);
        }

        /// <summary>
        ///    Show previous item
        /// </summary>
        public void PreviousItem()
        {
            // Skip if no previous item available
            if (_currentIndex <= 0)  return;

            // Hide dot icon on current item
            _stationController.station.childrenContainer.transform.GetChild(_currentIndex)
                .GetComponent<HierarchyItemController>().SetItemActive(false);
            // Decrement current index
            _currentIndex--;

            // Show dot icon on previous item
            _stationController.station.childrenContainer.transform.GetChild(_currentIndex).GetComponent<HierarchyItemController>()
                .SetItemActive(true);

        }

        /// <summary>
        ///     Show next item
        /// </summary>
        public void NextItem()
        {
            // Skip if no further item available
            if (_currentIndex >= _numberOfItems-1) return;

            // Hide dot icon on current item
            _stationController.station.childrenContainer.transform.GetChild(_currentIndex).GetComponent<HierarchyItemController>()
                .SetItemActive(false);
                
            // Increment current index
            _currentIndex++;

            // Show dot icon on next item
            _stationController.station.childrenContainer.transform.GetChild(_currentIndex)
                .GetComponent<HierarchyItemController>().SetItemActive(true);

        }

        /// <summary>
        ///     Show last item
        /// </summary>
        public void SkipToLastItem()
        {
            
        }

        /// <summary>
        ///     Show first item
        /// </summary>
        public void SkipToFirstItem()
        {
            
        }

        /// <summary>
        ///     Shows the current item number in the controls
        /// </summary>
        public void UpdateItemIndexText()
        {
            itemIndexText.SetText((_currentIndex+1) + " / " + _numberOfItems);
        }
    }
}