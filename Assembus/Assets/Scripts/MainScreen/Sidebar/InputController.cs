using MainScreen.Sidebar.HierarchyView;
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
        ///     Amount of items found after completing a search
        /// </summary>
        private int _amountResults;

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

            // Hide search related buttons on start
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
        /// </summary>
        /// <param name="input"></param>
        private void SearchForResults(string input)
        {
            Debug.Log("Searching for objects...");

            // Search for gameObjects with the given name
            _amountResults = 10;
            _currentIndex = 0;

            // TODO :: Find every hierarchy object with the name "input"
            // TODO :: Save list of those objects (or their rect-transforms)
            var view = hierarchyViewController.hierarchyView;
            Debug.Log("Test");

            // No results, disable arrows and text indication
            if (_amountResults == 0)
            {
                _currentIndex = 0;
                
                textAmountResults.gameObject.SetActive(false);
            }
            else
            {
                // Set index to first item
                _currentIndex = 1;

                // More than one result -> update string of found objects -> enable arrows
                if (_amountResults > 1)
                {
                    UpdateResultsText();
                }

                textAmountResults.gameObject.SetActive(true);
                
                // TODO :: Jump to first item using hierarchyViewController's skipTo method from better-rename-control
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
            Debug.Log("Next...");

            // Check if there is more than 1 result
            if (_amountResults <= 1) return;

            // Increase index or go back to first object
            _currentIndex = _currentIndex == _amountResults ? 1 : _currentIndex + 1;
            UpdateResultsText();

            // Skip to the next result
            // TODO :: use hierarchyViewController's skipTo method from better-rename-control
        }

        /// <summary>
        ///     Go back to the previous item in the search results
        /// </summary>
        private void SkipToPreviousResult()
        {
            Debug.Log("Previous...");

            // Check if there is more than 1 result
            if (_amountResults <= 1) return;

            // Decrease index or go to last object
            _currentIndex = _currentIndex == 1 ? _amountResults : _currentIndex - 1;
            UpdateResultsText();

            // Go back to previous result
            // TODO :: use hierarchyViewController's skipTo method from better-rename-control
        }

        /// <summary>
        ///     Update string that indicates which item the user is currently
        ///     inspecting
        /// </summary>
        private void UpdateResultsText()
        {
            textAmountResults.SetText(_currentIndex + " / " + _amountResults);
        }
    }
}