using System.Collections.Generic;
using System.Linq;
using MainScreen.Sidebar.HierarchyView;
using Models.Project;
using Shared;
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
        ///     True if mouse position is over viewport of 3D editor
        /// </summary>
        private bool MouseOverViewport
        {
            get
            {
                var view = _cam.ScreenToViewportPoint(Input.mousePosition);
                return !(view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1);
            }
        }

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
            // Prevent highlighting updates when mouse is not over the 3d editor
            if (!MouseOverViewport || _cam is null) return;

            // Create ray from mouse position
            var ray = _cam.ScreenPointToRay(Input.mousePosition);

            // Check if the ray has an intersection with a game object
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
                // No previously hovered object, or previously hovered has been selected: No need to reset
                if (_hoveredObject is null || _selectedGameObjects.ContainsKey(_hoveredObject)) return;

                // Reset hover-highlighting on non-selected objects
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
            // Hovering over a selected game object after hovering over another one
            if (_selectedGameObjects.ContainsKey(hoveredObject) && _hoveredObject != null)
            {
                // Do nothing, if the previously hovered game object was a selected one
                if (_selectedGameObjects.ContainsKey(_hoveredObject)) return;

                // Remove highlighting of the previously hovered object
                _hoveredObject.GetComponent<Renderer>().material.color = _hoveredOriginalColor;

                // Reset previously hovered object
                _hoveredObject = null;

                return;
            }

            // Do not hover-highlight a selected game object
            if (_selectedGameObjects.ContainsKey(hoveredObject))
                return;

            // Different object was hovered over before
            if (!(_hoveredObject is null))
                // Reset highlighting color of previously hovered object if it is not a selected object
                if (!_selectedGameObjects.ContainsKey(_hoveredObject))
                    _hoveredObject.GetComponent<Renderer>().material.color = _hoveredOriginalColor;

            // Reference to the objects material
            var material = hoveredObject.GetComponent<Renderer>().material;

            // Set last hovered object and and safe its original color
            _hoveredObject = hoveredObject;
            _hoveredOriginalColor = material.color;

            // Set highlighting color on the hovered object
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
            if (!_selectedGameObjects.ContainsKey(clickedObject))
            {
                // Reset previously selected objects if multiple selection is not enabled
                if (_selectedGameObjects.Count > 0 && !multipleSelectionsAllowed) ResetPreviousSelections();

                // Save clicked object and its previous color (before highlighting)
                var originalColor = clickedObject == _hoveredObject
                    ? _hoveredOriginalColor
                    : clickedObject.GetComponent<Renderer>().material.color;
                _selectedGameObjects.Add(clickedObject, originalColor);

                // Highlight either single or collection of clicked game objects
                if (_selectedGameObjects.Count > 1)
                    HighlightGameObjects(_selectedGameObjects.Keys.ToList());
                else
                    clickedObject.GetComponent<Renderer>().material.color = colorSelectedSingle;
            }
            // Clicked object was selected before already
            else
            {
                // Reselect item if clicked item was part of a multi-selection before (without ctrl)
                if (_selectedGameObjects.Count > 1 && !multipleSelectionsAllowed)
                {
                    ResetPreviousSelections();

                    // Save clicked object and previous color
                    var originalColor = clickedObject == _hoveredObject
                        ? _hoveredOriginalColor
                        : clickedObject.GetComponent<Renderer>().material.color;

                    // Add clicked object to selection
                    _selectedGameObjects.Add(clickedObject, originalColor);

                    // Highlight single object
                    clickedObject.GetComponent<Renderer>().material.color = colorSelectedSingle;
                }
                // Only the clicked item was part of the selection before: Deselect
                else
                {
                    // Set original color before selection
                    clickedObject.GetComponent<Renderer>().material.color = _selectedGameObjects[clickedObject];

                    // Remove from selection group
                    _selectedGameObjects.Remove(clickedObject);

                    // Reset hovering state so deselected object can be hovered again
                    _hoveredObject = null;
                }
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
            // Reset hover-highlighting on non-selected objects
            if (!(_hoveredObject is null || _selectedGameObjects.ContainsKey(_hoveredObject)))
            {
                _hoveredObject.GetComponent<Renderer>().material.color = _hoveredOriginalColor;
                _hoveredObject = null;
            }

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
        ///     Highlight every game object or group contained in the list
        /// </summary>
        /// <param name="gameObjects">List of game objects or groups to be highlighted</param>
        public void HighlightGameObjects(List<GameObject> gameObjects)
        {
            // Clear current selection, to preserve original color invariance
            ResetPreviousSelections();

            // Get all grouping objects in the list (if there are any) and their children
            var groupedObjects = new List<GameObject>();
            var groups =
                gameObjects.Where(g => g.GetComponent<ItemInfoController>().ItemInfo.isGroup);
            foreach (var group in groups)
                groupedObjects.AddRange(Utility.GetAllChildren(group).ToList());

            // Add the grouped objects to the list of objects to be highlighted
            gameObjects.AddRange(groupedObjects);

            // Single selection or group selection
            var color = gameObjects.Count > 1 ? colorSelectedGroup : colorSelectedSingle;

            // Highlight every game object
            gameObjects.ForEach(
                group =>
                {
                    var itemRenderer = group.GetComponent<Renderer>();
                    if (itemRenderer == null) return;

                    // Add to selection list and save original color
                    if (!_selectedGameObjects.ContainsKey(group))
                        _selectedGameObjects.Add(group, itemRenderer.material.color);

                    // Highlight selection
                    itemRenderer.material.color = color;
                }
            );
        }

        /// <summary>
        ///     Highlights a given game object with the hovering color, used
        ///     to indicate which object is hovered over in the item list-view
        /// </summary>
        /// <param name="gO">Game object to be highlighted</param>
        public void HighlightHoverFromList(GameObject gO)
        {
            // Check if component has a Renderer
            if (gO.GetComponent<Renderer>() != null)
            {
                HighlightHover(gO);
            }
            else
            {
                if (_hoveredObject == null) return;

                _hoveredObject.GetComponent<Renderer>().material.color = _hoveredOriginalColor;
                _hoveredObject = null;
            }
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