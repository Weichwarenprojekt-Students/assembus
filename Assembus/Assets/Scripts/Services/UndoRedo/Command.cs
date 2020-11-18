namespace Services.UndoRedo
{
    public class Command
    {
        /// <summary>
        ///     The available command types
        /// </summary>
        public const int Move = 1, Rename = 2, Delete = 3, Create = 4;

        /// <summary>
        ///     The command type
        /// </summary>
        private readonly int _commandType;

        /// <summary>
        ///     The new item states
        /// </summary>
        private readonly ItemState[] _newStates;

        /// <summary>
        ///     The old item states
        /// </summary>
        private readonly ItemState[] _oldStates;

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
        /// <param name="commandType">The type of command the user wants to execute</param>
        public Command(ItemState[] newStates, ItemState[] oldStates, int commandType)
        {
            _newStates = newStates;
            _oldStates = oldStates;
            _commandType = commandType;
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
            // Get the undo action
            var undo = ActionCreator.GetUndoAction(_commandType);
            // Iterate over the old states
            foreach (var oldState in _oldStates) undo(oldState);
        }

        /// <summary>
        ///     Call the redo action for every new item state
        /// </summary>
        public void CallRedo()
        {
            // Get the redo action
            var redo = ActionCreator.GetRedoAction(_commandType);
            // Iterate over the new states
            foreach (var newState in _newStates) redo(newState);
        }
    }
}