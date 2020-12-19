using System;
using System.Collections.Generic;
using MainScreen.Sidebar.HierarchyView;
using Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainScreen.Sidebar
{
    public class InputController : MonoBehaviour
    {
        /// <summary>
        ///     Reference to HierarchyViewController to skip to items in the list
        /// </summary>
        public HierarchyViewController hierarchyViewController;

        /// <summary>
        ///     Input in the search bar
        /// </summary>
        public TMP_InputField userInput;

        /// <summary>
        ///     Text Field that indicates the amount of results
        /// </summary>
        public TMP_Text textAmountResults;

        /// <summary>
        ///     Button to skip to the next result
        /// </summary>
        public Button nextSearch;

        /// <summary>
        ///     Button to go back to the previous result
        /// </summary>
        public Button previousSearch;

        /// <summary>
        ///     Button to search for given item name in the hierarchy list
        /// </summary>
        public Button searchButton;

        /// <summary>
        ///     List items that have been found in the search process
        /// </summary>
        private readonly List<GameObject> _foundObjects = new List<GameObject>();

        /// <summary>
        ///     Current index when skipping through items
        /// </summary>
        private int _currentIndex;

        /// <summary>
        ///     Set listeners for buttons and hide unnecessary UI elements
        /// </summary>
        public void Start()
        {
            // Set listeners for buttons
            searchButton.onClick.AddListener(SearchForResults);
            nextSearch.onClick.AddListener(SkipToNextResult);
            previousSearch.onClick.AddListener(SkipToPreviousResult);

            // Search for every key-stroke
            userInput.onValueChanged.AddListener(SearchForResults);

            // Hide search related result indication on start
            textAmountResults.gameObject.SetActive(false);
        }

        /// <summary>
        ///     Update method for keyboard shortcuts
        /// </summary>
        public void Update()
        {
            // Set focus on search bar on CTRL-F
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
                userInput.ActivateInputField();
        }

        /// <summary>
        ///     Search for items in the hierarchy list
        /// </summary>
        /// <param name="input">Item name to be searched for</param>
        private void SearchForResults(string input)
        {
            // Reset search results and index position
            _currentIndex = 0;

            // Collect all game objects with the given input name
            _foundObjects.Clear();

            Utility.FillListWithChildrenByName(
                hierarchyViewController.hierarchyView.transform,
                input,
                _foundObjects
            );

            // No results, disable text indication and reset index
            if (_foundObjects.Count == 0)
            {
                _currentIndex = 0;
                textAmountResults.gameObject.SetActive(false);
            }
            // One or more results
            else
            {
                // Set index to first item
                _currentIndex = 1;
                UpdateResultsText();

                // Enable text indication
                textAmountResults.gameObject.SetActive(true);

                JumpToItemInListView();
            }
        }

        /// <summary>
        ///     Helper function for the search button
        /// </summary>
        private void SearchForResults()
        {
            // Forward user input to search function 
            SearchForResults(userInput.text);
        }

        /// <summary>
        ///     Skip to the next item in the search results
        /// </summary>
        private void SkipToNextResult()
        {
            // Check if there is more than 1 result
            if (_foundObjects.Count <= 1) return;

            // Increase index or go back to first object
            _currentIndex = _currentIndex == _foundObjects.Count ? 1 : _currentIndex + 1;
            UpdateResultsText();

            // Skip to the next result
            JumpToItemInListView();
        }

        /// <summary>
        ///     Jump to the current item in the list view
        /// </summary>
        private void JumpToItemInListView()
        {
            // TODO :: use hierarchyViewController's skipTo method from better-rename-control
            // hierarchyViewController.ScrollToItem(
            //     _foundObjects[_currentIndex - 1].GetComponent<RectTransform>()
            // );
        }

        /// <summary>
        ///     Go back to the previous item in the search results
        /// </summary>
        private void SkipToPreviousResult()
        {
            // Check if there is more than 1 result
            if (_foundObjects.Count <= 1) return;

            // Decrease index or go to last object
            _currentIndex = _currentIndex == 1 ? _foundObjects.Count : _currentIndex - 1;
            UpdateResultsText();

            // Go back to previous result
            JumpToItemInListView();
        }

        /// <summary>
        ///     Update string that indicates which item the user is currently
        ///     inspecting and adjust the font size if necessary
        /// </summary>
        private void UpdateResultsText()
        {
            if (_foundObjects.Count > 0)
            {
                var digitCount = Math.Floor(Math.Log10(_foundObjects.Count) + 1);

                textAmountResults.fontSize = digitCount > 3
                    ? textAmountResults.fontSize = (float) (15.0 - (digitCount - 3.0))
                    : textAmountResults.fontSize = 18.0f;
            }

            textAmountResults.SetText(_currentIndex + " / " + _foundObjects.Count);
        }
    }
}