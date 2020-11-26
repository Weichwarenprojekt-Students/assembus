using MainScreen.Sidebar.HierarchyView;
using Shared;
using Shared.Toast;
using TMPro;
using UnityEngine;

namespace MainScreen.StationView
{
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
        private HierarchyItemController _station;

        /// <summary>
        ///     True if there's a previous station
        /// </summary>
        private bool HasPrevious => _station.transform.GetSiblingIndex() > 1;

        /// <summary>
        ///     True if there's a next station
        /// </summary>
        private bool HasNext => _station.hierarchyView.childCount > _station.transform.GetSiblingIndex() + 1;

        /// <summary>
        ///     Open the station view
        /// </summary>
        /// <param name="station">The station to be shown</param>
        public void ShowStation(HierarchyItemController station)
        {
            // Show the station view
            _station = station;
            gameObject.SetActive(true);

            // Set the name of the station
            title.text = station.itemInfo.ItemInfo.displayName;

            // Update the station view
            UpdateStation();
        }

        /// <summary>
        ///     Update the station view
        /// </summary>
        public void UpdateStation()
        {
            // Check if the station is null
            if (_station == null) return;

            // Check if the station still is a station
            if (!_station.IsStation)
            {
                CloseStation();
                return;
            }

            // Only show the items that belong to the station
            Utility.ToggleVisibility(hierarchyView, false);
            _station.ShowItem(true);
            Utility.ToggleVisibility(_station.childrenContainer.transform, true);

            // Update the navigation buttons
            previousButton.Enable(HasPrevious);
            nextButton.Enable(HasNext);
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
                var newStation = _station.hierarchyView.GetChild(_station.transform.GetSiblingIndex() + offset);
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
            _station = null;
            Utility.ToggleVisibility(hierarchyView, true);
        }
    }
}