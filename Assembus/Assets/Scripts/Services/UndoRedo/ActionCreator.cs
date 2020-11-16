using System;
using MainScreen;
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
        ///     A default hierarchy view item
        /// </summary>
        private static MainController _main;

        /// <summary>
        ///     The hierarchy view controller
        /// </summary>
        private static HierarchyViewController _hierarchyController;

        /// <summary>
        ///     Initialize the command executor
        /// </summary>
        /// <param name="model">The actual model</param>
        /// <param name="hierarchyView">The list view</param>
        /// <param name="main">The main controller</param>
        /// <param name="hierarchyController">The hierarchy view controller</param>
        public static void Initialize(GameObject model, GameObject hierarchyView, MainController main,
            HierarchyViewController hierarchyController)
        {
            _model = model;
            _hierarchyView = hierarchyView;
            _main = main;
            _hierarchyController = hierarchyController;
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

                case Command.Create:
                    return Delete;

                case Command.Delete:
                    return Create;

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

                case Command.Create:
                    return Create;

                case Command.Delete:
                    return Delete;

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

        /// <summary>
        ///     Create a new group item
        /// </summary>
        /// <param name="state">The item state</param>
        private static void Create(ItemState state)
        {
            // Create the object
            var isRootParent = state.ParentID == _model.name;
            var parent = isRootParent ? _model.transform : Utility.FindChild(_model.transform, state.ParentID);
            var newObject = new GameObject(state.ID);
            newObject.AddComponent<ItemInfoController>();
            newObject.GetComponent<ItemInfoController>().ItemInfo
                = new ItemInfo {displayName = state.Name, isGroup = true};
            newObject.transform.parent = parent;

            // Create the list view item
            var listParent =
                isRootParent ? _hierarchyView : Utility.FindChild(_hierarchyView.transform, state.ParentID).gameObject;
            var indention = isRootParent ? 0 : listParent.GetComponent<HierarchyItemController>().GetIndention();
            _main.AddSingleListItem(listParent, newObject, indention);
        }

        /// <summary>
        ///     Delete a new group item
        /// </summary>
        /// <param name="state">The item state</param>
        private static void Delete(ItemState state)
        {
            // Delete the actual object
            var item = Utility.FindChild(_hierarchyView.transform, state.ID);
            var hierarchyItem = item.GetComponent<HierarchyItemController>();
            _main.DeleteObject(hierarchyItem.item);
            
            // Delete the list view item and the selection
            _hierarchyController.SelectedItems.Remove(hierarchyItem);
            _main.DeleteObject(item.gameObject);
        }
    }
}