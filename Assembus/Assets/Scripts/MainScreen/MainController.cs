using MainScreen.Sidebar.HierarchyView;
using Services;
using Services.UndoRedo;
using Shared;
using UnityEngine;
using UnityEngine.UI;

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
        ///     The hierarchy view
        /// </summary>
        public GameObject hierarchyView;

        /// <summary>
        ///     A default hierarchy view item
        /// </summary>
        public GameObject defaultHierarchyViewItem;

        /// <summary>
        ///     The hierarchy view controller
        /// </summary>
        public HierarchyViewController hierarchyViewController;

        /// <summary>
        ///     The project manager
        /// </summary>
        private readonly ProjectManager _projectManager = ProjectManager.Instance;

        /// <summary>
        ///     True if application shall be closed
        /// </summary>
        private bool _close;

        /// <summary>
        ///     True if hierarchy view needs to be updated
        /// </summary>
        private bool _updateHierarchyView;

        /// <summary>
        ///     The current width of the screen
        /// </summary>
        private int _width;

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
        ///     Check if the screen was resized
        /// </summary>
        private void Update()
        {
            var width = Screen.width;
            if (width == _width) return;

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
        }

        /// <summary>
        ///     Late update of the UI
        /// </summary>
        private void LateUpdate()
        {
            if (_updateHierarchyView)
                LayoutRebuilder.ForceRebuildLayoutImmediate(hierarchyView.GetComponent<RectTransform>());
        }

        /// <summary>
        ///     Set the name of the current project
        /// </summary>
        private void OnEnable()
        {
            // Initialize the command executor
            ActionCreator.Initialize(_projectManager.CurrentProject.ObjectModel, hierarchyView);

            // Load the model
            LoadModelIntoHierarchyView();
        }

        /// <summary>
        ///     Reset the camera
        /// </summary>
        public void ResetCamera()
        {
            _width = 0;
            mainCamera.rect = new Rect(0, 0, 1, 1);
        }

        /// <summary>
        ///     Load the object model into the hierarchy view
        /// </summary>
        private void LoadModelIntoHierarchyView()
        {
            defaultHierarchyViewItem.SetActive(true);

            // Get the root element of the object model
            var parent = _projectManager.CurrentProject.ObjectModel;

            // Remove the old children
            RemoveElementWithChildren(hierarchyView.transform, true);

            // Execute the recursive loading of game objects
            LoadElementWithChildren(hierarchyView, parent);

            // Force hierarchy view update
            _updateHierarchyView = true;

            defaultHierarchyViewItem.SetActive(false);
        }

        /// <summary>
        ///     Remove all previous list view items
        /// </summary>
        /// <param name="parent">The parent of the children that shall be removed</param>
        /// <param name="first">True if it is the first (to make sure that the default item isn't deleted)</param>
        private static void RemoveElementWithChildren(Transform parent, bool first)
        {
            for (var i = first ? 1 : 0; i < parent.childCount; i++)
            {
                RemoveElementWithChildren(parent.GetChild(i).transform, false);
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        /// <summary>
        ///     Load all elements of the game object and add them to the list
        /// </summary>
        /// <param name="containingListView">The container of the list view</param>
        /// <param name="parent">The parent item on the actual model</param>
        /// <param name="depth">The margin to the left side</param>
        private void LoadElementWithChildren(GameObject containingListView, GameObject parent, int depth = 32)
        {
            for (var i = 0; i < parent.transform.childCount; i++)
            {
                // for every child element
                var child = parent.transform.GetChild(i).gameObject;

                // generate a new hierarchy item in the hierarchy view
                var newHierarchyItem = Instantiate(
                    defaultHierarchyViewItem,
                    containingListView.transform,
                    true
                );

                newHierarchyItem.name = child.name;

                // get the script of the new item
                var itemController = newHierarchyItem.GetComponent<HierarchyItemController>();

                // initialize the item
                itemController.Initialize(child, depth, hierarchyView);

                // fill the new item recursively with children
                LoadElementWithChildren(itemController.childrenContainer, child, depth + 16);
            }
        }
    }
}