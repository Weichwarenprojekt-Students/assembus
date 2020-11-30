using MainScreen.Sidebar.HierarchyView;
using Shared;
using Shared.Toast;
using TMPro;
using UnityEngine;

namespace MainScreen.StationView
{
    /// <summary>
    ///     Delegate station update
    /// </summary>
    public delegate void Notify(); 
    
    public class StationController : MonoBehaviour
    {
        /// <summary>
        ///     The title of the station
        /// </summary>
        public TextMeshProUGUI title;

        /// <summary>
        ///     The navigation buttons
        /// </summary>
        public SwitchableButton previousButton, nextButton;

        /// <summary>
        ///     The root of the hierarchy view
        /// </summary>
        public Transform hierarchyView;

        /// <summary>
        ///     The toast controller
        /// </summary>
        public ToastController toast;

        /// <summary>
        ///     The current station
        /// </summary>
        public HierarchyItemController station;

        /// <summary>
        ///     True if there's a previous station
        /// </summary>
        private bool HasPrevious => station.transform.GetSiblingIndex() > 1;

        /// <summary>
        ///     True if there's a next station
        /// </summary>
        private bool HasNext => station.hierarchyView.childCount > station.transform.GetSiblingIndex() + 1;

        /// <summary>
        ///     Delegate which gets called on station update
        /// </summary>
        public event Notify StationUpdateOccured; 
        
        /// <summary>
        ///     Open the station view
        /// </summary>
        /// <param name="shownStation">The station to be shown</param>
        public void ShowStation(HierarchyItemController shownStation)
        {
            // Show the station view
            station = shownStation;
            gameObject.SetActive(true);

            // Set the name of the station
            title.text = shownStation.itemInfo.ItemInfo.displayName;

            // Update the station view
            UpdateStation();
        }

        /// <summary>
        ///     Update the station view
        /// </summary>
        public void UpdateStation()
        {
            Debug.Log("update station");
            
            // Check if the station is null
            if (station == null) return;

            // Check if the station still is a station
            if (!station.IsStation)
            {
                CloseStation();
                return;
            }

            // Only show the items that belong to the station
            Utility.ToggleVisibility(hierarchyView, false);
            station.ShowItem(true);
            Utility.ToggleVisibility(station.childrenContainer.transform, true);

            // Update the navigation buttons
            previousButton.Enable(HasPrevious);
            nextButton.Enable(HasNext);
            
            // Trigger station change event
            StationUpdateOccured?.Invoke();
        }

        /// <summary>
        ///     Go to the previous station
        /// </summary>
        public void PreviousStation()
        {
            ChangeStation(-1);
        }

        /// <summary>
        ///     Go to the next station
        /// </summary>
        public void NextStation()
        {
            ChangeStation(1);
        }

        /// <summary>
        ///     Change the station by a given offset
        /// </summary>
        /// <param name="offset">The offset to the next station</param>
        private void ChangeStation(int offset)
        {
            try
            {
                var newStation = station.hierarchyView.GetChild(station.transform.GetSiblingIndex() + offset);
                ShowStation(newStation.GetComponent<HierarchyItemController>());
            }
            catch
            {
                toast.Error(Toast.Short, "Couldn't change station!");
            }
        }

        /// <summary>
        ///     Close the station view
        /// </summary>
        public void CloseStation()
        {
            gameObject.SetActive(false);
            station = null;
            Utility.ToggleVisibility(hierarchyView, true);
        }
    }
}