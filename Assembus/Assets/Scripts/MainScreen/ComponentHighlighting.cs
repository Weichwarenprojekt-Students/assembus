using System.Collections.Generic;
using System.Linq;
using MainScreen.HierarchyView;
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
        ///     Hovering color
        /// </summary>
        private readonly Color _colorHover = Color.yellow;

        /// <summary>
        ///     Group selection color
        /// </summary>
        private readonly Color _colorSelectedGroup = Color.blue;

        /// <summary>
        ///     Single selection color
        /// </summary>
        private readonly Color _colorSelectedSingle = Color.red;

        /// <summary>
        ///     List of selected game objects and their initial color value
        /// </summary>
        private readonly Dictionary<GameObject, Color> _selectedGameObjects = new Dictionary<GameObject, Color>();
        
        /// <summary>
        ///     Currently selected items in list view for comparison between frames
        /// </summary>
        private HashSet<GameObject> _listViewSelection = new HashSet<GameObject>();

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
        ///     Called once per frame, update object highlighting
        /// </summary>
        private void Update()
        {
            // Highlight selections from listview, if any new ones have appeared
            
            /* var list = ProjectManager.Instance.SelectedItems.Values.Select(t => t.Item2).ToList();
            if (!_listViewSelection.SetEquals(list))
            {
                HighlightGameObjects(list);
                _listViewSelection = new HashSet<GameObject>(list);
            }*/
            
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
                if (Input.GetMouseButtonDown(0))
                {
                    HighlightSelection(hoveredObject, multipleSelectionsAllowed);
                }

                // Hovered object was not highlighted yet
                if (hoveredObject != _hoveredObject)
                {
                    // Highlight hovered object
                    HighlightHover(hoveredObject);
                }
            }
            // No hit was registered
            else
            {
                // Reset hover-highlighting on non-selected objects
                if (!(_hoveredObject is null) && !_selectedGameObjects.ContainsKey(_hoveredObject))
                {
                    _hoveredObject.GetComponent<Renderer>().material.color = _hoveredOriginalColor;

                    // Reset previously hovered object
                    _hoveredObject = null;
                }
            }
        }

        /// <summary>
        ///     Highlight game object that mouse is hovering over
        /// </summary>
        /// <param name="hoveredObject">Game object that the mouse is hovering over in the current frame</param>
        private void HighlightHover(GameObject hoveredObject)
        {
            if (_selectedGameObjects.ContainsKey(hoveredObject)) return;

            Debug.Log("Hovering");

            // Different object was hovered over before
            if (!(_hoveredObject is null))
            {
                // Reset highlighting color of previously hovered object if it is not a selected object
                if (!_selectedGameObjects.ContainsKey(_hoveredObject))
                {
                    _hoveredObject.GetComponent<Renderer>().material.color = _hoveredOriginalColor;
                }
            }

            // Save previous color and game object
            _hoveredObject = hoveredObject;
            _hoveredOriginalColor = hoveredObject.GetComponent<Renderer>().material.color;

            // Set highlighting color
            hoveredObject.GetComponent<Renderer>().material.color = _colorHover;
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
                Debug.Log("Select");

                // Reset previously selected objects if multiple selection is not enabled
                if (_selectedGameObjects.Count > 0 && !multipleSelectionsAllowed)
                {
                    ResetPreviousSelections();
                }

                // Save clicked object and its previous color (before highlighting)
                var originalColor = clickedObject == _hoveredObject
                    ? _hoveredOriginalColor
                    : clickedObject.GetComponent<Renderer>().material.color;
                _selectedGameObjects.Add(clickedObject, originalColor);

                // Highlight
                if (_selectedGameObjects.Count > 1)
                {
                    HighlightGameObjects(_selectedGameObjects.Keys.ToList());
                }
                else
                {
                    clickedObject.GetComponent<Renderer>().material.color = _colorSelectedSingle;
                }
            }
            // Clicked object was selected before already
            else
            {
                Debug.Log("Deselection");

                // Set color before selection
                clickedObject.GetComponent<Renderer>().material.color = _selectedGameObjects[clickedObject];

                // Remove from selection group
                _selectedGameObjects.Remove(clickedObject);
            }
            
            // Set the selected items active in the list view
            var names = _selectedGameObjects.Keys.ToList().Select((o => o.name));
            GameObject.Find("Viewport").GetComponent<HierarchyViewController>().SetItemStatusFromList(names);
        }

        /// <summary>
        ///     Reset state of all selected game objects
        /// </summary>
        private void ResetPreviousSelections()
        {
            Debug.Log("Reset");

            // Go through selected game objects
            foreach (var keyValuePair in _selectedGameObjects)
            {
                // Reset to original color before selection
                keyValuePair.Key.GetComponent<Renderer>().material.color = keyValuePair.Value;
            }

            // Clear selection
            _selectedGameObjects.Clear();
        }

        /// <summary>
        ///     Highlight every game object in list
        /// </summary>
        /// <param name="gameObjects">List of gameObjects to be highlighted</param>
        private void HighlightGameObjects(List<GameObject> gameObjects)
        {
            // Clear current selection, to preserve original color invariance
            ResetPreviousSelections();

            // Single selection or group selection
            Color color = (gameObjects.Count > 1) ? _colorSelectedGroup : _colorSelectedSingle;

            gameObjects.ForEach(
                g =>
                {
                    // Add to selection list and save original color
                    _selectedGameObjects.Add(g, g.GetComponent<Renderer>().material.color);

                    // Highlight selection
                    g.GetComponent<Renderer>().material.color = color;
                }
            );
        }
    }
}