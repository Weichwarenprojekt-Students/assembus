using MainScreen.Sidebar.HierarchyView;
using Models.Project;

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
        public string ID;

        /// <summary>
        ///     The name of the item
        /// </summary>
        public string Name;

        /// <summary>
        ///     The ID of the parent
        /// </summary>
        public string ParentID;

        /// <summary>
        ///     The sibling index
        /// </summary>
        public int SiblingIndex;

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="item">The item to be copied</param>
        public ItemState(ItemState item)
        {
            ID = item.ID;
            Name = item.Name;
            ParentID = item.ParentID;
            SiblingIndex = item.SiblingIndex;
        }

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

        /// <summary>
        ///     Create an item state snapshot from a list item
        /// </summary>
        /// <param name="item">The item of the hierarchy view</param>
        /// <returns>The created item state</returns>
        public static ItemState FromListItem(HierarchyItemController item)
        {
            var gameItem = item.item;
            var itemInfo = gameItem.GetComponent<ItemInfoController>().ItemInfo;
            return new ItemState(
                gameItem.name,
                itemInfo.displayName,
                gameItem.transform.parent.name,
                gameItem.transform.GetSiblingIndex()
            );
        }

        /// <summary>
        ///     Provide a human readable version of the object for debugging
        /// </summary>
        /// <returns>The object in string format</returns>
        public override string ToString()
        {
            return "Item(id=" + ID + ", name=" + Name + ", parent=" + ParentID + ", index=" + SiblingIndex + ")";
        }
    }
}