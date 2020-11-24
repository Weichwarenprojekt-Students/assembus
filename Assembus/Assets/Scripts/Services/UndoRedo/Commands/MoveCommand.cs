using System.Collections.Generic;
using MainScreen.Sidebar.HierarchyView;
using Services.UndoRedo.Models;
using UnityEngine;

namespace Services.UndoRedo.Commands
{
    public class MoveCommand : Command
    {
        /// <summary>
        ///     The item states
        /// </summary>
        private readonly List<ItemState> _oldStates, _newStates;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="oldStates">The old item states</param>
        /// <param name="newStates">The new item states</param>
        public MoveCommand(List<ItemState> oldStates, List<ItemState> newStates)
        {
            _oldStates = oldStates;
            _newStates = newStates;
        }

        /// <summary>
        ///     Undo the move
        /// </summary>
        public override void Undo()
        {
            foreach (var state in _oldStates) Move(state);
        }

        /// <summary>
        ///     Redo the move
        /// </summary>
        public override void Redo()
        {
            foreach (var state in _newStates) Move(state);
        }

        /// <summary>
        ///     Move an item
        /// </summary>
        /// <param name="state">The item state</param>
        public static void Move(ItemState state)
        {
            // Get the item and the parent
            var listItem = Utility.FindChild(HierarchyView.transform, state.ID);
            var container = HierarchyView.transform;
            var listParent = Utility.FindChild(HierarchyView.transform, state.ParentID);
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
            var modelItem = Utility.FindChild(Model.transform, state.ID);
            var modelParent = Utility.FindChild(Model.transform, state.ParentID);
            if (modelParent == null) modelParent = Model.transform;
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
    }
}