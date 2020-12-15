using System;
using System.Collections;
using System.Collections.Generic;
using CinemaScreen.Models;
using Models.Project;
using Services.Serialization;
using Shared;
using UnityEngine;
using UnityEngine.UI;

namespace CinemaScreen
{
    public class AnimationController : MonoBehaviour
    {
        /// <summary>
        ///     Reference to the playback speed slider
        /// </summary>
        public Slider speedSlider;

        /// <summary>
        ///     Private reference to the state machine for the cinema states
        /// </summary>
        private CinemaStateMachine _cinemaStateMachine;

        /// <summary>
        ///     Currently running animation coroutine
        /// </summary>
        private Coroutine _currentCoroutine;

        /// <summary>
        ///     Current index in the playback list (-1 is the beginning _playbackList.Count - 1 is the end)
        /// </summary>
        private int _index;

        /// <summary>
        ///     List that contains all animation items
        /// </summary>
        private List<GameObject> _playbackList;

        /// <summary>
        ///     True if the current playback animation should be skipped
        /// </summary>
        private bool _shouldSkipFw, _shouldSkipBw;

        /// <summary>
        ///     The state machine which controls the cinema states
        /// </summary>
        public CinemaStateMachine CinemaStateMachine
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
        ///     The current state of the cinema mode state machine
        /// </summary>
        private CinemaState CurrentState => _cinemaStateMachine.CurrentState;

        /// <summary>
        ///     True if the playback coroutines should be stopped
        /// </summary>
        private bool ShouldPause { get; set; }

        /// <summary>
        ///     Initialize the animation controller on enabling it
        /// </summary>
        private void OnEnable()
        {
            // Set playback speed to 1
            speedSlider.value = 1;

            // Set index of the playback list to the beginning
            _index = -1;

            // Initialize the empty playback list
            _playbackList = new List<GameObject>();

            // Fill the playback list with the objects from the 3d model
            FillList(ProjectManager.Instance.CurrentProject.ObjectModel);

            // Reset should pause to false, necessary if cinema mode was exited while playing was acitve
            ShouldPause = false;
        }

        /// <summary>
        ///     Stop the playback when the cinema mode is exited
        /// </summary>
        private void OnDisable()
        {
            if (CurrentState.In(CinemaStateMachine.PlayingFw, CinemaStateMachine.PlayingBw))
                Pause();
        }

        /// <summary>
        ///     Subscribe to the needed events from the cinema state machine
        /// </summary>
        private void SubscribeToStateMachine()
        {
            // Subscribe to Playing Entry events
            CinemaStateMachine.PlayingFw.Entry += () => { _currentCoroutine = StartCoroutine(PlayAnimationForward()); };
            CinemaStateMachine.PlayingBw.Entry += () =>
            {
                _currentCoroutine = StartCoroutine(PlayAnimationBackward());
            };

            // Subscribe to Skipping Entry events
            CinemaStateMachine.SkippingFw.Entry += () =>
            {
                _currentCoroutine = StartCoroutine(PlayAnimationForward());
            };
            CinemaStateMachine.SkippingBw.Entry += () =>
            {
                _currentCoroutine = StartCoroutine(PlayAnimationBackward());
            };

            // Reset the current coroutine
            CinemaStateMachine.Paused.Entry += () => { _currentCoroutine = null; };
            CinemaStateMachine.StoppedEnd.Entry += () => { _currentCoroutine = null; };
            CinemaStateMachine.StoppedStart.Entry += () => { _currentCoroutine = null; };

            // Enable skipping while playing
            CinemaStateMachine.SkippedFwWhilePlaying += () => { _shouldSkipFw = true; };
            CinemaStateMachine.SkippedBwWhilePlaying += () => { _shouldSkipBw = true; };

            // Enable skipping to start/end while playing
            CinemaStateMachine.SkippedToEndWhilePlaying += () =>
            {
                StopCoroutine(_currentCoroutine);
                SkipToEnd();
            };
            CinemaStateMachine.SkippedToStartWhilePlaying += () =>
            {
                StopCoroutine(_currentCoroutine);
                SkipToStart();
            };

            CinemaStateMachine.SkippedToEnd += SkipToEnd;
            CinemaStateMachine.SkippedToStart += SkipToStart;
        }

        /// <summary>
        ///     Fill list with components and fused groups
        /// </summary>
        /// <param name="parent">GameObject parent</param>
        private void FillList(GameObject parent)
        {
            // For all children in parent
            for (var i = 0; i < parent.transform.childCount; i++)
            {
                var go = parent.transform.GetChild(i).gameObject;

                var itemInfo = go.GetComponent<ItemInfoController>().ItemInfo;

                // Components (leaves in the tree) or fused group get added to the list because fused groups should
                //     be handled like components by the animation
                if (itemInfo.isGroup)
                    if (itemInfo.isFused) _playbackList.Add(go);
                    else FillList(go);
                else _playbackList.Add(go);
            }
        }

        /// <summary>
        ///     Skip to the start of the animation
        /// </summary>
        private void SkipToStart()
        {
            // Set index to the beginning of the list
            _index = -1;

            var objectModel = ProjectManager.Instance.CurrentProject.ObjectModel;

            // Hide all items recursively (except groups)
            Utility.ApplyRecursively(
                objectModel,
                o => o.SetActive(false),
                false
            );

            // Set opacity of all components to fully transparent
            SetOpacity(objectModel, 0);
        }

        /// <summary>
        ///     Skip to the end of the animation
        /// </summary>
        private void SkipToEnd()
        {
            // Set index to the end of the list
            _index = _playbackList.Count - 1;

            var objectModel = ProjectManager.Instance.CurrentProject.ObjectModel;

            // Show all items recursively (except groups)
            Utility.ApplyRecursively(
                objectModel,
                o => o.SetActive(true),
                false
            );

            // Set opacity of all components to fully opaque
            SetOpacity(objectModel, 1);
        }

        /// <summary>
        ///     Set the state machine into playback mode
        /// </summary>
        public void Play()
        {
            if (speedSlider.value >= 0) CinemaStateMachine.PlayFw();
            else CinemaStateMachine.PlayBw();
        }

        /// <summary>
        ///     Tell the animation to pause
        /// </summary>
        public void Pause()
        {
            ShouldPause = true;
        }

        /// <summary>
        ///     Play animation forward
        /// </summary>
        /// <returns>IEnumerator for the coroutine</returns>
        private IEnumerator PlayAnimationForward()
        {
            // Increase index to be on the item which should be faded in
            _index++;

            // Fade in the current item
            yield return PlayFadeAnimation(true);

            if (_index >= _playbackList.Count - 1)
            {
                // The animation reached the end
                CinemaStateMachine.ReachEnd();
            }
            else if (CurrentState == CinemaStateMachine.SkippingFw)
            {
                // Skipping ends
                CinemaStateMachine.EndSkipping();
            }
            else if (CurrentState == CinemaStateMachine.PlayingFw)
            {
                // Wait for delay after fading
                yield return WaitAccordingToSpeedSlider();

                // Skip in the opposite direction
                if (_shouldSkipBw) SkipWhilePlaying();

                // Reset skip flag
                _shouldSkipFw = false;

                if (ShouldPause)
                {
                    ShouldPause = false;

                    // Set the state machine to Paused
                    CinemaStateMachine.Pause();
                }
                else if (speedSlider.value >= 0)
                {
                    // Keep playing forward
                    _currentCoroutine = StartCoroutine(PlayAnimationForward());
                }
                else
                {
                    // Switch playback direction
                    CinemaStateMachine.SwitchPlayingDirection();
                }
            }
        }

        /// <summary>
        ///     Play animation Backward
        /// </summary>
        /// <returns>IEnumerator for the coroutine</returns>
        private IEnumerator PlayAnimationBackward()
        {
            // Fade out the current item
            yield return PlayFadeAnimation(false);

            // Decrease the index to be on the previous item
            _index--;

            if (_index <= -1)
            {
                // The animation reached the start
                CinemaStateMachine.ReachStart();
            }
            else if (CurrentState == CinemaStateMachine.SkippingBw)
            {
                // Skipping ends
                CinemaStateMachine.EndSkipping();
            }
            else if (CurrentState == CinemaStateMachine.PlayingBw)
            {
                // Wait for delay after fading
                yield return WaitAccordingToSpeedSlider();

                // Skip in the opposite direction
                if (_shouldSkipFw) SkipWhilePlaying();

                // Reset skip flag
                _shouldSkipBw = false;

                if (ShouldPause)
                {
                    ShouldPause = false;

                    // Set the state machine to Paused
                    CinemaStateMachine.Pause();
                }
                else if (speedSlider.value >= 0)
                {
                    // Switch playback direction
                    CinemaStateMachine.SwitchPlayingDirection();
                }
                else
                {
                    // Keep playing backward
                    _currentCoroutine = StartCoroutine(PlayAnimationBackward());
                }
            }
        }

        /// <summary>
        ///     Skip in the opposite direction of playback
        /// </summary>
        private void SkipWhilePlaying()
        {
            if (_shouldSkipFw)
                // repeat twice
                for (var i = 0; i < 2; i++)
                {
                    // select next item
                    _index++;
                    var currentObject = _playbackList[_index];

                    // Show item
                    SetOpacity(currentObject, 1);
                    Utility.ApplyRecursively(currentObject, obj => obj.SetActive(true), false);

                    // Don't go over the edge of the list
                    if (_index >= _playbackList.Count - 1) break;
                }
            else if (_shouldSkipBw)
                // repeat twice
                for (var i = 0; i < 2; i++)
                {
                    // Get previous item
                    var currentObject = _playbackList[_index];
                    _index--;

                    // Hide item
                    SetOpacity(currentObject, 0);
                    Utility.ApplyRecursively(currentObject, obj => obj.SetActive(false), false);

                    // Don't got over the edge of the list
                    if (_index <= -1) break;
                }

            // Reset skip flags
            _shouldSkipFw = false;
            _shouldSkipBw = false;
        }

        /// <summary>
        ///     Wait for a specific time according to the speed slider
        /// </summary>
        /// <returns>IEnumerator for the coroutine</returns>
        private IEnumerator WaitAccordingToSpeedSlider()
        {
            // Calculate delay until next fading, proportional to the speed slider value
            var waitTime = 1 / Math.Abs(speedSlider.value);

            var passedTime = 0.0;
            while (passedTime <= waitTime)
            {
                // If the animation should pause just skip
                if (ShouldPause || _shouldSkipFw || _shouldSkipBw) break;

                passedTime += Time.deltaTime;

                // Wait for the next frame
                yield return null;
            }
        }

        /// <summary>
        ///     Fade the current item in our out
        /// </summary>
        /// <param name="fadeIn">True if the component should be faded in, false if it should be faded out</param>
        /// <returns>IEnumerator for the coroutine</returns>
        private IEnumerator PlayFadeAnimation(bool fadeIn)
        {
            // Get the item to fade in
            var currentObject = _playbackList[_index];

            if (fadeIn)
            {
                // Set item to be fully transparent
                SetOpacity(currentObject, 0);

                // Set the item or its child components to be visible
                Utility.ApplyRecursively(currentObject, obj => obj.SetActive(true), false);
            }

            for (float opacity = 0; opacity <= 1; opacity += 3 * Time.deltaTime)
            {
                // Continuously increase the opacity of the item or its child components
                SetOpacity(currentObject, fadeIn ? opacity : 1 - opacity);

                // If the animation should pause, skip the fading
                if (ShouldPause || _shouldSkipFw || _shouldSkipBw) break;

                // Wait until the next frame
                yield return null;
            }

            if (!fadeIn)
            {
                // Set the item or its child components to be hidden
                SetOpacity(currentObject, 0);
                Utility.ApplyRecursively(currentObject, obj => obj.SetActive(false), false);
            }
            else
            {
                // Reset opacity so its always opaque
                SetOpacity(currentObject, 1);
            }
        }

        /// <summary>
        ///     Set opacity recursively for component and its children
        /// </summary>
        /// <param name="parent">The parent game object</param>
        /// <param name="opacity">The opacity</param>
        public static void SetOpacity(GameObject parent, float opacity)
        {
            Utility.ApplyRecursively(
                parent,
                obj =>
                {
                    // Get renderer and material from component
                    var objRenderer = obj.GetComponent<Renderer>();
                    var material = objRenderer.material;

                    // Set the alpha channel of the material to the opacity value
                    var c = material.color;
                    c.a = opacity;
                    material.color = c;
                },
                false
            );
        }
    }
}