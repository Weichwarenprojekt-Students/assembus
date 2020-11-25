using MainScreen.Sidebar.HierarchyView;
using Models.Project;
using Shared;

namespace Services.UndoRedo.Models
{
    /// <summary>
    ///     This class holds a snapshot of an item state
    /// </summary>
    public class ItemState
    {
        /// <summary>
        ///     Neighbour ID for an element that shall be in last place
        /// </summary>
        public const string Last = "Last";

        /// <summary>
        ///     The ID of the item
        /// </summary>
        public readonly string ID;

        /// <summary>
        ///     The name of the item
        /// </summary>
        public readonly string Name;

        /// <summary>
        ///     The id of the lower neighbour
        /// </summary>
        public string NeighbourID;

        /// <summary>
        ///     The ID of the parent
        /// </summary>
        public string ParentID;

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="item">The item to be copied</param>
        public ItemState(ItemState item)
        {
            ID = item.ID;
            Name = item.Name;
            ParentID = item.ParentID;
            NeighbourID = item.NeighbourID;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="id">ID of the item</param>
        /// <param name="name">Name of the item</param>
        /// <param name="parentID">Parent ID of the item</param>
        /// <param name="neighbourID">The id of the lower neighbour</param>
        public ItemState(string id, string name, string parentID, string neighbourID)
        {
            ID = id;
            Name = name;
            ParentID = parentID;
            NeighbourID = neighbourID;
        }

        /// <summary>
        ///     Create an item state snapshot from a list item
        /// </summary>
        /// <param name="item">The item of the hierarchy view</param>
        /// <returns>The created item state</returns>
        public ItemState(HierarchyItemController item)
        {
            var gameItem = item.item;
            var itemInfo = gameItem.GetComponent<ItemInfoController>().ItemInfo;
            ID = gameItem.name;
            Name = itemInfo.displayName;
            ParentID = gameItem.transform.parent.name;
            NeighbourID = Utility.GetNeighbourID(gameItem.transform);
        }

        /// <summary>
        ///     Provide a human readable version of the object for debugging
        /// </summary>
        /// <returns>The object in string format</returns>
        public override string ToString()
        {
            return "Item(id=" + ID + ", name=" + Name + ", parent=" + ParentID + ", upper=" + NeighbourID + ")";
        }
    }
}