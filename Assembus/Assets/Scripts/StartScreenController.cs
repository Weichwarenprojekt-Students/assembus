using Services;
using SFB;
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

    /// <summary>
    ///     The error text
    /// </summary>
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