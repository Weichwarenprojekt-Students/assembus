using System.Collections.Generic;
using UnityEngine;

namespace Services.UndoRedo.Commands
{
    public class CommandGroup : Command

    {
        private readonly List<Command> _commandList = new List<Command>();

        /// <summary>
        ///     Add a new Command to the List
        /// </summary>
        /// <param name="command"></param>
        public void AddToGroup(Command command)
        {
            _commandList.Add(command);
        }
        
        /// <summary>
        ///     Call Undo action from list
        /// </summary>
        public override void Undo()
        {
            for (var i = _commandList.Count-1; i >= 0; i--) _commandList[i].Undo();
        }

        /// <summary>
        ///     Call redo action from list
        /// </summary>
        public override void Redo()
        {
            foreach (var t in _commandList) t.Redo();
        }
    }
}