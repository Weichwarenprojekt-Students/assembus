using System;
using System.Linq;
using Models;
using Services;
using SFB;
using Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StartScreen
{
    public class UIController : MonoBehaviour
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
        ///     The error view
        /// </summary>
        public GameObject errorView;

        /// <summary>
        ///     The error text
        /// </summary>
        public TextMeshProUGUI errorText;

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
        ///     The dialog
        /// </summary>
        public DialogController dialog;

        /// <summary>
        ///     ConfigurationManager singleton to save/load config and handle XML serialization
        /// </summary>
        private readonly ConfigurationManager _configManager = ConfigurationManager.Instance;

        /// <summary>
        ///     The project manager
        /// </summary>
        private readonly ProjectManager _manager = ProjectManager.Instance;

        /// <summary>
        ///     Setup the UI
        /// </summary>
        private void Start()
        {
            // Hide the error view
            errorView.SetActive(false);

            //Apply old configuration to GUI window
            LoadWindowConfig();
        }

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
        ///     Create a project
        /// </summary>
        public void CreateProject()
        {
            dialog.Show("Create Project", "Create the project " + nameInput.text + "?", () =>
            {
                // Get the input data
                var projectName = nameInput.text;
                var dir = directoryInput.text;
                var importPath = importInput.text;
                var overwrite = overwriteToggle.isOn;

                // Try to create the project
                var (success, message) = _manager.CreateProject(projectName, dir, importPath, overwrite);

                // Check if creation was successful
                if (success)
                {
                    // Hide the error
                    errorView.SetActive(false);

                    // Show the main screen
                    startScreen.SetActive(false);
                    mainScreen.SetActive(true);

                    //Write window config to XML file before leaving this window
                    SaveWindowConfig();
                }
                else
                {
                    // Show the error
                    errorView.SetActive(true);
                    errorText.text = message;
                }
            });
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

                // Add new OnClick listener to remove items from listview and update the XML file
                deleteButton.onClick.AddListener(() =>
                {
                    dialog.Show(
                        "Delete Project",
                        "Delete the project " + projectText.text + "?",
                        () =>
                        {
                            // Remove existing entry
                            _configManager.Config.oldProjectsConfig = _configManager.Config.oldProjectsConfig
                                .Where(conf => conf.projectDirectory != descriptionText.text).ToList();

                            // Remove current listview item
                            Destroy(newListViewItem);

                            // Write the XML file
                            _configManager.SaveConfig();
                        });
                });

                projectText.text = _configManager.Config.oldProjectsConfig[i].projectName;
                descriptionText.text = _configManager.Config.oldProjectsConfig[i].projectDirectory;
            }

            // Hide the default item
            defaultListViewItem.SetActive(false);
        }

        /// <summary>
        ///     Stores the configuration from the GUI window to disk
        /// </summary>
        private void SaveWindowConfig()
        {
            // Create new config for new project
            var newConfig = new ProjectConfig
            {
                projectName = _manager.CurrentProject.Name,
                projectDirectory = directoryInput.text,
                projectImportPath = importInput.text
            };

            _configManager.Config.newProjectConfig = newConfig;

            // Add new config to project history/old projects
            var oldConfig = new ProjectConfig
            {
                projectName = _manager.CurrentProject.Name,
                projectDirectory = _manager.CurrentProjectDir
            };

            if (overwriteToggle.isOn)
                // Remove entry of existing project in oldProjects
                _configManager.Config.oldProjectsConfig = _configManager.Config.oldProjectsConfig
                    .Where(conf => conf.projectDirectory != _manager.CurrentProjectDir).ToList();

            _configManager.Config.oldProjectsConfig.Add(oldConfig);

            // Write the XML file
            _configManager.SaveConfig();
        }
    }
}