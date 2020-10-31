using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainScreen
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        
        /// <summary>
        ///     Reference to the main camera
        /// </summary>
        private Camera cam;
        
        /// <summary>
        ///     Point the camera rotates around
        /// </summary>
        private Vector3 _centerPoint;
        
        /// <summary>
        ///     Rotation speed of the camera
        /// </summary>
        private float rotationSpeed = 200f;
        
        /// <summary>
        ///     Camera position of previous frame. Used to calculate the new rotation
        /// </summary>
        private Vector3 _prevPosition;

        /// <summary>
        ///     Distance from the camera to the object
        /// </summary>
        private float _cameraDistance;
        
        void Start()
        {
            cam = Camera.main;
            _centerPoint = new Vector3(0,0,0);
            _cameraDistance = transform.position.x;
        }

        
        void LateUpdate()
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
            
        }

        private void CalculateNewCameraTransform()
        {
            Vector3 direction = _prevPosition - cam.ScreenToViewportPoint(Input.mousePosition);

            cam.transform.position = _centerPoint;
                
            cam.transform.Rotate(Vector3.right, direction.y * rotationSpeed);
            cam.transform.Rotate(Vector3.up, -direction.x * rotationSpeed, Space.World);
            cam.transform.Translate(new Vector3(0, 0, -_cameraDistance));

            _prevPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        private void StoreLastMousePosition()
        {
            _prevPosition = cam.ScreenToViewportPoint((Input.mousePosition));
        }
    }
}