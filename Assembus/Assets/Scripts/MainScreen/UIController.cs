using Services;
using Shared;
using Shared.Toast;
using TMPro;
using UnityEngine;

namespace MainScreen
{
    public class UIController : MonoBehaviour
    {
        /// <summary>
        ///     The main camera
        /// </summary>
        public Camera mainCamera;

        /// <summary>
        ///     The sidebar
        /// </summary>
        public RectTransform sidebar;

        /// <summary>
        ///     The canvas for the main screen
        /// </summary>
        public Canvas mainCanvas;

        /// <summary>
        ///     The two screens
        /// </summary>
        public GameObject startScreen, mainScreen;

        /// <summary>
        ///     The dialog controller
        /// </summary>
        public DialogController dialog;

        /// <summary>
        ///     The toast controller
        /// </summary>
        public ToastController toast;

        /// <summary>
        ///     The title view
        /// </summary>
        public TextMeshProUGUI title;

        /// <summary>
        ///     The configuration manager
        /// </summary>
        private readonly ConfigurationManager _configManager = ConfigurationManager.Instance;

        /// <summary>
        ///     The project manager
        /// </summary>
        private readonly ProjectManager _projectManager = ProjectManager.Instance;

        /// <summary>
        ///     The current width of the screen
        /// </summary>
        private int _width;

        /// <summary>
        ///     Check if the screen was resized
        /// </summary>
        private void Update()
        {
            var width = Screen.width;
            if (width == _width) return;

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
        ///     Set the name of the current project
        /// </summary>
        private void OnEnable()
        {
            title.text = _projectManager.CurrentProject.Name + "*";
        }

        /// <summary>
        ///     Save the current project
        /// </summary>
        public void SaveProject()
        {
            var (success, message) = _projectManager.SaveProject();
            if (!success)
            {
                toast.Error(Toast.Short, message);
                return;
            }

            // Show that the saving was successful
            _projectManager.Saved = true;
            title.text = _projectManager.CurrentProject.Name;
            toast.Success(Toast.Short, "Project was saved successfully!");
        }

        /// <summary>
        ///     Close a project
        /// </summary>
        public void CloseProject()
        {
            var description = _projectManager.Saved ? "Are you sure?" : "Unsaved changes! Are you sure?";
            dialog.Show(
                "Close Project",
                description,
                () =>
                {
                    // Remove the last opened project 
                    _configManager.Config.lastProject = "";
                    _configManager.SaveConfig();

                    // Reset camera
                    _width = 0;
                    mainCamera.rect = new Rect(0, 0, 1, 1);

                    // Show the start screen
                    mainScreen.SetActive(false);
                    startScreen.SetActive(true);
                }
            );
        }
    }
}