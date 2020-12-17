using System.IO;
using System.Xml.Linq;
using MainScreen.StationView;
using Models.Project;
using Services.Serialization;
using Services.UndoRedo;
using SFB;
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
        ///     The controller of the station view
        /// </summary>
        public StationController stationController;

        /// <summary>
        ///     The component highlighting script
        /// </summary>
        public ComponentHighlighting componentHighlighting;

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
        ///     Export the assembly data to hard disk
        /// </summary>
        public void ExportData()
        {
            // Get the name of the previously exported file
            var previousExportFileName = _projectManager.CurrentProject.ExportFileName;

            // Open save dialog for the user
            var exportPath = StandaloneFileBrowser.SaveFilePanel(
                "Export Data",
                "",
                string.IsNullOrEmpty(previousExportFileName)
                    ? _projectManager.CurrentProject.Name
                    : previousExportFileName,
                "xml"
            );

            // If save dialog was canceled, string is empty
            if (string.IsNullOrEmpty(exportPath)) return;

            // Save chosen file name
            _projectManager.CurrentProject.ExportFileName = Path.GetFileName(exportPath);

            try
            {
                // Export data structure to XML file
                ConvertToExportXml().Save(exportPath);

                toast.Success(Toast.Short, "Data was exported successfully!");
            }
            catch (ModelManager.ToplevelComponentException e)
            {
                toast.Error(Toast.Short, e.ComponentName + " can't be on top level!");
            }
            catch (IOException)
            {
                toast.Error(Toast.Short, "Error while exporting data!");
            }
        }

        /// <summary>
        ///     Convert the model data to the export XML format
        /// </summary>
        /// <returns></returns>
        private XElement ConvertToExportXml()
        {
            // Get the current model
            var currentObject = _projectManager.CurrentProject.ObjectModel.transform;

            // Create a root element for the XML file
            var rootElement = new XElement("AssemblyLine");

            // For every station add the XML structure to the root element
            for (var i = 0; i < currentObject.transform.childCount; i++)
                rootElement.Add(ConvertToExportXml(currentObject.GetChild(i), true));

            return rootElement;
        }

        /// <summary>
        ///     Convert sub components into xml structure for data export
        /// </summary>
        /// <param name="parent">The element which needs to be converted to xml</param>
        /// <param name="topLevel">True if parent is in the top level of the model</param>
        /// <returns>XElement with all information of an item and its possible children</returns>
        private static XElement ConvertToExportXml(Transform parent, bool topLevel = false)
        {
            // Get item info
            var itemInfo = parent.GetComponent<ItemInfoController>().ItemInfo;

            // Check that a top level item is also a group (station)
            if (topLevel && !itemInfo.isGroup)
                throw new ModelManager.ToplevelComponentException {ComponentName = itemInfo.displayName};

            // Get the type for the xml element
            string type;
            if (topLevel) type = "Station";
            else if (itemInfo.isGroup) type = "Group";
            else type = "Component";

            // Generate the xml element for the current item
            var elem = new XElement(
                type,
                new XAttribute("name", itemInfo.displayName),
                new XAttribute("id", parent.name)
            );

            // Add attribute for fused groups
            if (!topLevel && itemInfo.isFused) elem.Add(new XAttribute("isFused", true));

            // Add xml elements for all children recursively
            if (itemInfo.isGroup)
                for (var i = 0; i < parent.childCount; i++)
                    elem.Add(ConvertToExportXml(parent.GetChild(i)));

            return elem;
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
                    Destroy(_projectManager.CurrentProject.ObjectModel);

                    // Show the start screen
                    mainScreen.SetActive(false);
                    startScreen.SetActive(true);
                }
            );
        }
    }
}