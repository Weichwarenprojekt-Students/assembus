using System;

namespace Services.UndoRedo
{
    public class Command
    {
        /// <summary>
        ///     The new item states
        /// </summary>
        private readonly ItemState[] _newStates;

        /// <summary>
        ///     The old item states
        /// </summary>
        private readonly ItemState[] _oldStates;

        /// <summary>
        ///     The redo action
        /// </summary>
        private readonly Action<ItemState> _redo;

        /// <summary>
        ///     The undo action
        /// </summary>
        private readonly Action<ItemState> _undo;

        /// <summary>
        ///     True if the command is empty
        /// </summary>
        public readonly bool IsEmpty;

        /// <summary>
        ///     Private constructor for empty commands
        /// </summary>
        private Command()
        {
            IsEmpty = true;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="newStates">The new item states</param>
        /// <param name="oldStates">The old item states</param>
        /// <param name="redo">The redo action</param>
        /// <param name="undo">The undo action</param>
        public Command(ItemState[] newStates, ItemState[] oldStates, Action<ItemState> redo, Action<ItemState> undo)
        {
            _newStates = newStates;
            _oldStates = oldStates;
            _redo = redo;
            _undo = undo;
            IsEmpty = false;
        }


        /// <summary>
        ///     An empty command instance
        /// </summary>
        public static Command Empty { get; } = new Command();

        /// <summary>
        ///     Call the undo action for every old item state
        /// </summary>
        public void CallUndo()
        {
            foreach (var oldState in _oldStates) _undo(oldState);
        }

        /// <summary>
        ///     Call the redo action for every new item state
        /// </summary>
        public void CallRedo()
        {
            foreach (var newState in _newStates) _redo(newState);
        }
    }
}