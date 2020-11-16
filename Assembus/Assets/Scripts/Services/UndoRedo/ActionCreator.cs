using System;
using MainScreen.Sidebar.HierarchyView;
using Models.Project;
using UnityEngine;

namespace Services.UndoRedo
{
    public static class ActionCreator
    {
        /// <summary>
        ///     The actual model
        /// </summary>
        private static GameObject _model;

        /// <summary>
        ///     The list view
        /// </summary>
        private static GameObject _hierarchyView;

        /// <summary>
        ///     Initialize the command executor
        /// </summary>
        /// <param name="model">The actual model</param>
        /// <param name="hierarchyView">The list view</param>
        public static void Initialize(GameObject model, GameObject hierarchyView)
        {
            _model = model;
            _hierarchyView = hierarchyView;
        }

        /// <summary>
        ///     Find the right action for a given command type
        /// </summary>
        /// <param name="commandType">The command type</param>
        /// <returns>The matching action</returns>
        public static Action<ItemState> GetUndoAction(int commandType)
        {
            switch (commandType)
            {
                case Command.Move:
                    return Move;
                
                case Command.Rename:
                    return Rename;

                default:
                    return item => { };
            }
        }

        /// <summary>
        ///     Find the right action for a given command type
        /// </summary>
        /// <param name="commandType">The command type</param>
        /// <returns>The matching action</returns>
        public static Action<ItemState> GetRedoAction(int commandType)
        {
            switch (commandType)
            {
                case Command.Move:
                    return Move;
                
                case Command.Rename:
                    return Rename;

                default:
                    return item => { };
            }
        }

        /// <summary>
        ///     The action for moving an item
        /// </summary>
        /// <param name="state">The item state</param>
        private static void Move(ItemState state)
        {
            // Get the item and the parent
            var listItem = Utility.FindChild(_hierarchyView.transform, state.ID);
            var container = _hierarchyView.transform;
            var listParent = Utility.FindChild(_hierarchyView.transform, state.ParentID);
            if (listParent != null)
                container = listParent.GetComponent<HierarchyItemController>().childrenContainer.transform;

            // Move the item
            var oldParent = listItem.parent.parent;
            listItem.SetParent(container);
            var offset = listParent == null ? 1 : 0;
            listItem.SetSiblingIndex(state.SiblingIndex + offset);

            // Check if the parent needs to be collapsed
            var oldParentItem = oldParent.GetComponent<HierarchyItemController>();
            if (oldParentItem != null) oldParentItem.ExpandItem(true);

            // Expand the parent and indent the item
            var indentionDepth = 0f;
            if (listParent != null)
            {
                var parentItem = listParent.GetComponent<HierarchyItemController>();
                parentItem.ExpandItem(true);
                parentItem.childrenContainer.SetActive(true);
                indentionDepth = parentItem.GetIndention() + 16f;
            }

            listItem.GetComponent<HierarchyItemController>().IndentItem(indentionDepth);

            // Move the item in the actual object
            var modelItem = Utility.FindChild(_model.transform, state.ID);
            var modelParent = Utility.FindChild(_model.transform, state.ParentID);
            if (modelParent == null) modelParent = _model.transform;
            modelItem.SetParent(modelParent);
            modelItem.SetSiblingIndex(state.SiblingIndex);
        }

        /// <summary>
        ///     Rename an item
        /// </summary>
        /// <param name="state">The item state</param>
        private static void Rename(ItemState state)
        {
            // Get the item and change the name on it
            var listItem = Utility.FindChild(_hierarchyView.transform, state.ID);
            var nameLabel = listItem.GetComponent<HierarchyItemController>().nameText;
            nameLabel.text = state.Name;
            
            // Change the display name in the actual object
            var modelItem = Utility.FindChild(_model.transform, state.ID);
            var itemInfo = modelItem.GetComponent<ItemInfoController>().ItemInfo;
            itemInfo.displayName = state.Name;
        }
    }
}