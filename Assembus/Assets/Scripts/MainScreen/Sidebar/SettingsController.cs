using Services;
using Services.Serialization;
using Shared.Toast;
using TMPro;
using UnityEngine;

namespace MainScreen.Sidebar
{
    public class SettingsController : MonoBehaviour
    {
        /// <summary>
        ///     The input field for the name
        /// </summary>
        public TMP_InputField nameInput;

        /// <summary>
        ///     The input field for the undo redo history length
        /// </summary>
        public TMP_InputField historyLengthInput;

        /// <summary>
        ///     The toast controller
        /// </summary>
        public ToastController toast;

        /// <summary>
        ///     The configuration manager
        /// </summary>
        private readonly ConfigurationManager _configManager = ConfigurationManager.Instance;

        /// <summary>
        ///     The project manager
        /// </summary>
        private readonly ProjectManager _projectManager = ProjectManager.Instance;

        /// <summary>
        ///     Show the settings
        /// </summary>
        public void Show()
        {
            // Load the configurations
            nameInput.text = _projectManager.CurrentProject.Name;
            historyLengthInput.text = _configManager.Config.undoHistoryLimit.ToString();

            // Show the settings
            gameObject.SetActive(true);
        }

        /// <summary>
        ///     Save the settings
        /// </summary>
        public void Save()
        {
            // Get the new number for the history
            if (int.TryParse(historyLengthInput.text, out var newValue))
            {
                _configManager.Config.undoHistoryLimit = newValue;
                if (!_configManager.SaveConfig())
                    toast.Error(Toast.Short, "Couldn't save new settings!");
                else
                    toast.Success(Toast.Short, "Settings saved successfully!");
            }
            else
            {
                historyLengthInput.text = _configManager.Config.undoHistoryLimit.ToString();
                toast.Error(Toast.Short, "Undo History Limit \n should be a number!");
            }
        }

        /// <summary>
        ///     Close the settings
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}