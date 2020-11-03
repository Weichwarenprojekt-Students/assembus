using System.Collections;
using UnityEngine;

namespace MainScreen
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        /// <summary>
        ///     Time frame for double click detection
        /// </summary>
        private const float TimeBetweenClicks = 0.25f;

        /// <summary>
        ///     Rotation speed of the camera
        /// </summary>
        private readonly float rotationSpeed = 200f;

        /// <summary>
        ///     Scroll/Zoom speed of the camera
        /// </summary>
        private readonly float scrollSpeed = 20f;

        /// <summary>
        ///     Reference to the main camera
        /// </summary>
        private Camera _cam;

        /// <summary>
        ///     Distance from the camera to the object
        /// </summary>
        private float _cameraDistance = 200f;

        /// <summary>
        ///     Point the camera rotates around
        /// </summary>
        private Vector3 _centerPoint;

        /// <summary>
        ///     Allow coroutine for double click detection
        /// </summary>
        private bool _coroutineAllowed = true;

        /// <summary>
        ///     Time when left mouse button is clicked first time
        /// </summary>
        private float _firstLeftClickTime;

        /// <summary>
        ///     Left mouse click counter for double click detection
        /// </summary>
        private int _leftClickCounter;

        /// <summary>
        ///     Camera position of previous frame. Used to calculate the new rotation
        /// </summary>
        private Vector3 _prevPosition;

        /// <summary>
        ///     Private reference to singleton
        /// </summary>
        public static CameraController Instance { get; private set; }

        /// <summary>
        ///     Called when script object is initialized
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        ///     Called on the frame when script is enabled
        /// </summary>
        private void Start()
        {
            _cam = Camera.main;
            _centerPoint = new Vector3(0, 0, 0);

            // this is needed! without this the camera "snaps" to another location on first right click
            StoreLastMousePosition();
            CalculateNewCameraTransform();
        }

        /// <summary>
        ///     Called after all update methods have been called
        /// </summary>
        private void LateUpdate()
        {
            // store position when right mouse button is clicked
            if (Input.GetMouseButtonDown(1)) StoreLastMousePosition();

            // when right mouse button is held rotate the camera
            if (Input.GetMouseButton(1)) CalculateNewCameraTransform();

            // Focus camera if game object is double clicked
            if (Input.GetMouseButtonUp(0))
                _leftClickCounter += 1;

            if (_leftClickCounter == 1 && _coroutineAllowed)
            {
                _firstLeftClickTime = Time.time;
                StartCoroutine(DoubleClickDetection());
            }

            // detect scrolling
            if (Input.mouseScrollDelta.y != 0) Zoom(Input.mouseScrollDelta.y);
        }

        /// <summary>
        ///     Coroutine, detect double clicking
        /// </summary>
        /// <returns>IEnumerator to pause action</returns>
        private IEnumerator DoubleClickDetection()
        {
            _coroutineAllowed = false;
            while (Time.time < _firstLeftClickTime + TimeBetweenClicks)
            {
                if (_leftClickCounter == 2)
                {
                    UpdateCameraFocus();
                    break;
                }

                yield return new WaitForEndOfFrame();
            }

            _leftClickCounter = 0;
            _firstLeftClickTime = 0f;
            _coroutineAllowed = true;
        }

        /// <summary>
        ///     Set focus on clicked game object
        /// </summary>
        private void UpdateCameraFocus()
        {
            if (_cam is null) return;

            var ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out var hit, 10000)) return;

            var transformGameObject = hit.transform.gameObject.GetComponent<Renderer>().bounds.center;

            SetFocus(transformGameObject);
            StoreLastMousePosition();
            CalculateNewCameraTransform();
        }

        /// <summary>
        ///     Calculates the new camera position based on the mouse position
        /// </summary>
        private void CalculateNewCameraTransform()
        {
            var direction = _prevPosition - _cam.ScreenToViewportPoint(Input.mousePosition);

            _cam.transform.position = _centerPoint;

            _cam.transform.Rotate(Vector3.right, direction.y * rotationSpeed);
            _cam.transform.Rotate(Vector3.up, -direction.x * rotationSpeed, Space.World);
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
            // calculate camera distance
            _cameraDistance -= delta * scrollSpeed;

            // apply camera distance
            StoreLastMousePosition();
            CalculateNewCameraTransform();
        }

        /// <summary>
        ///     Sets focus on given transform
        /// </summary>
        /// <param name="t">Transform to center on</param>
        public void SetFocus(Transform t)
        {
            _centerPoint = t.position;
        }

        /// <summary>
        ///     Sets focus on given Vector3
        /// </summary>
        /// <param name="position">Vector3 to center on</param>
        public void SetFocus(Vector3 position)
        {
            _centerPoint = position;
        }

        /// <summary>
        ///     Sets focus on given gameobject
        /// </summary>
        /// <param name="g">Gameobject to center on</param>
        public void SetFocus(GameObject g)
        {
            _centerPoint = g.transform.position;
        }
    }
}