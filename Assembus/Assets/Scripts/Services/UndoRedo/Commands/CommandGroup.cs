using System.Collections.Generic;

namespace Services.UndoRedo.Commands
{
    public class CommandGroup : Command
    {
        /// <summary>
        ///     List containing all commands in order of execution
        /// </summary>
        private readonly List<Command> _commandList = new List<Command>();

        /// <summary>
        ///     Add a new Command to the List
        /// </summary>
        /// <param name="command">The command to add to the list</param>
        public void AddToGroup(Command command)
        {
            _commandList.Add(command);
        }

        /// <summary>
        ///     Call Undo action from list
        /// </summary>
        public override void Undo()
        {
            for (var i = _commandList.Count - 1; i >= 0; i--) _commandList[i].Undo();
        }

        /// <summary>
        ///     Call redo action from list
        /// </summary>
        public override void Redo()
        {
            foreach (var cmd in _commandList) cmd.Redo();
        }
    }
}