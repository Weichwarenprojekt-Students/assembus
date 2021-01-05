using MainScreen.Sidebar.HierarchyView;
using Shared;

namespace Services.UndoRedo.Commands
{
    public class FuseCommand : Command
    {
        /// <summary>
        ///     The id of the affected game object
        /// </summary>
        private readonly string _id;

        /// <summary>
        ///     Boolean displaying the situation of the fused group before the action
        /// </summary>
        private readonly bool _isFused;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="isFused">True if the group currently is fused</param>
        /// <param name="id">Id of the affected group</param>
        public FuseCommand(bool isFused, string id)
        {
            _isFused = isFused;
            _id = id;
        }

        /// <summary>
        ///     Undo the fusing
        /// </summary>
        public override void Undo()
        {
            SetFused(_isFused);
        }

        /// <summary>
        ///     Redo the fusing
        /// </summary>
        public override void Redo()
        {
            SetFused(!_isFused);
        }

        /// <summary>
        ///     Set the isFused state of the group to a value
        /// </summary>
        /// <param name="value">True is the group should be fused</param>
        private void SetFused(bool value)
        {
            // Get the item controller
            var listItem = Utility.FindChild(HierarchyView.transform, _id);
            var itemController = listItem.GetComponent<HierarchyItemController>();

            // Set the new state and update the hierarchy item
            itemController.itemInfo.ItemInfo.isFused = value;
            itemController.UpdateVisuals();
        }
    }
}