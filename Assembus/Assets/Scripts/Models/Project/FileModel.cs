using System;

namespace Models.Project
{
    /// <summary>
    ///     This data structure defines the content of the XML file
    ///     and stores all data which is required to save/restore the
    ///     hierarchy and the configuration parameters of the model's GameObjects
    /// </summary>
    [Serializable]
    public class FileModel
    {
        /// <summary>
        ///     The fixed id (original name from OBJ file, if not a group) of the GameObject
        /// </summary>
        public string id;

        /// <summary>
        ///     The fixed id (original name from OBJ file, if not a group) of the parent of the GameObject
        /// </summary>
        public string parentId;

        /// <summary>
        ///     Additional information about the GameObject which cannot be stored in the GameObject directly
        /// </summary>
        public ItemInfo itemInfo;
    }
}