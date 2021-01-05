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
        ///     Placeholder string for the empty search field
        /// </summary>
        private const string Placeholder = "Search...";

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
        ///     Allows userInput to be reset to the placeholder text
        /// </summary>
        private bool _allowInputReset;

        /// <summary>
        ///     Current index when skipping through items
        /// </summary>
        private int _currentIndex;

        /// <summary>
        ///     Reset search state
        /// </summary>
        public void Reset()
        {
            _foundObjects.Clear();
            userInput.SetTextWithoutNotify(Placeholder);
            textAmountResults.gameObject.SetActive(false);
            _allowInputReset = false;
        }

        /// <summary>
        ///     Set listeners for buttons and hide unnecessary UI elements
        /// </summary>
        public void Start()
        {
            // Set listeners for buttons
            searchButton.onClick.AddListener(SearchForResults);
            nextSearch.onClick.AddListener(SkipToNextResult);
            previousSearch.onClick.AddListener(SkipToPreviousResult);

            // Assign listeners to input field
            userInput.onValueChanged.AddListener(SearchForResults);
            userInput.onSelect.AddListener(OnClick);
            userInput.onEndEdit.AddListener(OnExit);

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

            if (!_allowInputReset || !userInput.text.Equals(string.Empty)) return;

            _foundObjects.Clear();
            textAmountResults.gameObject.SetActive(false);
        }

        /// <summary>
        ///     Method that is called on clicking into the input field
        /// </summary>
        /// <param name="input">String that is contained in the input field</param>
        private void OnClick(string input)
        {
            if (!input.Equals(Placeholder)) return;

            userInput.SetTextWithoutNotify(string.Empty);
            _allowInputReset = false;
        }

        /// <summary>
        ///     Method that is called on losing focus in the input field
        /// </summary>
        /// <param name="input">String that is contained in the input field</param>
        private void OnExit(string input)
        {
            if (!input.Equals(string.Empty)) return;

            userInput.SetTextWithoutNotify(Placeholder);
            _allowInputReset = true;
        }

        /// <summary>
        ///     Search for items in the hierarchy list
        /// </summary>
        /// <param name="input">Item name to be searched for</param>
        private void SearchForResults(string input)
        {
            // Reset previous highlighting
            if (_currentIndex != 0 && _foundObjects.Count != 0)
            {
                var previousController = _foundObjects[_currentIndex - 1].GetComponent<HierarchyItemController>();
                if (!hierarchyViewController.IsSelected(previousController))
                    hierarchyViewController.SetColor(
                        previousController,
                        false
                    );
            }

            // Don't search for empty string
            if (input.Equals(string.Empty)) return;

            // Reset search state
            _currentIndex = 0;
            _foundObjects.Clear();
            _allowInputReset = true;
            
            // Collect all game objects with the given input name
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

                JumpToItemInListView(-1);
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

            var previousIndex = _currentIndex;

            // Increase index or go back to first object
            _currentIndex = _currentIndex == _foundObjects.Count ? 1 : _currentIndex + 1;
            UpdateResultsText();

            // Skip to the next result
            JumpToItemInListView(previousIndex);
        }

        /// <summary>
        ///     Jump to the current item in the list view
        /// </summary>
        /// <param name="previousIndex">Index of the previously selected item (negative if there was none)</param>
        private void JumpToItemInListView(int previousIndex)
        {
            // Remove previous highlighting in list
            if (previousIndex >= 0)
            {
                var previousController = _foundObjects[previousIndex - 1].GetComponent<HierarchyItemController>();
                if (!hierarchyViewController.IsSelected(previousController))
                    hierarchyViewController.SetColor(
                        previousController,
                        false
                    );
            }

            // Highlight current item in list
            var targetObject = _foundObjects[_currentIndex - 1];
            var targetController = targetObject.GetComponent<HierarchyItemController>();

            if (!hierarchyViewController.IsSelected(targetController))
                hierarchyViewController.SetColor(targetController, true);

            // Scroll to the given item in the list-view
            hierarchyViewController.ScrollToItem(
                targetObject.GetComponent<RectTransform>()
            );
        }

        /// <summary>
        ///     Go back to the previous item in the search results
        /// </summary>
        private void SkipToPreviousResult()
        {
            // Check if there is more than 1 result
            if (_foundObjects.Count <= 1) return;

            var previousIndex = _currentIndex;

            // Decrease index or go to last object
            _currentIndex = _currentIndex == 1 ? _foundObjects.Count : _currentIndex - 1;
            UpdateResultsText();

            // Go back to previous result
            JumpToItemInListView(previousIndex);
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