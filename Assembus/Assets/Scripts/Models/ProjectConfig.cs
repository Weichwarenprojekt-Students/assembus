using System;

namespace Models
{
    /// <summary>
    ///     This data model stores the project configuration data which is required to restore GUI window settings
    /// </summary>
    [Serializable]
    public class ProjectConfig
    {
        /// <summary>
        ///     Actual project name
        /// </summary>
        public string projectName;

        /// <summary>
        ///     Directory where all project data is being saved
        /// </summary>
        public string projectDirectory;

        /// <summary>
        ///     The import path to get the 3D-objects
        /// </summary>
        public string projectImportPath;
    }
}