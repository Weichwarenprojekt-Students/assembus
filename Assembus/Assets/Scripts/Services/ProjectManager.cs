using System;
using System.IO;
using Models;

namespace Services
{
    public class ProjectManager
    {
        /// <summary>
        ///     The current project
        /// </summary>
        public ProjectSpace CurrentProject;

        /// <summary>
        ///     Current project dir
        /// </summary>
        public string CurrentProjectDir;

        /// <summary>
        ///     Make constructor private
        /// </summary>
        private ProjectManager()
        {
        }

        /// <summary>
        ///     The singleton
        /// </summary>
        public static ProjectManager Instance { get; } = new ProjectManager();

        /// <summary>
        ///     Try to create a new project
        /// </summary>
        /// <param name="name">The name of the project</param>
        /// <param name="dirPath">The path to the directory in which the project shall be created</param>
        /// <param name="importPath">The path to the model that shall be imported</param>
        /// <param name="overwrite">True if the directory can be overwritten</param>
        /// <returns>
        ///     A tuple containing a bool that is true if the operation was
        ///     successful and a string that contains a message in the case
        ///     of an error
        /// </returns>
        public (bool, string) CreateProject(string name, string dirPath, string importPath, bool overwrite)
        {
            // Check if the name is empty
            if (name.Equals("")) return (false, "Name is empty!");

            // Check if the directory path is valid
            if (dirPath.Equals("") || !Path.IsPathRooted(dirPath))
                return (false, "The given directory path is invalid!");

            // Check if the import path is valid
            if (!File.Exists(importPath) || !Path.IsPathRooted(importPath))
                return (false, "The given import path is incorrect!");

            // Create the project path
            var projectPath = Path.Combine(dirPath, name);

            // Check if the project directory exists
            if (Directory.Exists(projectPath))
            {
                if (!overwrite) return (false, "Directory already exists!");
                CurrentProject = new ProjectSpace(name);
                CurrentProjectDir = projectPath;
            }
            else
            {
                // Otherwise try to create the directories
                try
                {
                    Directory.CreateDirectory(projectPath);
                    CurrentProject = new ProjectSpace(name);
                    CurrentProjectDir = projectPath;
                }
                catch (Exception)
                {
                    return (false, "The target directory could not be created!");
                }
            }

            // Load object model into the MainScene
            var (parent, children) = ObjectLoader.LoadObject(importPath);
            if (parent == null)
            {
                return (false, "Object model could not be initialized.");
            }

            return (true, "");
        }
    }
}