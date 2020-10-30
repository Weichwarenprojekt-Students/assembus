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
        ///     The GameObject deserializer which initializes the loaded GameObject
        /// </summary>
        private readonly GameObjectDeSerializer _gameObjectDeSerializer = new GameObjectDeSerializer();

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

                // Import the OBJ model and convert it to GameObject with default hierarchy
                var importModel = LoadGameObjectsFromObj(importPath);

                // Create a new ProjectSpace
                CurrentProject = new ProjectSpace(name, importModel);
                CurrentProjectDir = projectPath;

                return (true, "Project successfully created");
            }

            // Otherwise try to create the directories
            try
            {
                Directory.CreateDirectory(projectPath);

                // Import the OBJ model and convert it to GameObject with default hierarchy
                var importModel = LoadGameObjectsFromObj(importPath);

                // Create a new ProjectSpace
                CurrentProject = new ProjectSpace(name, importModel);
                CurrentProjectDir = projectPath;

                return (true, "Project successfully created");
            }
            catch (Exception)
            {
                return (false, "The target directory or project could not be created!");
            }
        }

        /// <summary>
        ///     Save all project data from the local ProjectSpace instance to disk
        /// </summary>
        public bool SaveProject(string name, string dirPath)
        {
            try
            {
                // Save the project configuration in XML (excluding 3D model)
                _xmlDeSerializer.SerializeData(Path.Combine(CurrentProjectDir, ProjectConfigFile), CurrentProject);

                // Save the hierarchy of the GameObject 3D model
                _gameObjectDeSerializer.SerializeGameObject(
                    Path.Combine(CurrentProjectDir, ProjectModelFile),
                    CurrentProject.ObjectModel
                );

                return true;
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't write the project files: " + e.Message);
                return false;
            }
        }

        /// <summary>
        ///     Load all project data into the local ProjectSpace instance
        /// </summary>
        public (bool, string) LoadProject(string name, string dirPath, string importPath)
        {
            // Check if the name is empty
            if (name.Equals("")) return (false, "Name is empty!");

            // Check if the directory path is valid
            if (dirPath.Equals("") || !Path.IsPathRooted(dirPath))
                return (false, "The given directory path is invalid!");

            // Check if the import path is valid
            if (!File.Exists(importPath) || !Path.IsPathRooted(importPath))
                return (false, "The given import path is incorrect!");

            //    Initialize the project dir when loading the project
            CurrentProjectDir = Path.Combine(dirPath, name);

            try
            {
                // Load the project configuration from XML (excluding 3D model)
                CurrentProject = _xmlDeSerializer.DeserializeData(Path.Combine(CurrentProjectDir, ProjectConfigFile));

                // Load the OBJ model
                CurrentProject.ObjectModel = LoadGameObjectsFromObj(importPath);

                // Load the 3D model hierarchy
                CurrentProject.ObjectModel = _gameObjectDeSerializer.DeserializeGameObject(
                    Path.Combine(CurrentProjectDir, ProjectModelFile),
                    CurrentProject.ObjectModel
                );

                return (true, "");
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't load the project configuration files: " + e.Message);
                return (false, "Couldn't load the project configuration files: ");
            }
        }

        /// <summary>
        ///     Imports the GameObject instances from the provided OBJ file
        /// </summary>
        /// <returns></returns>
        public GameObject LoadGameObjectsFromObj(string importPath)
        {
            GameObject importObject;
            (importObject, _) = ObjectLoader.LoadObject(importPath);
            return importObject;
        }
    }
}