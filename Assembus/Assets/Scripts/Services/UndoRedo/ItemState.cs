namespace Services.UndoRedo
{
    /// <summary>
    ///     This class holds a snapshot of an item state
    /// </summary>
    public class ItemState
    {
        /// <summary>
        ///     The ID of the item
        /// </summary>
        public readonly string ID;

        /// <summary>
        ///     The name of the item
        /// </summary>
        public readonly string Name;

        /// <summary>
        ///     The ID of the parent
        /// </summary>
        public readonly string ParentID;

        /// <summary>
        ///     The sibling index
        /// </summary>
        public readonly int SiblingIndex;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="id">ID of the item</param>
        /// <param name="name">Name of the item</param>
        /// <param name="parentID">Parent ID of the item</param>
        /// <param name="siblingIndex">The sibling index</param>
        public ItemState(string id, string name, string parentID, int siblingIndex)
        {
            ID = id;
            Name = name;
            ParentID = parentID;
            SiblingIndex = siblingIndex;
        }
    }
}