using System.Linq;
using UnityEngine;

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
        ///     Check if the transition from this state to another state is invalid,
        ///     given a list of valid states
        /// </summary>
        /// <param name="states">The list of states as multiple parameters</param>
        /// <returns>True if state transition is valid</returns>
        internal bool InvalidTransition(params CinemaState[] states)
        {
            var notIn = !states.Contains(this); 
            if (notIn) Debug.LogWarning("WARNING! Invalid state transition! State is not switched!");
            return notIn;
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