using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using Models.Project;
using Services.Serialization;
using UnityEngine;

namespace Shared
{
    public static class DataExport
    {
        private static readonly ProjectManager ProjectManager = ProjectManager.Instance;

        /// <summary>
        ///     Export assembly data to a given file
        /// </summary>
        /// <param name="exportPath">Path to save the xml file to</param>
        /// <returns>A boolean indicating the success and an error message in case of failure</returns>
        public static (bool, string) ExportData(string exportPath)
        {
            // Save chosen file name
            ProjectManager.CurrentProject.ExportFileName = Path.GetFileName(exportPath);

            // Export the file locally first, because an error might lead to an incomplete zip archive
            var localExportPath = Path.Combine(ProjectManager.CurrentProjectDir, "export.bus");

            try
            {
                // Create new file for zip archive
                using (var zipToSave = new FileStream(localExportPath, FileMode.Create))
                {
                    using (var archive = new ZipArchive(zipToSave, ZipArchiveMode.Update))
                    {
                        // Create entry in zip file for the xml export
                        var xmlExportEntry = archive.CreateEntry(ProjectManager.CurrentProject.Name + ".xml");

                        using (var writer = new StreamWriter(xmlExportEntry.Open()))
                        {
                            // Write the xml export data to the archive entry
                            ConvertToExportXml().Save(writer);
                        }

                        // Create entry from the obj file of the project
                        var objFileName = ProjectManager.CurrentProject.ObjectFile;

                        archive.CreateEntryFromFile(
                            Path.Combine(ProjectManager.CurrentProjectDir, objFileName),
                            objFileName
                        );
                    }
                }

                // Copy the finished archive to the intended path
                File.Copy(localExportPath, exportPath, true);
            }
            catch (ModelManager.ToplevelComponentException e)
            {
                return (false, e.ComponentName + " can't be on top level!");
            }
            catch (IOException)
            {
                return (false, "Error while exporting data!");
            }
            finally
            {
                // Always delete the local export archive
                if (File.Exists(localExportPath)) File.Delete(localExportPath);
            }

            return (true, "");
        }

        /// <summary>
        ///     Convert the model data to the export XML format
        /// </summary>
        /// <returns>XElement representing the current internal data structure</returns>
        private static XElement ConvertToExportXml()
        {
            // Get the current model
            var currentObject = ProjectManager.CurrentProject.ObjectModel.transform;

            // Create a root element for the XML file
            var rootElement = new XElement("AssemblyLine");

            // For every station add the XML structure to the root element
            for (var i = 0; i < currentObject.transform.childCount; i++)
                rootElement.Add(ConvertToExportXml(currentObject.GetChild(i), true));

            return rootElement;
        }

        /// <summary>
        ///     Convert sub components into xml structure for data export
        /// </summary>
        /// <param name="parent">The element which needs to be converted to xml</param>
        /// <param name="topLevel">True if parent is in the top level of the model</param>
        /// <returns>XElement with all information of an item and its possible children</returns>
        private static XElement ConvertToExportXml(Transform parent, bool topLevel = false)
        {
            // Get item info
            var itemInfo = parent.GetComponent<ItemInfoController>().ItemInfo;

            // Check that a top level item is also a group (station)
            if (topLevel && !itemInfo.isGroup)
                throw new ModelManager.ToplevelComponentException {ComponentName = itemInfo.displayName};

            // Get the type for the xml element
            string type;
            if (topLevel) type = "Station";
            else if (itemInfo.isGroup) type = "Group";
            else type = "Component";

            // Generate the xml element for the current item
            var elem = new XElement(
                type,
                new XAttribute("name", itemInfo.displayName),
                new XAttribute("id", parent.name)
            );

            // Add attribute for fused groups
            if (!topLevel && itemInfo.isFused) elem.Add(new XAttribute("isFused", true));

            // Add xml elements for all children recursively
            if (itemInfo.isGroup)
                for (var i = 0; i < parent.childCount; i++)
                    elem.Add(ConvertToExportXml(parent.GetChild(i)));

            return elem;
        }
    }
}