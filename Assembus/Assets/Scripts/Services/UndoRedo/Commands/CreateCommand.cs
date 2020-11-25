using MainScreen.Sidebar.HierarchyView;
using Models.Project;
using Services.UndoRedo.Models;
using Shared;
using UnityEngine;

namespace Services.UndoRedo.Commands
{
    public class CreateCommand : Command
    {
        /// <summary>
        ///     True if the command is a create command
        /// </summary>
        private readonly bool _create;

        /// <summary>
        ///     The item state
        /// </summary>
        private readonly ItemState _state;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="create">True if the command is a create command</param>
        /// <param name="state">The item state</param>
        public CreateCommand(bool create, ItemState state)
        {
            _create = create;
            _state = state;
        }

        /// <summary>
        ///     Undo the creation (or deletion)
        /// </summary>
        public override void Undo()
        {
            if (_create) Delete(_state);
            else Create(_state);
        }

        /// <summary>
        ///     Redo the creation (or deletion)
        /// </summary>
        public override void Redo()
        {
            if (_create) Create(_state);
            else Delete(_state);
        }

        /// <summary>
        ///     Create a new group item
        /// </summary>
        /// <param name="state">The item state</param>
        private static void Create(ItemState state)
        {
            // Create the object
            var newObject = new GameObject(state.ID);
            newObject.AddComponent<ItemInfoController>();
            newObject.GetComponent<ItemInfoController>().ItemInfo
                = new ItemInfo {displayName = state.Name, isGroup = true};
            newObject.transform.parent = Model.transform;

            // Create the list view item
            HierarchyController.AddSingleListItem(HierarchyView, newObject, 0);

            // Move the item
            MoveCommand.Move(state);
        }

        /// <summary>
        ///     Delete a new group item
        /// </summary>
        /// <param name="state">The item state</param>
        private static void Delete(ItemState state)
        {
            // Delete the actual object
            var item = Utility.FindChild(HierarchyView.transform, state.ID);
            var hierarchyItem = item.GetComponent<HierarchyItemController>();
            HierarchyController.DeleteObject(hierarchyItem.item);

            // Delete the list view item and the selection
            HierarchyController.SelectedItems.Remove(hierarchyItem);
            HierarchyController.DeleteObject(item.gameObject);
        }
    }
}