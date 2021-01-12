using MainScreen.Sidebar.HierarchyView;
using Services.Serialization;
using Shared;
using UnityEngine;

namespace MainScreen
{
    public class MainController : MonoBehaviour
    {
        /// <summary>
        ///     The main camera
        /// </summary>
        public Camera mainCamera;

        /// <summary>
        ///     The camera controller
        /// </summary>
        public CameraController cameraController;

        /// <summary>
        ///     The sidebar
        /// </summary>
        public RectTransform sidebar;

        /// <summary>
        ///     The canvas for the main screen
        /// </summary>
        public Canvas mainCanvas;

        /// <summary>
        ///     The dialog controller
        /// </summary>
        public DialogController dialog;

        /// <summary>
        ///     The hierarchy view controller
        /// </summary>
        public HierarchyViewController hierarchyController;

        /// <summary>
        ///     The project manager
        /// </summary>
        private readonly ProjectManager _projectManager = ProjectManager.Instance;

        /// <summary>
        ///     True if application shall be closed
        /// </summary>
        private bool _close;

        /// <summary>
        ///     The current width and height of the screen
        /// </summary>
        private int _width, _height;

        /// <summary>
        ///     Add event for window closing
        /// </summary>
        private void Start()
        {
            Application.wantsToQuit += () =>
            {
                if (_projectManager.Saved || _close) return true;
                dialog.Show(
                    "Close Assembus",
                    "Unsaved changes! Are you sure?",
                    () =>
                    {
                        _close = true;
                        Application.Quit();
                    }
                );
                return false;
            };
        }

        /// <summary>
        ///     Enter the editor view
        /// </summary>
        public void ShowEditor()
        {
            // When opening a project, it's always saved
            _projectManager.Saved = true;

            gameObject.SetActive(true);
            hierarchyController.SetupHierarchy();
        }

        /// <summary>
        ///     Check if the screen was resized
        /// </summary>
        private void Update()
        {
            var width = Screen.width;
            var height = Screen.height;
            if (width == _width && height == _height) return;

            // Reposition the camera
            cameraController.ZoomOnObject(_projectManager.CurrentProject.ObjectModel);

            // Get the actual layout sizes
            var localScale = mainCanvas.transform.localScale;
            var sidebarWidth = localScale.x * sidebar.rect.width;

            // Calculate the bounds
            var x = sidebarWidth / width;

            // Set the new bounds
            mainCamera.rect = new Rect(x, 0, 1 - x, 1);
            _width = width;
            _height = height;
        }

        /// <summary>
        ///     Reset the camera
        /// </summary>
        public void ResetCamera()
        {
            _width = 0;
            mainCamera.rect = new Rect(0, 0, 1, 1);
        }
    }
}