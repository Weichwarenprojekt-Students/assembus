using MainScreen.Sidebar.HierarchyView;
using Shared;
using Shared.Toast;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MainScreen.StationView
{
    public class StationController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
        ///     The buttons for hiding/showing previous stations
        /// </summary>
        public GameObject showPreviousButton, hidePreviousButton;

        /// <summary>
        ///     The root of the hierarchy view
        /// </summary>
        public Transform hierarchyView;

        /// <summary>
        ///     The toast controller
        /// </summary>
        public ToastController toast;

        /// <summary>
        ///     Field to pull items into stations
        /// </summary>
        public GameObject insertionArea;

        /// <summary>
        ///     The sequence controller
        /// </summary>
        public SequenceController sequenceController;

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
        ///     True if station view is open
        /// </summary>
        public bool IsOpen => station != null;

        /// <summary>
        ///     Add selected items to this station
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!HierarchyItemController.Dragging) return;
            insertionArea.SetActive(true);
            station.StartHoveringOverInsertingArea(null);
        }

        /// <summary>
        ///     Stop adding selected items to this station
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            insertionArea.SetActive(false);
            station.StopHoveringOverInsertingArea(null);
        }

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

            // Update the sequence view controller
            sequenceController.OnStationUpdate(station);

            // Update the other toolbar buttons
            hidePreviousButton.SetActive(false);
            showPreviousButton.SetActive(true);
        }

        /// <summary>
        ///     Hide the insertion area
        /// </summary>
        public void HideInsertionArea()
        {
            insertionArea.SetActive(false);
        }

        /// <summary>
        ///     Show the items of the previous stations
        /// </summary>
        public void ShowPreviousStations()
        {
            ChangePreviousStations(true);
        }

        /// <summary>
        ///     Hide the items of the previous stations
        /// </summary>
        public void HidePreviousStations()
        {
            ChangePreviousStations(false);
        }

        /// <summary>
        ///     Change the visibility of the previous stations
        /// </summary>
        /// <param name="visible">True if the stations shall be visible</param>
        private void ChangePreviousStations(bool visible)
        {
            for (var i = 0; i < station.transform.GetSiblingIndex(); i++)
            {
                var nextStation = station.hierarchyView.GetChild(i).GetComponent<HierarchyItemController>();
                Utility.ToggleVisibility(nextStation.childrenContainer.transform, visible);
                nextStation.ShowItem(visible);
            }

            hidePreviousButton.SetActive(visible);
            showPreviousButton.SetActive(!visible);
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

            // Close the SequenceView
            sequenceController.OnStationLeave();
        }
    }
}