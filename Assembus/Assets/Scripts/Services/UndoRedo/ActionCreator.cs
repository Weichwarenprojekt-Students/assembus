using System;
using MainScreen.Sidebar.HierarchyView;
using Models.Project;
using Shared;
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
        ///     The hierarchy view controller
        /// </summary>
        private static HierarchyViewController _hierarchyController;

        /// <summary>
        ///     Initialize the command executor
        /// </summary>
        /// <param name="model">The actual model</param>
        /// <param name="hierarchyView">The list view</param>
        /// <param name="hierarchyController">The hierarchy view controller</param>
        public static void Initialize(GameObject model, GameObject hierarchyView,
            HierarchyViewController hierarchyController)
        {
            _model = model;
            _hierarchyView = hierarchyView;
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
                indentionDepth = parentItem.GetIndention() + 16f;
                parentItem.ExpandItem(true);
                parentItem.childrenContainer.SetActive(true);
            }

            IndentItems(listItem, indentionDepth);

            // Move the item in the actual object
            var modelItem = Utility.FindChild(_model.transform, state.ID);
            var modelParent = Utility.FindChild(_model.transform, state.ParentID);
            if (modelParent == null) modelParent = _model.transform;
            modelItem.SetParent(modelParent);
            modelItem.SetSiblingIndex(state.SiblingIndex);
        }

        /// <summary>
        ///     Recursively indent the items
        /// </summary>
        /// <param name="item">The item to be corrected</param>
        /// <param name="indentionDepth">The indention depth</param>
        private static void IndentItems(Transform item, float indentionDepth)
        {
            // Indent the children
            var listItem = item.GetComponent<HierarchyItemController>();
            var childrenContainer = listItem.childrenContainer.transform;
            for (var i = 0; i < childrenContainer.childCount; i++)
                IndentItems(childrenContainer.GetChild(i), indentionDepth + HierarchyViewController.Indention);

            // Indent the item
            listItem.IndentItem(indentionDepth);
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
            var newObject = new GameObject(state.ID);
            newObject.AddComponent<ItemInfoController>();
            newObject.GetComponent<ItemInfoController>().ItemInfo
                = new ItemInfo {displayName = state.Name, isGroup = true};
            newObject.transform.parent = _model.transform;
            newObject.transform.SetSiblingIndex(state.SiblingIndex);

            // Create the list view item
            _hierarchyController.AddSingleListItem(_hierarchyView, newObject, 0);

            // Move the item
            Move(state);
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
            _hierarchyController.DeleteObject(hierarchyItem.item);

            // Delete the list view item and the selection
            _hierarchyController.SelectedItems.Remove(hierarchyItem);
            _hierarchyController.DeleteObject(item.gameObject);
        }
    }
}