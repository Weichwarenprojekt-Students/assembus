using MainScreen.Sidebar.HierarchyView;
using Models.Project;
using Shared;

namespace Services.UndoRedo.Commands
{
    public class RenameCommand : Command
    {
        /// <summary>
        ///     The id of the item that shall be renamed
        /// </summary>
        private readonly string _id;

        /// <summary>
        ///     The different name states
        /// </summary>
        private readonly string _oldName, _newName;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="id">The id of the item</param>
        /// <param name="oldName">The old name of the item</param>
        /// <param name="newName">The new name of the item</param>
        public RenameCommand(string id, string oldName, string newName)
        {
            _id = id;
            _oldName = oldName;
            _newName = newName;
        }

        /// <summary>
        ///     Undo the renaming
        /// </summary>
        public override void Undo()
        {
            Rename(_id, _oldName);
        }

        /// <summary>
        ///     Rename the item
        /// </summary>
        public override void Redo()
        {
            Rename(_id, _newName);
        }

        /// <summary>
        ///     Change the name of an item
        /// </summary>
        /// <param name="id">The id of the item</param>
        /// <param name="name">The new name of the item</param>
        private static void Rename(string id, string name)
        {
            // Get the item and change the name on it
            var listItem = Utility.FindChild(HierarchyView.transform, id);
            var nameLabel = listItem.GetComponent<HierarchyItemController>().nameText;
            nameLabel.text = name;

            // Change the display name in the actual object
            var modelItem = Utility.FindChild(Model.transform, id);
            var itemInfo = modelItem.GetComponent<ItemInfoController>().ItemInfo;
            itemInfo.displayName = name;
        }
    }
}