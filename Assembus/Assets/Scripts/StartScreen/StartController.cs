using System.Linq;
using Models.AppConfiguration;
using Models.Project;
using Services;
using SFB;
using Shared;
using Shared.LoadingScreen;
using Shared.Toast;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StartScreen
{
    public class StartController : MonoBehaviour
    {
        /// <summary>
        ///     The input fields for the project creation
        /// </summary>
        public TMP_InputField nameInput, directoryInput, importInput;

        /// <summary>
        ///     The toggle for the overwrite function
        /// </summary>
        public Toggle overwriteToggle;

        /// <summary>
        ///     The listview item for old projects
        /// </summary>
        public GameObject defaultListViewItem;

        /// <summary>
        ///     A prefab instance which is used to create new listview items
        /// </summary>
        public GameObject listView;

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
        ///     The loading screen controller
        /// </summary>
        public LoadingController loadingScreen;

        /// <summary>
        ///     ConfigurationManager singleton to save/load config and handle XML serialization
        /// </summary>
        private readonly ConfigurationManager _configManager = ConfigurationManager.Instance;

        /// <summary>
        ///     The project manager
        /// </summary>
        private readonly ProjectManager _projectManager = ProjectManager.Instance;

        /// <summary>
        ///     Check if tab was clicked
        /// </summary>
        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Tab)) return;

            // Check which input is currently focussed
            if (nameInput.isFocused)
                directoryInput.Select();
            else if (directoryInput.isFocused)
                importInput.Select();
            else if (importInput.isFocused)
                nameInput.Select();
            else
                nameInput.Select();
        }

        /// <summary>
        ///     Setup the UI
        /// </summary>
        private void OnEnable()
        {
            // Apply old configuration to GUI window
            LoadWindowConfig();
            // Check if there was a project open
            ReopenLastProject();
        }

        /// <summary>
        ///     Open the file explorer to get the path selected by the user
        /// </summary>
        public void GetDirectory()
        {
            var paths = StandaloneFileBrowser.OpenFolderPanel("", "", false);
            if (paths.Length != 0 && !paths[0].Equals("")) directoryInput.SetTextWithoutNotify(paths[0]);
            directoryInput.caretPosition = directoryInput.text.Length;
        }

        /// <summary>
        ///     Open the file explorer to get the path selected by the user
        /// </summary>
        public void GetImportPath()
        {
            var paths = StandaloneFileBrowser.OpenFilePanel("", "", "obj", false);
            if (paths.Length != 0 && !paths[0].Equals("")) importInput.SetTextWithoutNotify(paths[0]);
        }

        /// <summary>
        ///     Import a project
        /// </summary>
        public void ImportProject()
        {
            // Check the path
            var paths = StandaloneFileBrowser.OpenFolderPanel("", "", false);
            if (paths.Length == 0 || paths[0].Equals("")) return;
            dialog.Show(
                "Open Project",
                "Open the project?",
                () =>
                {
                    // Show the loading screen
                    loadingScreen.ShowLoadingScreen(
                        () => _projectManager.LoadProject(paths[0]),
                        result =>
                        {
                            // Check if the object was loaded
                            var (success, message) = result;
                            if (!success)
                            {
                                toast.Error(Toast.Short, message);
                                return;
                            }

                            // Load the OBJ model
                            var (loadSuccess, loadMessage) = _projectManager.LoadGameObject(false);
                            if (!loadSuccess)
                            {
                                toast.Error(Toast.Short, loadMessage);
                                return;
                            }

                            // Show the new project
                            startScreen.SetActive(false);
                            mainScreen.SetActive(true);
                            SaveWindowConfig();
                        },
                        2000
                    );
                }
            );
        }

        /// <summary>
        ///     Create a project
        /// </summary>
        public void CreateProject()
        {
            dialog.Show(
                "Create Project",
                "Create the project " + nameInput.text + "?",
                () =>
                {
                    // Get the input data
                    var projectName = nameInput.text;
                    var dir = directoryInput.text;
                    var importPath = importInput.text;
                    var overwrite = overwriteToggle.isOn;

                    // Try to create the project
                    loadingScreen.ShowLoadingScreen(
                        () => _projectManager.CreateProject(projectName, dir, importPath, overwrite),
                        result =>
                        {
                            // Check if creation was successful
                            var (success, message) = result;
                            if (!success)
                            {
                                // Show the error
                                toast.Error(Toast.Short, message);
                                return;
                            }

                            var (loadSuccess, loadMessage) = _projectManager.LoadGameObject(true);
                            if (!loadSuccess)
                            {
                                // Show the error
                                toast.Error(Toast.Short, loadMessage);
                                return;
                            }

                            // Show the main screen
                            startScreen.SetActive(false);
                            mainScreen.SetActive(true);

                            // Write window config to XML file before leaving this window
                            SaveWindowConfig();
                        },
                        3000
                    );
                }
            );
        }

        /// <summary>
        ///     Loads the configuration file from disk and applies config to GUI window.
        ///     This includes changing the listview and the fields for new projects.
        /// </summary>
        private void LoadWindowConfig()
        {
            // Apply data to the GUI elements. No need to load XML file first, 
            // as this has been already done in configManager constructor
            nameInput.text = _configManager.Config.newProjectConfig.projectName;
            directoryInput.text = _configManager.Config.newProjectConfig.projectDirectory;
            importInput.text = _configManager.Config.newProjectConfig.projectImportPath;

            // Remove old items
            for (var i = 1; i < listView.transform.childCount; i++) Destroy(listView.transform.GetChild(i).gameObject);

            // Show the default item
            defaultListViewItem.SetActive(true);

            // Apply data to listview. Iterate oldProjects list backwards
            for (var i = _configManager.Config.oldProjectsConfig.Count - 1; i >= 0; i--)
            {
                // Create new listview item by instantiating a new prefab
                var newListViewItem = Instantiate(
                    defaultListViewItem,
                    listView.transform,
                    true
                );

                // Get text components of listview item GameObjects
                var projectText = newListViewItem.transform.Find("Name").GetComponent<TextMeshProUGUI>();
                var descriptionText = newListViewItem.transform.Find("Description").GetComponent<TextMeshProUGUI>();
                var deleteButton = newListViewItem.transform.Find("Delete").GetComponent<Button>();

                // Set the values
                var projectName = _configManager.Config.oldProjectsConfig[i].projectName;
                var projectPath = _configManager.Config.oldProjectsConfig[i].projectDirectory;
                projectText.text = projectName;
                descriptionText.text = projectPath;

                // Set the on click for the list item
                newListViewItem.GetComponent<Button>().onClick.AddListener(
                    () => { OpenProject(projectName, projectPath); }
                );

                // Add new OnClick listener to remove items from listview and update the XML file
                deleteButton.onClick.AddListener(
                    () => { DeleteItem(projectName, projectPath, newListViewItem); }
                );
            }

            // Hide the default item
            defaultListViewItem.SetActive(false);
        }

        /// <summary>
        ///     Open a project
        /// </summary>
        /// <param name="project">The name of the project</param>
        /// <param name="projectPath">The path to the project's directory</param>
        private void OpenProject(string project, string projectPath)
        {
            dialog.Show(
                "Open Project",
                "Open the project " + project + "?",
                () =>
                {
                    // Show the loading screen
                    loadingScreen.ShowLoadingScreen(
                        () => _projectManager.LoadProject(projectPath),
                        result =>
                        {
                            // Check if the object was loaded
                            var (success, message) = result;
                            if (!success)
                            {
                                toast.Error(Toast.Short, message);
                                return;
                            }

                            // Load the OBJ model
                            var (loadSuccess, loadMessage) = _projectManager.LoadGameObject(false);
                            if (!loadSuccess)
                            {
                                toast.Error(Toast.Short, loadMessage);
                                return;
                            }

                            // Show the new project
                            startScreen.SetActive(false);
                            mainScreen.SetActive(true);
                            SaveWindowConfig();
                        },
                        2000
                    );
                }
            );
        }

        /// <summary>
        ///     Delete an item on click
        /// </summary>
        /// <param name="project">The project name</param>
        /// <param name="description">The project's description (the directory path)</param>
        /// <param name="item">The game object to be deleted</param>
        private void DeleteItem(string project, string description, Object item)
        {
            dialog.Show(
                "Delete Project",
                "Delete the project " + project + "?",
                () =>
                {
                    // Remove existing entry
                    _configManager.Config.oldProjectsConfig = _configManager.Config.oldProjectsConfig
                        .Where(conf => conf.projectDirectory != description).ToList();

                    // Remove current listview item
                    Destroy(item);

                    // Write the XML file
                    _configManager.SaveConfig();
                }
            );
        }

        /// <summary>
        ///     Stores the configuration from the GUI window to disk
        /// </summary>
        private void SaveWindowConfig()
        {
            // Create new config for new project
            var newConfig = new ProjectConfig
            {
                projectName = _projectManager.CurrentProject.Name,
                projectDirectory = directoryInput.text,
                projectImportPath = importInput.text
            };

            _configManager.Config.newProjectConfig = newConfig;

            // Add new config to project history/old projects
            var oldConfig = new ProjectConfig
            {
                projectName = _projectManager.CurrentProject.Name,
                projectDirectory = _projectManager.CurrentProjectDir
            };

            // Remove entry of existing project in oldProjects
            _configManager.Config.oldProjectsConfig = _configManager.Config.oldProjectsConfig
                .Where(conf => conf.projectDirectory != _projectManager.CurrentProjectDir).ToList();

            _configManager.Config.oldProjectsConfig.Add(oldConfig);

            // Write the XML file
            _configManager.SaveConfig();
        }

        /// <summary>
        ///     Check if there was a last project
        /// </summary>
        private void ReopenLastProject()
        {
            // If there's no path given 
            var lastProject = _configManager.Config.lastProject;
            if (lastProject.Equals("")) return;

            // Try to load the project
            var (success, _) = _projectManager.LoadProject(lastProject);
            if (!success) return;

            // Load the OBJ model
            var (loadSuccess, _) = _projectManager.LoadGameObject(false);
            if (!loadSuccess) return;

            // Show the main screen
            startScreen.SetActive(false);
            mainScreen.SetActive(true);
        }
    }
}