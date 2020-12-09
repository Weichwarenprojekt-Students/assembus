using CinemaScreen.Models;
using MainScreen;
using Services.Serialization;
using Shared;
using UnityEngine;

namespace CinemaScreen
{
    public class CinemaController : MonoBehaviour
    {
        /// <summary>
        ///     References to main and cinema screen
        /// </summary>
        public GameObject mainScreen, cinemaScreen;

        /// <summary>
        ///     References to the cinema control buttons
        /// </summary>
        public SwitchableButton playButton, pauseButton, previousButton, nextButton, skipStart, skipEnd;

        /// <summary>
        ///     The animation controller
        /// </summary>
        public AnimationController animationController;

        /// <summary>
        ///     Reference to the component highlighting which must be disabled
        /// </summary>
        public ComponentHighlighting componentHighlighting;

        /// <summary>
        ///     Private field for the cinema state machine
        /// </summary>
        private CinemaStateMachine _cinemaStateMachine;

        /// <summary>
        ///     Property for the cinema state machine
        /// </summary>
        private CinemaStateMachine CinemaStateMachine
        {
            get => _cinemaStateMachine;
            set
            {
                _cinemaStateMachine = value;

                // When a new cinema state machine is set, subscribe to its events
                SubscribeToStateMachine();
            }
        }

        /// <summary>
        ///     Initialize the cinema view
        /// </summary>
        private void OnEnable()
        {
            // Disable component highlighting
            componentHighlighting.isActive = false;

            // Hide all components
            Utility.ApplyRecursively(
                ProjectManager.Instance.CurrentProject.ObjectModel,
                o => o.SetActive(false),
                false
            );

            // Initialize new state machine
            CinemaStateMachine = new CinemaStateMachine();

            // Initialize the animation control buttons correctly  
            skipStart.Enable(false);
            previousButton.Enable(false);

            pauseButton.gameObject.SetActive(false);
            playButton.gameObject.SetActive(true);

            skipEnd.Enable(true);
            nextButton.Enable(true);

            // Pass the cinema state machine to the animation controller
            animationController.CinemaStateMachine = CinemaStateMachine;
        }

        /// <summary>
        ///     Subscribe to the events of the cinema state machine
        /// </summary>
        private void SubscribeToStateMachine()
        {
            // Local function for setting the buttons correctly
            void ExitPlayingState()
            {
                pauseButton.gameObject.SetActive(false);
                playButton.gameObject.SetActive(true);

                skipStart.Enable(true);
                skipEnd.Enable(true);
                nextButton.Enable(true);
                previousButton.Enable(true);
            }

            // Local function for setting the buttons correctly
            void EnterPlayingState()
            {
                playButton.gameObject.SetActive(false);
                pauseButton.gameObject.SetActive(true);

                skipStart.Enable(false);
                skipEnd.Enable(false);
                nextButton.Enable(false);
                previousButton.Enable(false);
            }

            CinemaStateMachine.PlayingFw.Entry += EnterPlayingState;
            CinemaStateMachine.PlayingFw.Exit += ExitPlayingState;

            CinemaStateMachine.PlayingBw.Entry += EnterPlayingState;
            CinemaStateMachine.PlayingBw.Exit += ExitPlayingState;

            CinemaStateMachine.StoppedEnd.Entry += () =>
            {
                skipEnd.Enable(false);
                nextButton.Enable(false);
            };
            CinemaStateMachine.StoppedEnd.Exit += () =>
            {
                skipEnd.Enable(true);
                nextButton.Enable(true);
            };

            CinemaStateMachine.StoppedStart.Entry += () =>
            {
                skipStart.Enable(false);
                previousButton.Enable(false);
            };
            CinemaStateMachine.StoppedStart.Exit += () =>
            {
                skipStart.Enable(true);
                previousButton.Enable(true);
            };
        }

        /// <summary>
        ///     Action for the next button
        /// </summary>
        public void NextButton()
        {
            CinemaStateMachine.SkipFw();
        }

        /// <summary>
        ///     Action for the previous button
        /// </summary>
        public void PreviousButton()
        {
            CinemaStateMachine.SkipBw();
        }

        /// <summary>
        ///     Action for the play button
        /// </summary>
        public void PlayButton()
        {
            animationController.Play();
        }

        /// <summary>
        ///     Action for the pause button
        /// </summary>
        public void PauseButton()
        {
            animationController.Pause();
        }

        /// <summary>
        ///     Action for the skip to end button
        /// </summary>
        public void SkipToEndButton()
        {
            CinemaStateMachine.SkipToEnd();
        }

        /// <summary>
        ///     Action for the skip to start button
        /// </summary>
        public void SkipToStartButton()
        {
            CinemaStateMachine.SkipToStart();
        }

        /// <summary>
        ///     Exit the cinema mode
        /// </summary>
        public void CloseCinemaMode()
        {
            // Activate component highlighting again
            componentHighlighting.isActive = true;

            // Switch screens
            cinemaScreen.SetActive(false);
            mainScreen.SetActive(true);

            var objectModel = ProjectManager.Instance.CurrentProject.ObjectModel;

            // Show the model
            Utility.ApplyRecursively(
                objectModel,
                o => o.SetActive(true),
                false
            );
        }
    }
}