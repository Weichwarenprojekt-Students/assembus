using System.Linq;
using MainScreen.Sidebar.HierarchyView;
using Services;
using Shared;
using UnityEngine;

namespace MainScreen
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        /// <summary>
        ///     Rotation speed of the camera
        /// </summary>
        private const float RotationSpeed = 200f;

        /// <summary>
        ///     The factor between scroll speed and distance of the camera
        /// </summary>
        private const float ScrollFactor = 0.05f;

        /// <summary>
        ///     Reference to highlighting script
        /// </summary>
        public ComponentHighlighting componentHighlighting;

        /// <summary>
        ///     Reference to the hierarchy view controller
        /// </summary>
        public HierarchyViewController hierarchyViewController;

        /// <summary>
        ///     The click detector instance
        /// </summary>
        public DoubleClickDetector doubleClickDetector;

        /// <summary>
        ///     Reference to the main camera
        /// </summary>
        private Camera _cam;

        /// <summary>
        ///     Distance from the camera to the object
        /// </summary>
        private float _cameraDistance = 200f;

        /// <summary>
        ///     The transform object of the camera
        /// </summary>
        private Transform _camTransform;

        /// <summary>
        ///     Point the camera rotates around
        /// </summary>
        private Vector3 _centerPoint;

        /// <summary>
        ///     Camera position of previous frame. Used to calculate the new rotation
        /// </summary>
        private Vector3 _prevPosition;

        /// <summary>
        ///     True if right click occured over the viewport of the camera
        /// </summary>
        private bool _rightClickOverViewport;

        /// <summary>
        ///     Scroll/Zoom speed of the camera
        /// </summary>
        private float _scrollSpeed = 20f;

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
        ///     Called on the frame when script is enabled
        /// </summary>
        private void Start()
        {
            _cam = Camera.main;
            if (!(_cam is null)) _camTransform = _cam.transform;
            _centerPoint = new Vector3(0, 0, 0);

            // This is needed! without this the camera "snaps" to another location on first right click
            StoreLastMousePosition();
            CalculateNewCameraTransform();

            // Add the event handler. Update camera when double click occured
            doubleClickDetector.DoubleClickOccured += UpdateCameraFocus;
        }

        /// <summary>
        ///     Called after all update methods have been called
        /// </summary>
        private void LateUpdate()
        {
            // Store position when right mouse button is clicked
            if (Input.GetMouseButtonDown(1) && MouseOverViewport)
            {
                _rightClickOverViewport = true;
                StoreLastMousePosition();
            }

            // Reset flag when right mouse button is released
            if (Input.GetMouseButtonUp(1)) _rightClickOverViewport = false;

            // When right mouse button is held and right click occured over the viewport, rotate the camera
            if (Input.GetMouseButton(1) && _rightClickOverViewport) CalculateNewCameraTransform();

            // Focus camera if game object is double clicked
            if (Input.GetMouseButtonUp(0))
                doubleClickDetector.Click();

            // Check for second click
            doubleClickDetector.CheckForSecondClick();

            // Detect scrolling
            if (Input.mouseScrollDelta.y != 0) Zoom(Input.mouseScrollDelta.y);
        }

        /// <summary>
        ///     Set focus on passed GameObject component group
        /// </summary>
        /// <param name="parent">The object that shall be shown</param>
        /// <param name="setScrollSpeed">True if the scroll speed should be adjusted to the new camera distance</param>
        public void ZoomOnObject(GameObject parent, bool setScrollSpeed = true)
        {
            // Get the bounds of the game object
            var bounds = GetBounds(parent);

            // Get the sizes of the object and its children
            var objectSizes = bounds.max - bounds.min;

            // Calculate the camera distance
            var objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
            _cameraDistance = 0.5f * (objectSize / Mathf.Tan(0.5f * Mathf.Deg2Rad * _cam.fieldOfView) + objectSize);
            _camTransform.position = bounds.center - _cameraDistance * _camTransform.forward;

            // Set the focus to the middle
            SetFocus(bounds.center);

            if (setScrollSpeed)
                // Recalculate the scroll speed
                _scrollSpeed = ScrollFactor * _cameraDistance;
        }

        /// <summary>
        ///     Set focus on clicked game object
        /// </summary>
        private void UpdateCameraFocus()
        {
            if (_cam is null) return;

            if (!MouseOverViewport) return;

            var ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out var hit, 10000)) return;

            var go = hit.transform.gameObject;

            componentHighlighting.HighlightGameObject(go);
            hierarchyViewController.SetItemStatusFromList(new[] {go.name});
            SetFocus(GetBounds(go).center);
            StoreLastMousePosition();
            CalculateNewCameraTransform();
        }

        /// <summary>
        ///     Set focus on passed GameObject
        /// </summary>
        /// <param name="go">GameObject which should be focused</param>
        public void UpdateCameraFocus(GameObject go)
        {
            if (_cam is null) return;

            SetFocus(GetBounds(go).center);
            StoreLastMousePosition();
            CalculateNewCameraTransform();
        }

        /// <summary>
        ///     Calculate the rendering bounds of a GameObjects and all children
        /// </summary>
        /// <param name="gameObj">The GameObject from which to get the bounds</param>
        /// <returns>Bounds that include a GameObject and its children</returns>
        private static Bounds GetBounds(GameObject gameObj)
        {
            // initialize with wrong value, because null initialization is not possible
            var bounds = new Bounds();

            // Set basic bounds to the ones of the GameObject, if the GameObject has one
            var renderer = gameObj.GetComponent<Renderer>();
            if (renderer != null) bounds = renderer.bounds;

            // Get all Renderer components of the children of the GameObject
            var childRenderers = Utility.GetAllChildren(gameObj)
                .Select(child => child.GetComponent<Renderer>())
                .Where(childRenderer => childRenderer != null).ToList();

            // Set the basic bounds to the first bounds from the child objects, if a child exists
            // Usually the GameObject does not have bounds, if it has children
            bounds = childRenderers.FirstOrDefault()?.bounds ?? bounds;

            // Add all bounds from all child objects to the bounds
            foreach (var childRenderer in childRenderers) bounds.Encapsulate(childRenderer.bounds);

            return bounds;
        }

        /// <summary>
        ///     Calculates the new camera position based on the mouse position
        /// </summary>
        private void CalculateNewCameraTransform()
        {
            var direction = _prevPosition - _cam.ScreenToViewportPoint(Input.mousePosition);

            _cam.transform.position = _centerPoint;

            _cam.transform.Rotate(Vector3.right, direction.y * RotationSpeed);
            _cam.transform.Rotate(Vector3.up, -direction.x * RotationSpeed, Space.World);
            _cam.transform.Translate(new Vector3(0, 0, -_cameraDistance));

            _prevPosition = _cam.ScreenToViewportPoint(Input.mousePosition);
        }

        /// <summary>
        ///     Stores the current mouse position in view port space when called
        /// </summary>
        private void StoreLastMousePosition()
        {
            _prevPosition = _cam.ScreenToViewportPoint(Input.mousePosition);
        }

        /// <summary>
        ///     Calculates camera distance
        /// </summary>
        private void Zoom(float delta)
        {
            // Don't zoom if mouse is not over viewport
            if (!MouseOverViewport) return;

            // Calculate camera distance
            _cameraDistance -= delta * _scrollSpeed;
            if (_cameraDistance < 0) _cameraDistance = 0;

            // Apply camera distance
            StoreLastMousePosition();
            CalculateNewCameraTransform();
        }

        /// <summary>
        ///     Sets focus on given Vector3
        /// </summary>
        /// <param name="position">Vector3 to center on</param>
        private void SetFocus(Vector3 position)
        {
            _centerPoint = position;
        }
    }
}