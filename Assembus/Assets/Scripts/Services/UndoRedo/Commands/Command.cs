using MainScreen.Sidebar.HierarchyView;
using UnityEngine;

namespace Services.UndoRedo.Commands
{
    public abstract class Command
    {
        /// <summary>
        ///     The actual model
        /// </summary>
        protected static GameObject Model;

        /// <summary>
        ///     The list view
        /// </summary>
        protected static GameObject HierarchyView;

        /// <summary>
        ///     The hierarchy view controller
        /// </summary>
        protected static HierarchyViewController HierarchyController;

        /// <summary>
        ///     Initialize the command executor
        /// </summary>
        /// <param name="model">The actual model</param>
        /// <param name="hierarchyView">The list view</param>
        /// <param name="hierarchyController">The hierarchy view controller</param>
        public static void Initialize(GameObject model, GameObject hierarchyView,
            HierarchyViewController hierarchyController)
        {
            Model = model;
            HierarchyView = hierarchyView;
            HierarchyController = hierarchyController;
        }

        /// <summary>
        ///     The undo action
        /// </summary>
        public abstract void Undo();

        /// <summary>
        ///     The redo action
        /// </summary>
        public abstract void Redo();
    }
}