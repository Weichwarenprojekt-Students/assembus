using System;
using System.Windows.Forms;
using Services;
using SFB;
using UnityEngine;
using UnityEngine.UI;

namespace StartScreen
{
    public class UIController : MonoBehaviour
    {
        /// <summary>
        ///     The input fields for the project creation
        /// </summary>
        public InputField nameInput, directoryInput, importInput;

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
        public Text errorText;

        /// <summary>
        ///     The dialog panel
        /// </summary>
        public GameObject dialog;

        /// <summary>
        ///     The dialog text
        /// </summary>
        public Text title, description;

        /// <summary>
        ///     The project manager
        /// </summary>
        private readonly ProjectManager _manager = ProjectManager.GetInstance();

        /// <summary>
        ///     The current action for the dialog
        /// </summary>
        private Action _dialogAction;

        /// <summary>
        ///     Check if tab was clicked
        /// </summary>
        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Tab)) return;
        
            // Check which input is currently focussed
            if (nameInput.isFocused)
            {
                directoryInput.Select();
            }
            else if (directoryInput.isFocused)
            {
                importInput.Select();
            }
            else if (importInput.isFocused)
            {
                nameInput.Select();
            }
            else
            {
                nameInput.Select();
            }
        }

        /// <summary>
        ///     Open the file explorer to get the path selected by the user
        /// </summary>
        public void GetDirectory()
        {
            var paths = StandaloneFileBrowser.OpenFolderPanel("", "", false);
            if (paths.Length != 0 && !paths[0].Equals("")) directoryInput.SetTextWithoutNotify(paths[0]);
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
            ShowDialog("Create", "Create the project?", () =>
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
        ///     Show the dialog
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="description">The description of the dialog</param>
        /// <param name="action">The action that shall be executed on confirm</param>
        private void ShowDialog(string title, string description, Action action)
        {
            // Set the title
            this.title.text = title;
            // Set the description
            this.description.text = description;
            // Set the action
            _dialogAction = action;
            // Show the dialog
            dialog.SetActive(true);
        }

        /// <summary>
        ///     Execute the dialog action
        /// </summary>
        public void ConfirmDialog()
        {
            _dialogAction();
            dialog.SetActive(false);
        }

        /// <summary>
        ///     Close the dialog
        /// </summary>
        public void CancelDialog()
        {
            dialog.SetActive(false);
        }
    }
}