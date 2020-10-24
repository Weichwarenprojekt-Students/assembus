using Services;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class StartScreenController : MonoBehaviour
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

    public Text errorText;

    /// <summary>
    ///     The project manager
    /// </summary>
    private readonly ProjectManager _manager = ProjectManager.GetInstance();

    /// <summary>
    ///     Setup the UI
    /// </summary>
    private void Start()
    {
        // Hide the error view
        errorView.SetActive(false);
    }

    /// <summary>
    ///     Open the file explorer to get the path selected by the user
    /// </summary>
    public void GetDirectory()
    {
        var path = EditorUtility.OpenFolderPanel("Directory", "", "");
        if (!path.Equals("")) directoryInput.SetTextWithoutNotify(path);
    }

    /// <summary>
    ///     Open the file explorer to get the path selected by the user
    /// </summary>
    public void GetImportPath()
    {
        var path = EditorUtility.OpenFilePanel("Import Path", "", "");
        if (!path.Equals("")) importInput.SetTextWithoutNotify(path);
    }

    /// <summary>
    ///     Create a project
    /// </summary>
    public void CreateProject()
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
    }
}