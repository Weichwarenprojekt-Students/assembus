using System;
using System.Collections.Generic;
using Services.UndoRedo.Commands;

namespace Services.UndoRedo
{
    public class UndoService
    {
        /// <summary>
        ///     Maximum number of actions in the history
        /// </summary>
        private const int UndoHistoryLimit = 420;

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
        ///     The action to be executed when a command is executed
        /// </summary>
        public Action<Command> OnCommandExecution { get; set; }

        /// <summary>
        ///     Singleton instance
        /// </summary>
        public static UndoService Instance { get; } = new UndoService();

        /// <summary>
        ///     Reset the undo redo service
        /// </summary>
        public void Reset()
        {
            _current = new LinkedListNode<Command>(null);
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

            // Update the current element and delete all the commands that were undone
            _current = _current.Next;
            while (_current?.Next != null) _linkedList.Remove(_current.Next);

            // Remove first entry if greater than configured length
            if (_linkedList.Count > UndoHistoryLimit) _linkedList.RemoveFirst();

            // Execute the new action
            _current?.Value.Redo();

            // Notify the UI that new action was added
            if (_current != null) OnCommandExecution(_current.Value);
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
            _current?.Value.Redo();

            // Notify the UI that new action was added
            if (_current != null) OnCommandExecution(_current.Value);
        }

        /// <summary>
        ///     Checks whether a undo action is possible/available
        /// </summary>
        /// <returns>True if undo possible</returns>
        public bool HasUndo()
        {
            return _current.Value != null;
        }

        /// <summary>
        ///     Call undo action from list
        /// </summary>
        public void Undo()
        {
            // Exit if no undo command available
            if (!HasUndo()) return;

            // Call Undo function of command
            _current.Value.Undo();

            // Move back in linked list
            if (_current.Previous == null)
            {
                var previous = new LinkedListNode<Command>(null);
                _linkedList.AddFirst(previous);
            }

            _current = _current.Previous;

            // Notify the UI that new action was added
            if (_current != null) OnCommandExecution(_current.Next?.Value);
        }
    }
}