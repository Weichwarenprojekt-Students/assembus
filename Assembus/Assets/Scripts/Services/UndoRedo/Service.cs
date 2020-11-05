using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Services.UndoRedo
{
    public class UndoService
    {
        /// <summary>
        ///      Singleton private constructor 
        /// </summary>
        private UndoService() {}
        
        /// <summary>
        ///     Singleton instance
        /// </summary>
        public static UndoService Instance { get; } = new UndoService();

        /// <summary>
        ///     LinkedList contains the undo redo history
        /// </summary>
        private readonly LinkedList<ICommand> _linkedList = new LinkedList<ICommand>();
        
        /// <summary>
        ///     Current element of the linked list
        /// </summary>
        private LinkedListNode<ICommand> _current = null;

        /// <summary>
        ///     Add a command to the history of commands
        /// </summary>
        /// <param name="command"></param>
        public void AddCommand(ICommand command)
        {
            // Check if append after is available
            if (_linkedList.Count == 0)
            {
                // Insert element as the first one and set current to it
                _linkedList.AddFirst(command);
                _current = _linkedList.First;
            }
            else
            {
                // Add element after current
                _linkedList.AddAfter(_current, command);       
            }
        }

        /// <summary>
        ///     Checks whether a redo action is possible/avaiable
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

            // Call redo function of the command
            _current.Value.CallRedo();
            
            // Set current to next element of the linked list
            _current = _current.Next;
        }

        /// <summary>
        ///      Checks whether a undo action is possible/available
        /// </summary>
        /// <returns></returns>
        public bool HasUndo()
        {
            return _current != null;
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
            _current = _current.Previous;
        }
        
    }
}