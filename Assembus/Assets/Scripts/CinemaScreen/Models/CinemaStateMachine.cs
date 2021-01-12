namespace CinemaScreen.Models
{
    public delegate void StateMachineAction();

    public class CinemaStateMachine
    {
        /// <summary>
        ///     All states of the cinema state machine
        /// </summary>
        public readonly CinemaState Paused = new CinemaState(),
            PlayingFw = new CinemaState(),
            PlayingBw = new CinemaState(),
            SkippingFw = new CinemaState(),
            SkippingBw = new CinemaState(),
            StoppedEnd = new CinemaState(),
            StoppedStart = new CinemaState();

        /// <summary>
        ///     Private field for the current state of the state machine
        /// </summary>
        private CinemaState _currentState;

        /// <summary>
        ///     Constructor
        /// </summary>
        public CinemaStateMachine()
        {
            // Set the starting state
            _currentState = StoppedStart;
        }

        /// <summary>
        ///     Property for the current state
        /// </summary>
        public CinemaState CurrentState
        {
            get => _currentState;
            private set
            {
                // Invoke Exit and Entry events of the old and new state
                _currentState.OnExit();
                _currentState = value;
                _currentState.OnEntry();
            }
        }

        /// <summary>
        ///     Action to go into the PlayingFw state
        /// </summary>
        /// <param name="skip">True if the state change is caused by skip</param>
        public void PlayFw(bool skip = false)
        {
            // Don't change the state if not in an allowed state
            if (!skip && CurrentState.InvalidTransition(Paused, StoppedStart)) return;

            CurrentState = PlayingFw;
        }

        /// <summary>
        ///     Action to go into the PlayingBw state
        /// </summary>
        /// <param name="skip">True if the state change is caused by skip</param>
        public void PlayBw(bool skip = false)
        {
            // Don't change the state if not in an allowed state
            if (!skip && CurrentState.InvalidTransition(Paused, StoppedEnd)) return;

            CurrentState = PlayingBw;
        }

        /// <summary>
        ///     Action to go from the Playing into the Paused state
        /// </summary>
        /// <param name="skip">True if the state change is caused by skip</param>
        public void Pause(bool skip = false)
        {
            // Don't change the state if not in an allowed state
            if (!skip && CurrentState.InvalidTransition(PlayingFw, PlayingBw)) return;

            CurrentState = Paused;
        }

        /// <summary>
        ///     Action to go into the SkippingFw state
        /// </summary>
        public void SkipFw()
        {
            // Don't change the state if not in an allowed state
            if (CurrentState.InvalidTransition(Paused, StoppedStart)) return;

            CurrentState = SkippingFw;
        }

        /// <summary>
        ///     Action to go into the SkippingBw state
        /// </summary>
        public void SkipBw()
        {
            // Don't change the state if not in an allowed state
            if (CurrentState.InvalidTransition(Paused, StoppedEnd)) return;

            CurrentState = SkippingBw;
        }

        /// <summary>
        ///     Action to go from Skipping into the Paused state
        /// </summary>
        public void EndSkipping()
        {
            // Don't change the state if not in an allowed state
            if (CurrentState.InvalidTransition(SkippingFw, SkippingBw)) return;

            CurrentState = Paused;
        }

        /// <summary>
        ///     Action to go into the StoppedEnd state whenever the animation reaches
        ///     the end
        /// </summary>
        /// <param name="skip">True if the state change is caused by skip</param>
        public void ReachEnd(bool skip = false)
        {
            // Don't change the state if not in an allowed state
            if (!skip && CurrentState.InvalidTransition(PlayingFw, SkippingFw)) return;

            CurrentState = StoppedEnd;
        }

        /// <summary>
        ///     Action to go into the StoppedStart state whenever the animation reaches
        ///     the start
        /// </summary>
        /// <param name="skip">True if the state change is caused by skip</param>
        public void ReachStart(bool skip = false)
        {
            // Don't change the state if not in an allowed state
            if (!skip && CurrentState.InvalidTransition(PlayingBw, SkippingBw)) return;

            CurrentState = StoppedStart;
        }

        /// <summary>
        ///     Action switch the playing direction
        /// </summary>
        public void SwitchPlayingDirection()
        {
            // Don't change the state if not in an allowed state
            if (CurrentState.InvalidTransition(PlayingFw, PlayingBw)) return;

            CurrentState = CurrentState == PlayingFw ? PlayingBw : PlayingFw;
        }

        /// <summary>
        ///     Action to go into the StoppedStart state with skip to start
        /// </summary>
        public void SkipToStart()
        {
            // Don't change the state if not in an allowed state
            if (CurrentState.InvalidTransition(Paused, StoppedEnd)) return;

            SkippedToStart?.Invoke();
            CurrentState = StoppedStart;
        }

        /// <summary>
        ///     Event which gets invoked when skipping to start
        /// </summary>
        public event StateMachineAction SkippedToStart;

        /// <summary>
        ///     Action to go into the StoppedEnd state with skip to end
        /// </summary>
        public void SkipToEnd()
        {
            // Don't change the state if not in an allowed state
            if (CurrentState.InvalidTransition(Paused, StoppedStart)) return;

            SkippedToEnd?.Invoke();
            CurrentState = StoppedEnd;
        }

        /// <summary>
        ///     Event which gets invoked when skipping to end
        /// </summary>
        public event StateMachineAction SkippedToEnd;

        /// <summary>
        ///     Action to skip forward while playing
        /// </summary>
        public void SkipFwWhilePlaying()
        {
            if (CurrentState.InvalidTransition(PlayingFw, PlayingBw)) return;

            SkippedFwWhilePlaying?.Invoke();
        }

        /// <summary>
        ///     Event which gets invoked when skipping forward while playing
        /// </summary>
        public event StateMachineAction SkippedFwWhilePlaying;

        /// <summary>
        ///     Action to skip backward while playing
        /// </summary>
        public void SkipBwWhilePlaying()
        {
            if (CurrentState.InvalidTransition(PlayingFw, PlayingBw)) return;

            SkippedBwWhilePlaying?.Invoke();
        }

        /// <summary>
        ///     Event which gets invoked when skipping backward while playing
        /// </summary>
        public event StateMachineAction SkippedBwWhilePlaying;

        /// <summary>
        ///     Action to skip to end while playing
        /// </summary>
        public void SkipToEndWhilePlaying()
        {
            if (CurrentState.InvalidTransition(PlayingFw, PlayingBw)) return;

            SkippedToEndWhilePlaying?.Invoke();
            CurrentState = StoppedEnd;
        }

        /// <summary>
        ///     Event which gets invoked when skipping to end while playing
        /// </summary>
        public event StateMachineAction SkippedToEndWhilePlaying;

        /// <summary>
        ///     Action to skip to start while playing
        /// </summary>
        public void SkipToStartWhilePlaying()
        {
            if (CurrentState.InvalidTransition(PlayingFw, PlayingBw)) return;

            SkippedToStartWhilePlaying?.Invoke();
            CurrentState = StoppedStart;
        }

        /// <summary>
        ///     Event which gets invoked when skipping to start while playing
        /// </summary>
        public event StateMachineAction SkippedToStartWhilePlaying;
    }
}