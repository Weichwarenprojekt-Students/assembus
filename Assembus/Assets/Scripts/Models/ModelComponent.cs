using System;

namespace Models
{
    /// <summary>
    ///     This data structure stores the data to make GameObject-Hierarchy persistent
    /// </summary>
    [Serializable]
    public class ModelComponent
    {
        /// <summary>
        ///     The name of the GameObject
        /// </summary>
        public string gameObjectName;

        /// <summary>
        ///     The name of the parent of the GameObject
        /// </summary>
        public string parentGameObjectName;
    }
}