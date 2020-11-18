using System;

namespace Models.Project
{
    /// <summary>
    ///     This data structure stores additional information about a given GameObject.
    ///     Using a memento because class instances of ItemInfoController inherit from MonoBehaviour,
    ///     which cannot be serialized.
    /// </summary>
    [Serializable]
    public class ItemInfo
    {
        /// <summary>
        ///     Stores the variable displayed name (not the id) of the GameObject
        /// </summary>
        public string displayName;

        /// <summary>
        ///     Stores if this GameObject is a component group
        /// </summary>
        public bool isGroup;
    }
}