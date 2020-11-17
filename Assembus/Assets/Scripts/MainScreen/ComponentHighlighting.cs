using System.Collections.Generic;
using System.Linq;
using MainScreen.Sidebar.HierarchyView;
using Models.Project;
using Services;
using UnityEngine;

namespace MainScreen
{
    public class ComponentHighlighting : MonoBehaviour
    {
        /// <summary>
        ///     Maximum distance for a ray intersection
        /// </summary>
        private const int MAXSelectionDistance = 10000;

        /// <summary>
        ///     The hierarchy view controller
        /// </summary>
        public HierarchyViewController hierarchyViewController;

        /// <summary>
        ///     Highlighting colors
        /// </summary>
        public Color colorHover, colorSelectedGroup, colorSelectedSingle;

        /// <summary>
        ///     List of selected game objects and their initial color value
        /// </summary>
        private readonly Dictionary<GameObject, Color> _selectedGameObjects = new Dictionary<GameObject, Color>();

        /// <summary>
        ///     Main camera
        /// </summary>
        private Camera _cam;

        /// <summary>
        ///     Previously hovered object for saving states
        /// </summary>
        private GameObject _hoveredObject;

        /// <summary>
        ///     Original color of previously hovered object for resetting
        /// </summary>
        private Color _hoveredOriginalColor;

        /// <summary>
        ///     Called on the frame the script is enabled on
        /// </summary>
        private void Start()
        {
            // Reference to camera
            _cam = Camera.main;
        }

        /// <summary>
        ///     Called once per frame, update object highlighting after update-processing
        /// </summary>
        private void LateUpdate()
        {
            // View highlighting
            if (_cam is null) return;

            // Create ray from mouse position
            var ray = _cam.ScreenPointToRay(Input.mousePosition);

            // Check if has an intersection with a game object
            var hitRegistered = Physics.Raycast(ray, out var hit, MAXSelectionDistance);

            if (hitRegistered)
            {
                // Get game object that was hit by the ray
                var hoveredObject = hit.transform.gameObject;

                // Allow multiple selections
                var multipleSelectionsAllowed = Input.GetKey(KeyCode.LeftControl);

                // Selection of a game object
                if (Input.GetMouseButtonDown(0)) HighlightSelection(hoveredObject, multipleSelectionsAllowed);

                // Hovered object was not highlighted yet
                if (hoveredObject != _hoveredObject)
                    // Highlight hovered object
                    HighlightHover(hoveredObject);
            }
            // No hit was registered
            else
            {
                // Reset hover-highlighting on non-selected objects
                if (_hoveredObject is null || _selectedGameObjects.ContainsKey(_hoveredObject)) return;

                _hoveredObject.GetComponent<Renderer>().material.color = _hoveredOriginalColor;

                // Reset previously hovered object
                _hoveredObject = null;
            }
        }

        /// <summary>
        ///     Highlight game object that mouse is hovering over
        /// </summary>
        /// <param name="hoveredObject">Game object that the mouse is hovering over in the current frame</param>
        private void HighlightHover(GameObject hoveredObject)
        {
            if (_selectedGameObjects.ContainsKey(hoveredObject)) return;

            // Different object was hovered over before
            if (!(_hoveredObject is null))
                // Reset highlighting color of previously hovered object if it is not a selected object
                if (!_selectedGameObjects.ContainsKey(_hoveredObject))
                    _hoveredObject.GetComponent<Renderer>().material.color = _hoveredOriginalColor;

            // Save previous color and game object
            var material = hoveredObject.GetComponent<Renderer>().material;

            _hoveredObject = hoveredObject;
            _hoveredOriginalColor = material.color;

            // Set highlighting color
            material.color = colorHover;
        }

        /// <summary>
        ///     Highlight selection of game objects
        /// </summary>
        /// <param name="clickedObject">Game object that was clicked in the current frame</param>
        /// <param name="multipleSelectionsAllowed">Boolean to allow selection of more than one object</param>
        private void HighlightSelection(GameObject clickedObject, bool multipleSelectionsAllowed)
        {
            // Clicked object is not selected yet
            if (!_selectedGameObjects.ContainsKey(clickedObject) || !multipleSelectionsAllowed)
            {
                // Reset previously selected objects if multiple selection is not enabled
                if (_selectedGameObjects.Count > 0 && !multipleSelectionsAllowed) ResetPreviousSelections();

                // Save clicked object and its previous color (before highlighting)
                var originalColor = clickedObject == _hoveredObject
                    ? _hoveredOriginalColor
                    : clickedObject.GetComponent<Renderer>().material.color;
                _selectedGameObjects.Add(clickedObject, originalColor);

                // Highlight
                if (_selectedGameObjects.Count > 1)
                    HighlightGameObjects(_selectedGameObjects.Keys.ToList());
                else
                    clickedObject.GetComponent<Renderer>().material.color = colorSelectedSingle;
            }
            // Clicked object was selected before already
            else
            {
                // Set color before selection
                clickedObject.GetComponent<Renderer>().material.color = _selectedGameObjects[clickedObject];

                // Remove from selection group
                _selectedGameObjects.Remove(clickedObject);
            }

            // Set the selected items active in the list view
            var names = _selectedGameObjects.Keys.ToList().Select(o => o.name);
            hierarchyViewController.SetItemStatusFromList(names);
        }

        /// <summary>
        ///     Reset state of all selected game objects
        /// </summary>
        private void ResetPreviousSelections()
        {
            // Go through selected game objects
            foreach (var keyValuePair in _selectedGameObjects)
                // Reset to original color before selection
                keyValuePair.Key.GetComponent<Renderer>().material.color = keyValuePair.Value;

            // Clear selection
            _selectedGameObjects.Clear();
        }

        /// <summary>
        ///     Highlight single game object
        /// </summary>
        /// <param name="gameObjects">Game object to be highlighted</param>
        public void HighlightGameObject(GameObject gameObjects)
        {
            // Clear current selection, to preserve original color invariance
            ResetPreviousSelections();

            var material = gameObjects.GetComponent<Renderer>();

            // Add to selection list and save original color
            _selectedGameObjects.Add(gameObjects, material.material.color);

            // Highlight selection
            material.material.color = colorSelectedSingle;
        }

        /// <summary>
        ///     Highlight every game object in list
        /// </summary>
        /// <param name="gameObjects">List of game objects to be highlighted</param>
        public void HighlightGameObjects(List<GameObject> gameObjects)
        {
            // Clear current selection, to preserve original color invariance
            ResetPreviousSelections();

            // Get all grouping objects in the list (if there are any) and their children
            var groupedObjects = new List<GameObject>();
            foreach (var g in gameObjects.Where(
                g => g.GetComponent<ItemInfoController>().ItemInfo.isGroup
            ))
            {
                groupedObjects.AddRange((Utility.GetAllGameObjects(g).ToList()));
            }
            gameObjects.AddRange(groupedObjects);

            // Single selection or group selection
            var color = gameObjects.Count > 1 ? colorSelectedGroup : colorSelectedSingle;

            gameObjects.ForEach(
                g =>
                {
                    var itemRenderer = g.GetComponent<Renderer>();
                    if (itemRenderer == null) return;

                    // Add to selection list and save original color
                    if (!_selectedGameObjects.ContainsKey(g))
                        _selectedGameObjects.Add(g, itemRenderer.material.color);

                    // Highlight selection
                    itemRenderer.material.color = color;
                }
            );
        }

        /// <summary>
        ///     Reset highlighting, when application is closed
        /// </summary>
        public void ResetHighlighting()
        {
            _selectedGameObjects.Clear();
            _hoveredObject = null;
        }
    }
}