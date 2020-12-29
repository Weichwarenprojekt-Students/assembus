using System.Collections.Generic;
using CinemaScreen.Models;
using MainScreen;
using Services.Serialization;
using Shared;
using Shared.Tooltip;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CinemaScreen
{
    public class CinemaController : MonoBehaviour
    {
        /// <summary>
        ///     The size of a gap between stations on the progress bar
        /// </summary>
        private const int GapSize = 10;

        /// <summary>
        ///     References to main and cinema screen
        /// </summary>
        public GameObject mainScreen, cinemaScreen;

        /// <summary>
        ///     References to the cinema control buttons
        /// </summary>
        public SwitchableButton playButton, pauseButton, previousButton, nextButton, skipStart, skipEnd;

        /// <summary>
        ///     The canvas of the cinema screen
        /// </summary>
        public Canvas canvas;

        /// <summary>
        ///     The animation controller
        /// </summary>
        public AnimationController animationController;

        /// <summary>
        ///     Reference to the component highlighting which must be disabled
        /// </summary>
        public ComponentHighlighting componentHighlighting;

        /// <summary>
        ///     The progress bar and the foreground and background objects
        /// </summary>
        public RectTransform progressBar, progressBarBack, progressBarFront, progressBarDot;

        /// <summary>
        ///     The title of the station view (shows the current station)
        /// </summary>
        public TextMeshProUGUI title;

        /// <summary>
        ///     The tooltip controller
        /// </summary>
        public TooltipController tooltip;

        /// <summary>
        ///     The rectangles that are placed on the progress bar for each station
        /// </summary>
        private List<RectTransform> _backRects, _frontRects;

        /// <summary>
        ///     Private field for the cinema state machine
        /// </summary>
        private CinemaStateMachine _cinemaStateMachine;

        /// <summary>
        ///     The total amount of components
        /// </summary>
        private int _componentCount;

        /// <summary>
        ///     True if the mouse is in the progress bar
        /// </summary>
        private bool _mouseInProgressBar;

        /// <summary>
        ///     The information about the stations
        /// </summary>
        private List<Station> _stations;

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
        ///     Update the progress bar
        /// </summary>
        private void Update()
        {
            UpdateProgressBar();
            ShowTooltip();
        }

        /// <summary>
        ///     Initialize the cinema view
        /// </summary>
        private void OnEnable()
        {
            // Disable component highlighting
            componentHighlighting.isActive = false;

            var objectModel = ProjectManager.Instance.CurrentProject.ObjectModel;

            // Hide all components
            Utility.ApplyRecursively(
                objectModel,
                o => o.SetActive(false),
                false
            );
            AnimationController.SetOpacity(objectModel, 0);

            // Initialize new state machine
            CinemaStateMachine = new CinemaStateMachine();

            // Initialize the animation control buttons correctly  
            skipStart.Enable(false);
            previousButton.Enable(false);

            pauseButton.gameObject.SetActive(false);
            playButton.gameObject.SetActive(true);

            skipEnd.Enable(true);
            nextButton.Enable(true);

            // Initialize the animation controller
            (_componentCount, _stations) = animationController.Initialize();
            animationController.CinemaStateMachine = CinemaStateMachine;
            SetUpProgressBar();
        }

        /// <summary>
        ///     Update the progress bar by respecting the progress
        ///     of the animation controller
        /// </summary>
        private void UpdateProgressBar()
        {
            var scale = canvas.transform.localScale.y;
            var width = Screen.width / scale - (_stations.Count - 1) * GapSize;
            var progress = animationController.Index;
            for (var i = 0; i < _stations.Count; i++)
            {
                // Position the rectangles
                _frontRects[i].anchoredPosition = _backRects[i].anchoredPosition = new Vector2(
                    (float) _stations[i].PreviousItems / _componentCount * width + i * GapSize,
                    0
                );

                // Adjust the size of the backgrounds
                var fullWidth = (float) _stations[i].ChildCount / _componentCount * width;
                _backRects[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fullWidth);

                // Adjust the size of the foregrounds and update the station name
                if (_stations[i].PreviousItems + _stations[i].ChildCount < progress)
                {
                    _frontRects[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fullWidth);
                    if (i != _stations.Count - 1) continue;
                    progressBarDot.anchoredPosition = new Vector2(
                        _frontRects[i].anchoredPosition.x + _frontRects[i].sizeDelta.x,
                        0
                    );
                    title.text = _stations[i].Name;
                }
                else if (_stations[i].PreviousItems < progress)
                {
                    _frontRects[i].SetSizeWithCurrentAnchors(
                        RectTransform.Axis.Horizontal,
                        (progress - _stations[i].PreviousItems) / _componentCount * width
                    );
                    progressBarDot.anchoredPosition = new Vector2(
                        _frontRects[i].anchoredPosition.x + _frontRects[i].sizeDelta.x,
                        0
                    );
                    title.text = _stations[i].Name;
                }
                else
                {
                    _frontRects[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
                    if (i != 0) continue;
                    progressBarDot.anchoredPosition = new Vector2(0, 0);
                    title.text = _stations[i].Name;
                }
            }
        }

        /// <summary>
        ///     Show the tooltip for the progress bar
        /// </summary>
        private void ShowTooltip()
        {
            var scale = canvas.transform.localScale.y;
            if (!_mouseInProgressBar) return;
            float start = 0, position = Input.mousePosition.x;
            for (var i = 0; i < _stations.Count; i++)
            {
                var end = (_backRects[i].anchoredPosition.x + _backRects[i].sizeDelta.x) * scale;
                if (position > start && position < end)
                    tooltip.ShowTooltip(
                        position,
                        progressBar.position.y + 55 * scale,
                        _stations[i].Name + " \u2022 " + _stations[i].ChildCount + " Items",
                        true
                    );
                start = end;
            }
        }

        /// <summary>
        ///     Set up the progress bar
        /// </summary>
        private void SetUpProgressBar()
        {
            // Destroy the previous children
            _frontRects = new List<RectTransform>();
            _backRects = new List<RectTransform>();
            for (var i = 3; i < progressBar.transform.childCount; i++)
                Destroy(progressBar.transform.GetChild(i).gameObject);

            // Add the new background
            for (var i = 0; i < _stations.Count; i++)
            {
                _backRects.Add(
                    Instantiate(
                        progressBarBack,
                        progressBar.transform,
                        true
                    )
                );
                _frontRects.Add(
                    Instantiate(
                        progressBarFront,
                        progressBar.transform,
                        true
                    )
                );
            }
        }


        /// <summary>
        ///     React to clicking or dragging on the progress bar
        /// </summary>
        /// <param name="data">The event data</param>
        public void OnProgressBarDrag(BaseEventData data)
        {
            var pointerData = (PointerEventData) data;

            // Calculate the right index
            var index = 0;
            if (pointerData.position.x > Screen.width)
            {
                index = _componentCount;
            }
            else if (pointerData.position.x > 0)
            {
                float start = 0, position = pointerData.position.x / canvas.transform.localScale.x;
                for (var i = 0; i < _stations.Count; i++)
                {
                    var end = _backRects[i].anchoredPosition.x + _backRects[i].sizeDelta.x;
                    if (position > start && position < end)
                    {
                        var relativePosition = position - _backRects[i].anchoredPosition.x;
                        index = _stations[i].PreviousItems;
                        index += (int) (relativePosition / _backRects[i].sizeDelta.x * _stations[i].ChildCount);
                    }

                    start = end;
                }
            }

            animationController.SkipTo(index);
        }

        /// <summary>
        ///     React to hovering over the progress bar
        /// </summary>
        /// <param name="data">The event data</param>
        public void OnProgressBarHover(BaseEventData data)
        {
            _mouseInProgressBar = true;
        }

        /// <summary>
        ///     Hide the tooltip on mouse exit
        /// </summary>
        /// <param name="data"></param>
        public void OnProgressBarExit(BaseEventData data)
        {
            _mouseInProgressBar = false;
            tooltip.HideTooltip();
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
            }

            // Local function for setting the buttons correctly
            void EnterPlayingState()
            {
                playButton.gameObject.SetActive(false);
                pauseButton.gameObject.SetActive(true);
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
            if (CinemaStateMachine.CurrentState.In(CinemaStateMachine.PlayingFw, CinemaStateMachine.PlayingBw))
                CinemaStateMachine.SkipFwWhilePlaying();
            else
                CinemaStateMachine.SkipFw();
        }

        /// <summary>
        ///     Action for the previous button
        /// </summary>
        public void PreviousButton()
        {
            if (CinemaStateMachine.CurrentState.In(CinemaStateMachine.PlayingFw, CinemaStateMachine.PlayingBw))
                CinemaStateMachine.SkipBwWhilePlaying();
            else
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
            if (CinemaStateMachine.CurrentState.In(CinemaStateMachine.PlayingFw, CinemaStateMachine.PlayingBw))
                CinemaStateMachine.SkipToEndWhilePlaying();
            else
                CinemaStateMachine.SkipToEnd();
        }

        /// <summary>
        ///     Action for the skip to start button
        /// </summary>
        public void SkipToStartButton()
        {
            if (CinemaStateMachine.CurrentState.In(CinemaStateMachine.PlayingFw, CinemaStateMachine.PlayingBw))
                CinemaStateMachine.SkipToStartWhilePlaying();
            else
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
            AnimationController.SetOpacity(objectModel, 1);
            Utility.ApplyRecursively(
                objectModel,
                o => o.SetActive(true),
                false
            );
        }
    }
}