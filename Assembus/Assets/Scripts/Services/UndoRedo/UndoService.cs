using System;
using System.Collections.Generic;

namespace Services.UndoRedo
{
    public class UndoService
    {
        /// <summary>
        ///     The configuration manager
        /// </summary>
        private readonly ConfigurationManager _configManager = ConfigurationManager.Instance;

        /// <summary>
        ///     LinkedList contains the undo redo history
        /// </summary>
        private readonly LinkedList<Command> _linkedList = new LinkedList<Command>();

        /// <summary>
        ///     Current element of the linked list
        /// </summary>
        private LinkedListNode<Command> _current;

        /// <summary>
        ///     Singleton private constructor
        /// </summary>
        private UndoService()
        {
            Reset();
        }

        /// <summary>
        ///     The action to be executed when a new command is inserted
        /// </summary>
        public Action OnNewCommand { get; set; }

        /// <summary>
        ///     Singleton instance
        /// </summary>
        public static UndoService Instance { get; } = new UndoService();

        /// <summary>
        ///     Reset the undo redo service
        /// </summary>
        public void Reset()
        {
            _current = new LinkedListNode<Command>(Command.Empty);
            _linkedList.Clear();
            _linkedList.AddFirst(_current);
        }

        /// <summary>
        ///     Add a command to the history of commands
        /// </summary>
        /// <param name="command">The command to be added</param>
        public void AddCommand(Command command)
        {
            // Add element after current
            _linkedList.AddAfter(_current, command);

            // Update the current element
            _current = _current.Next;
            if (_current?.Next != null) _linkedList.Remove(_current.Next);

            // Remove first entry if greater than configured length
            if (_linkedList.Count > _configManager.Config.undoHistoryLimit) _linkedList.RemoveFirst();

            // Execute the new action
            _current?.Value.CallRedo();

            // Notify the UI that new action was added
            OnNewCommand?.Invoke();
        }

        /// <summary>
        ///     Checks whether a redo action is possible/available
        /// </summary>
        /// <returns>True if redo possible</returns>
        public bool HasRedo()
        {
            return _current.Next != null;
        }

        /// <summary>
        ///     Call redo action from list
        /// </summary>
        public void Redo()
        {
            // Exit if no redo command exists
            if (!HasRedo()) return;

            // Set current to next element of the linked list
            _current = _current.Next;

            // Call redo function of the command
            _current?.Value.CallRedo();
        }

        /// <summary>
        ///     Checks whether a undo action is possible/available
        /// </summary>
        /// <returns>True if undo possible</returns>
        public bool HasUndo()
        {
            return !_current.Value.IsEmpty;
        }

        /// <summary>
        ///     Call undo action from list
        /// </summary>
        public void Undo()
        {
            // Exit if no undo command available
            if (!HasUndo()) return;

            // Call Undo function of command
            _current.Value.CallUndo();

            // Move back in linked list
            if (_current.Previous == null)
            {
                var previous = new LinkedListNode<Command>(Command.Empty);
                _linkedList.AddFirst(previous);
            }

            _current = _current.Previous;
        }
    }
}