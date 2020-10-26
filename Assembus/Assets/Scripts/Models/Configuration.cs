using System;
using System.Collections.Generic;

namespace Models
{
    /// <summary>
    ///     This data model stores all data which is loaded/saved to the XML config file
    ///     Contains all configuration parameters for Assembus
    /// </summary>
    [Serializable]
    public class Configuration
    {
        /// <summary>
        ///     Project configuration for newly created project
        /// </summary>
        public ProjectConfig newProjectConfig = new ProjectConfig();

        /// <summary>
        ///     Project configuration for all previously generated projects
        /// </summary>
        public List<ProjectConfig> oldProjectsConfig = new List<ProjectConfig>();
    }
}