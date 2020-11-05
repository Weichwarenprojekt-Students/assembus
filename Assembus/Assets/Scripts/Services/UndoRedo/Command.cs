using System;

namespace Services.UndoRedo
{
    public class Command: ICommand
    {
        /// <summary>
        ///     Delegate to store undo function
        /// </summary>
        public Action Undo { get; set; }
        /// <summary>
        ///     Delegate to store redo function
        /// </summary>
        public Action Redo { get; set; }
        
        /// <summary>
        ///     Call the undo function
        /// </summary>
        public void CallUndo()
        {
            Undo();
        }

        /// <summary>
        ///     Call the redo function
        /// </summary>
        public void CallRedo()
        {
            Redo();
        }
    }
}