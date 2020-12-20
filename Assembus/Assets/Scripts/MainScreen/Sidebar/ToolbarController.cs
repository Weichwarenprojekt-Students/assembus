using MainScreen.StationView;
using Services.Serialization;
using Services.UndoRedo;
using Shared;
using Shared.Toast;
using TMPro;
using UnityEngine;

namespace MainScreen.Sidebar
{
    public class ToolbarController : MonoBehaviour
    {
        /// <summary>
        ///     The three screens
        /// </summary>
        public GameObject startScreen, mainScreen, cinemaScreen;

        /// <summary>
        ///     The toast controller
        /// </summary>
        public ToastController toast;

        /// <summary>
        ///     The dialog controller
        /// </summary>
        public DialogController dialog;

        /// <summary>
        ///     The title view
        /// </summary>
        public TextMeshProUGUI title;

        /// <summary>
        ///     The main controller
        /// </summary>
        public MainController mainController;

        /// <summary>
        ///     The buttons that can be disabled
        /// </summary>
        public SwitchableButton undo, redo;

        /// <summary>
        ///     The settings controller
        /// </summary>
        public SettingsController settings;

        /// <summary>
        ///     The controller of the station view
        /// </summary>
        public StationController stationController;

        /// <summary>
        ///     The component highlighting script
        /// </summary>
        public ComponentHighlighting componentHighlighting;

        /// <summary>
        ///     InputController script
        /// </summary>
        public InputController inputController;

        /// <summary>
        ///     The configuration manager
        /// </summary>
        private readonly ConfigurationManager _configManager = ConfigurationManager.Instance;

        /// <summary>
        ///     The project manager
        /// </summary>
        private readonly ProjectManager _projectManager = ProjectManager.Instance;

        /// <summary>
        ///     The undo redo service
        /// </summary>
        private readonly UndoService _undoService = UndoService.Instance;

        /// <summary>
        ///     Set the callback for new commands
        /// </summary>
        private void Start()
        {
            _undoService.OnNewCommand = () => UpdateProjectView(false);
        }

        /// <summary>
        ///     Check if either CTRL-Z, CTRL-Y or CTRL-SHIFT_Z was used
        /// </summary>
        private void Update()
        {
            // Check which keys were pressed
            var ctrlZ = Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z);
            var ctrlShiftZ = ctrlZ && Input.GetKey(KeyCode.LeftShift);
            var ctrlY = Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y);

            // Check if an action should be redone
            if (ctrlShiftZ || ctrlY) RedoAction();
            // Check if an action should be undone
            else if (ctrlZ) UndoAction();
        }

        /// <summary>
        ///     Update the buttons and the title
        /// </summary>
        private void OnEnable()
        {
            UpdateProjectView(true);
        }

        /// <summary>
        ///     Update the buttons and the title
        /// </summary>
        /// <param name="saved">True if the project is in a saved state</param>
        private void UpdateProjectView(bool saved)
        {
            // Show the title
            _projectManager.Saved = saved;
            title.text = _projectManager.CurrentProject.Name;
            if (!saved) title.text += "*";

            // Enable the buttons
            undo.Enable(_undoService.HasUndo());
            redo.Enable(_undoService.HasRedo());

            // Update the station view
            stationController.UpdateStation();
        }

        /// <summary>
        ///     Undo the last action
        /// </summary>
        public void UndoAction()
        {
            _undoService.Undo();
            UpdateProjectView(false);
        }

        /// <summary>
        ///     Redo the last action
        /// </summary>
        public void RedoAction()
        {
            _undoService.Redo();
            UpdateProjectView(false);
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
        ///     Enter the cinema mode
        /// </summary>
        public void StartCinemaMode()
        {
            // Deselect all items
            componentHighlighting.ResetPreviousSelections();

            // Center camera focus
            mainController.cameraController.ZoomOnObject(ProjectManager.Instance.CurrentProject.ObjectModel);

            // Reset camera viewport
            mainController.ResetCamera();

            // Switch screens
            mainScreen.SetActive(false);
            cinemaScreen.SetActive(true);
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
                    // Reset the undo redo queue
                    _undoService.Reset();

                    // Remove the last opened project 
                    _configManager.Config.lastProject = "";
                    _configManager.SaveConfig();
                    _projectManager.Saved = true;

                    // Reset camera
                    mainController.ResetCamera();

                    // Remove GameObject of current project
                    componentHighlighting.ResetHighlighting();
                    inputController.Reset();
                    Destroy(_projectManager.CurrentProject.ObjectModel);

                    // Show the start screen
                    mainScreen.SetActive(false);
                    startScreen.SetActive(true);
                }
            );
        }

        /// <summary>
        ///     Show the settings
        /// </summary>
        public void Settings()
        {
            settings.Show();
        }
    }
}