using System;
using System.IO;
using Models;
using UnityEngine;

namespace Services
{
    public class ProjectManager
    {
        /// <summary>
        ///     Filename for the serialized ProjectSpace instance
        /// </summary>
        private const string ProjectConfigFile = "projectConfig.xml";

        /// <summary>
        ///     Filename for the serialized 3D model
        /// </summary>
        private const string ProjectModelFile = "projectModel.xml";

        /// <summary>
        ///     The configuration manager
        /// </summary>
        private readonly ConfigurationManager _configManager = ConfigurationManager.Instance;

        /// <summary>
        ///     The GameObject deserializer which initializes the loaded GameObject
        /// </summary>
        private readonly ModelDeSerializer _modelDeSerializer = new ModelDeSerializer();

        /// <summary>
        ///     The XML writer/reader instance to store all non 3D-model settings
        /// </summary>
        private readonly XmlDeSerializer<ProjectSpace> _xmlDeSerializer = new XmlDeSerializer<ProjectSpace>();

        /// <summary>
        ///     The current project
        /// </summary>
        public ProjectSpace CurrentProject;

        /// <summary>
        ///     Current project dir
        /// </summary>
        public string CurrentProjectDir;

        /// <summary>
        ///     True if the current project is saved
        /// </summary>
        public bool Saved = false;

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
                return (false, "The given directory \n path is invalid!");

            // Check if the import path is valid
            if (!File.Exists(importPath) || !Path.IsPathRooted(importPath))
                return (false, "The given import \n path is incorrect!");
            var objectFile = Path.GetFileName(importPath);

            // Create the project path
            var projectPath = Path.Combine(dirPath, name);

            // Check if the project directory exists
            if (Directory.Exists(projectPath))
            {
                if (!overwrite) return (false, "Directory already exists!");

                // Create a new ProjectSpace
                CurrentProject = new ProjectSpace(name, objectFile);
                CurrentProjectDir = projectPath;

                // Save the new open project in the config
                _configManager.Config.lastProject = projectPath;

                return CreateProjectFiles(projectPath, importPath);
            }

            // Otherwise try to create the directories
            try
            {
                Directory.CreateDirectory(projectPath);

                // Create a new ProjectSpace
                CurrentProject = new ProjectSpace(name, objectFile);
                CurrentProjectDir = projectPath;

                // Save the new open project in the config
                _configManager.Config.lastProject = projectPath;

                return CreateProjectFiles(projectPath, importPath);
            }
            catch (Exception)
            {
                return (false, "The target project \n could not be created!");
            }
        }

        /// <summary>
        ///     Create the necessary files for the project and copy
        ///     the import object.
        /// </summary>
        /// <param name="projectPath">The path to the project</param>
        /// <param name="importPath">The path to the import object</param>
        /// <returns>
        ///     A success flag and a message in case of an error.
        /// </returns>
        private static (bool, string) CreateProjectFiles(string projectPath, string importPath)
        {
            // Copy the file
            try
            {
                var file = Path.GetFileName(importPath);
                File.Copy(importPath, Path.Combine(projectPath, file), true);
            }
            catch (Exception)
            {
                return (false, "The model couldn't \n be imported!");
            }

            return (true, "");
        }

        /// <summary>
        ///     Save all project data from the local ProjectSpace instance to disk
        /// </summary>
        /// <returns>
        ///     A success flag and a message in case of an error.
        /// </returns>
        public (bool, string) SaveProject()
        {
            try
            {
                // Save the project configuration in XML (excluding 3D model)
                _xmlDeSerializer.SerializeData(Path.Combine(CurrentProjectDir, ProjectConfigFile), CurrentProject);

                // Save the hierarchy of the GameObject 3D model
                _modelDeSerializer.SerializeGameObject(
                    Path.Combine(CurrentProjectDir, ProjectModelFile),
                    CurrentProject.ObjectModel
                );

                return (true, "");
            }
            catch (Exception)
            {
                return (false, "The project couldn't \n be saved!");
            }
        }

        /// <summary>
        ///     Load all project data into the local ProjectSpace instance
        /// </summary>
        /// <returns>
        ///     A success flag and a message in case of an error.
        /// </returns>
        public (bool, string) LoadProject(string projectPath)
        {
            // Check if the directory path is valid
            if (projectPath.Equals("") || !Path.IsPathRooted(projectPath))
                return (false, "The given directory path is invalid!");

            // Check if the project config file exists
            var projectConfigFile = Path.Combine(projectPath, ProjectConfigFile);
            if (!File.Exists(projectConfigFile)) return (false, "Project doesn't contain \n a config file!");

            // Check if the project model file exists
            if (!File.Exists(projectConfigFile)) return (false, "Project doesn't contain \n a model file!");

            try
            {
                // Load the project configuration from XML (excluding 3D model)
                CurrentProject = _xmlDeSerializer.DeserializeData(projectConfigFile);
                CurrentProjectDir = projectPath;

                // Check if the project model file exists
                return !File.Exists(projectConfigFile)
                    ? (false, "Project doesn't contain \n an object file!")
                    : (true, "");
            }
            catch (Exception)
            {
                return (false, "The project couldn't \n be loaded!");
            }
        }

        /// <summary>
        ///     Load the object
        /// </summary>
        /// <param name="firstTime">True if the object is loaded for the first time</param>
        /// <returns>Return bool that indicates if the operation was successful and an error message</returns>
        public (bool, string) LoadGameObject(bool firstTime)
        {
            // Create the import path
            var importPath = Path.Combine(CurrentProjectDir, CurrentProject.ObjectFile);

            // Load the object
            var (success, message, objectModel) = LoadGameObjectsFromObj(importPath);
            if (!success) return (false, message);
            CurrentProject.ObjectModel = objectModel;

            // If it was the first time try to save the project
            if (firstTime) return SaveProject();

            // Otherwise try to load the object hierarchy
            try
            {
                // Load the 3D model hierarchy
                CurrentProject.ObjectModel = _modelDeSerializer.DeserializeGameObject(
                    Path.Combine(CurrentProjectDir, ProjectModelFile),
                    CurrentProject.ObjectModel
                );

                // Save the new open project in the config
                _configManager.Config.lastProject = CurrentProjectDir;
            }
            catch (Exception)
            {
                return (false, "The project couldn't \n be loaded!");
            }

            return (true, "");
        }

        /// <summary>
        ///     Imports the GameObject instances from the provided OBJ file
        /// </summary>
        /// <param name="importPath">The import path</param>
        /// <returns>The game object, a success flag and a message in case of an error</returns>
        private (bool, string, GameObject) LoadGameObjectsFromObj(string importPath)
        {
            try
            {
                GameObject importObject;
                (importObject, _) = ObjectLoader.LoadObject(importPath);
                return (true, "", importObject);
            }
            catch (Exception)
            {
                return (false, "Object could not \n be imported!", null);
            }
        }
    }
}