using System.Linq;

namespace CinemaScreen.Models
{
    public class CinemaState
    {
        /// <summary>
        ///     Event when entering the state
        /// </summary>
        public event StateMachineAction Entry;

        /// <summary>
        ///     Event when leaving the state
        /// </summary>
        public event StateMachineAction Exit;

        /// <summary>
        ///     Invoke entry event
        /// </summary>
        internal void OnEntry()
        {
            Entry?.Invoke();
        }

        /// <summary>
        ///     Invoke exit event
        /// </summary>
        internal void OnExit()
        {
            Exit?.Invoke();
        }

        /// <summary>
        ///     Check if the state is not in a list of other states
        /// </summary>
        /// <param name="states">The list of states as multiple parameters</param>
        /// <returns>boolean if state is not in the list</returns>
        internal bool NotIn(params CinemaState[] states)
        {
            return !states.Contains(this);
        }

        /// <summary>
        ///     Check if the state is  in a list of other states
        /// </summary>
        /// <param name="states">The list of states as multiple parameters</param>
        /// <returns>boolean if state is  in the list</returns>
        internal bool In(params CinemaState[] states)
        {
            return states.Contains(this);
        }
    }
}