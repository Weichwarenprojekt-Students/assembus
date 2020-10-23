using System;
using System.IO;
using Models;
using UnityEditor;

namespace Services
{
    public class ProjectManager
    {
        /// <summary>
        ///     The current project
        /// </summary>
        public ProjectSpace currentProject;

        /// <summary>
        ///     Open the file explorer and select a path
        /// </summary>
        /// <param name="title">The title of the file explorer</param>
        /// <param name="startingDir">The starting directory</param>
        /// <returns>The selected path</returns>
        public static string GetPath(string title, string startingDir)
        {
            return EditorUtility.OpenFilePanel(title, startingDir, "");
        }

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
            if (name.Equals(""))
            {
                return (false, "Name is empty!");
            }
            
            // Check if the import path is valid
            if (!File.Exists(importPath))
            {
                return (false, "The given import path is incorrect!");
            }

            // Create the project path
            var projectPath = Path.Combine(dirPath, name);
            
            // Check if the project directory exists
            if (Directory.Exists(projectPath))
            {
                if (!overwrite)
                {
                    return (false, "Directory already exists!");
                }
                currentProject = new ProjectSpace(name);
            }
            else
            {
                // Otherwise try to create the directories
                try
                {
                    Directory.CreateDirectory(projectPath);
                    currentProject = new ProjectSpace(name);
                }
                catch (Exception)
                {
                    return (false, "The target directory could not be created!");
                }
            }
            return (true, "");
        }
        
        
    }
}