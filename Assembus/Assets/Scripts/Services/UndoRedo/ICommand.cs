using System;
using JetBrains.Annotations;

namespace Services.UndoRedo
{
    public interface ICommand
    {
        /// <summary>
        ///     Function to undo
        /// </summary>
        void CallUndo();

        /// <summary>
        ///     Function to redo
        /// </summary>
        void CallRedo();
    }
}