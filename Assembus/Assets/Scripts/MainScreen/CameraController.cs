using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainScreen
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {

        /// <summary>
        ///     Private reference to singleton
        /// </summary>
        private static CameraController _instance;
        
        public static CameraController Instance
        {
            get { return _instance; }
        }

        /// <summary>
        ///     Reference to the main camera
        /// </summary>
        private Camera _cam;
        
        /// <summary>
        ///     Point the camera rotates around
        /// </summary>
        private Vector3 _centerPoint;
        
        /// <summary>
        ///     Rotation speed of the camera
        /// </summary>
        private float rotationSpeed = 200f;
        
        /// <summary>
        ///     Scroll/Zoom speed of the camera
        /// </summary>
        private float scrollSpeed = 20f;
        
        /// <summary>
        ///     Camera position of previous frame. Used to calculate the new rotation
        /// </summary>
        private Vector3 _prevPosition;

        /// <summary>
        ///     Distance from the camera to the object
        /// </summary>
        private float _cameraDistance = 200f;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            } else 
            {
                _instance = this;
            }
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _cam = Camera.main;
            _centerPoint = new Vector3(0,0,0);
            
            // this is needed! without this the camera "snaps" to another location on first right click
            StoreLastMousePosition();
            CalculateNewCameraTransform();
        }

        
        private void LateUpdate()
        {
            // store position when right mouse button is clicked
            if (Input.GetMouseButtonDown(1))
            {
                StoreLastMousePosition();
            }
            
            // when right mouse button is held rotate the camera
            if (Input.GetMouseButton(1))
            {
                CalculateNewCameraTransform();
            }
            
            // detect scrolling
            if (Input.mouseScrollDelta.y != 0)
            {
                Zoom(Input.mouseScrollDelta.y);
            }

        }
        
        /// <summary>
        ///     Calculates the new camera position based on the mouse position
        /// </summary>
        private void CalculateNewCameraTransform()
        {
            Vector3 direction = _prevPosition - _cam.ScreenToViewportPoint(Input.mousePosition);

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
            _prevPosition = _cam.ScreenToViewportPoint((Input.mousePosition));
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