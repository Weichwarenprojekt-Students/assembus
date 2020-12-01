﻿using System.Collections;
using System.Collections.Generic;
using MainScreen;
using Models.Project;
using Services.Serialization;
using Shared;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace CinemaScreen
{
    public class CinemaController : MonoBehaviour
    {
        /// <summary>
        ///     References to main and cinema screen
        /// </summary>
        public GameObject mainScreen, cinemaScreen;

        /// <summary>
        ///     References to play and pause buttons
        /// </summary>
        public GameObject playButton, pauseButton;

        /// <summary>
        ///     Reference to the playback speed slider
        /// </summary>
        public Slider speedSlider;

        /// <summary>
        ///     Reference to the component highlighting which must be disabled
        /// </summary>
        public ComponentHighlighting componentHighlighting;

        /// <summary>
        ///     Bool if an animation is currently running
        /// </summary>
        private bool _animationRunning;

        /// <summary>
        ///     Current index in the playback list
        /// </summary>
        private int _index;

        /// <summary>
        ///     List that contains all animation items
        /// </summary>
        private List<GameObject> _playbackList;

        /// <summary>
        ///     Initialize the cinema view
        /// </summary>
        private void OnEnable()
        {
            // Disable component highlighting
            componentHighlighting.isActive = false;

            speedSlider.value = 1;

            var currentProjectObjectModel = ProjectManager.Instance.CurrentProject.ObjectModel;
            Utility.ApplyRecursively(
                currentProjectObjectModel,
                o => o.SetActive(false),
                false
            );

            _index = -1;
            _playbackList = new List<GameObject>();
            FillList(currentProjectObjectModel);
        }

        /// <summary>
        ///  Fill list with components and fused groups
        /// </summary>
        /// <param name="parent">GameObject parent</param>
        private void FillList(GameObject parent)
        {
            for (var i = 0; i < parent.transform.childCount; i++)
            {
                var go = parent.transform.GetChild(i).gameObject;

                var itemInfo = go.GetComponent<ItemInfoController>().ItemInfo;

                if (itemInfo.isGroup)
                    if (itemInfo.isFused) _playbackList.Add(go);
                    else FillList(go);
                else _playbackList.Add(go);
            }
        }

        /// <summary>
        ///     Action for the next button
        /// </summary>
        public void NextButton()
        {
            if (!_animationRunning) StartCoroutine(FadeInComponent(true));
        }

        /// <summary>
        ///     Action for the previous button
        /// </summary>
        public void PreviousButton()
        {
            if (!_animationRunning) StartCoroutine(PreviousComponent(true));
        }

        /// <summary>
        ///     Action for the play button
        /// </summary>
        public void PlayButton()
        {
            _animationRunning = true;

            playButton.SetActive(false);
            pauseButton.SetActive(true);

            if (speedSlider.value >= 0) StartCoroutine(FadeInComponent(false));
            else StartCoroutine(PreviousComponent(false));
        }

        /// <summary>
        ///     Action for the pause button
        /// </summary>
        public void PauseButton()
        {
            pauseButton.SetActive(false);
            playButton.SetActive(true);

            _animationRunning = false;
        }

        /// <summary>
        ///     Action for the skip to front button
        /// </summary>
        public void SkipToStart()
        {
            if (_animationRunning) return;

            _index = -1;
            
            Utility.ApplyRecursively(
                ProjectManager.Instance.CurrentProject.ObjectModel,
                o => o.SetActive(false),
                false
            );
        }

        /// <summary>
        ///     Action for the skip to end button
        /// </summary>
        public void SkipToEnd()
        {
            if (_animationRunning) return;

            _index = _playbackList.Count - 1;
            
            Utility.ApplyRecursively(
                ProjectManager.Instance.CurrentProject.ObjectModel,
                o => o.SetActive(true),
                false
            );
        }

        /// <summary>
        ///     Play animation to fade in the next component
        /// </summary>
        /// <param name="skip">True if the coroutine is started from the skip button</param>
        /// <returns>IEnumerator for the coroutine</returns>
        private IEnumerator FadeInComponent(bool skip)
        {
            if (_index >= _playbackList.Count - 1)
            {
                if (skip) yield break;

                _animationRunning = false;
                pauseButton.SetActive(false);
                playButton.SetActive(true);

                yield break;
            }

            _index++;

            _animationRunning = true;

            var currentObject = _playbackList[_index];

            Utility.ApplyRecursively(currentObject, obj => obj.SetActive(true), false);

            var speedSliderValue = speedSlider.value;

            Utility.ApplyRecursively(currentObject, SetFade, false);
            for (float opacity = 0; opacity <= 1; opacity += 0.1f * (skip ? 1.5f : speedSliderValue))
            {
                SetOpacity(currentObject, opacity);
                yield return new WaitForSeconds(0.05f);
            }

            Utility.ApplyRecursively(currentObject, SetOpaque, false);

            if (skip)
            {
                _animationRunning = false;
            }
            else if (_animationRunning)
            {
                if (speedSlider.value >= 0) StartCoroutine(FadeInComponent(false));
                else StartCoroutine(PreviousComponent(false));
            }
        }

        /// <summary>
        ///     Fade out current component
        /// </summary>
        /// <param name="skip">True if the coroutine is started from the skip button</param>
        /// <returns>IEnumerator for the coroutine</returns>
        private IEnumerator PreviousComponent(bool skip)
        {
            if (_index < 0)
            {
                if (skip) yield break;

                _animationRunning = false;
                pauseButton.SetActive(false);
                playButton.SetActive(true);

                yield break;
            }

            _animationRunning = true;

            var currentObject = _playbackList[_index];

            var speedSliderValue = speedSlider.value;

            Utility.ApplyRecursively(currentObject, SetFade, false);
            for (float opacity = 1; opacity >= 0; opacity += 0.1f * (skip ? -1.5f : speedSliderValue))
            {
                SetOpacity(currentObject, opacity);
                yield return new WaitForSeconds(0.05f);
            }

            Utility.ApplyRecursively(currentObject, obj => obj.SetActive(false), false);
            Utility.ApplyRecursively(currentObject, SetOpaque, false);

            _index--;

            if (skip)
            {
                _animationRunning = false;
            }
            else if (_animationRunning)
            {
                if (speedSlider.value >= 0) StartCoroutine(FadeInComponent(false));
                else StartCoroutine(PreviousComponent(false));
            }
        }

        /// <summary>
        ///     Set opacity recursively for component and its children
        /// </summary>
        /// <param name="parent">The parent game object</param>
        /// <param name="opacity">The opacity</param>
        private static void SetOpacity(GameObject parent, float opacity)
        {
            Utility.ApplyRecursively(
                parent,
                obj =>
                {
                    var objRenderer = obj.GetComponent<Renderer>();
                    var c = objRenderer.material.color;
                    c.a = opacity;
                    objRenderer.material.color = c;
                },
                false
            );
        }

        /// <summary>
        ///     Set rendering mode to "Fade" for animation
        /// </summary>
        /// <param name="go"></param>
        private static void SetFade(GameObject go)
        {
            var renderer = go.GetComponent<Renderer>();
            var m = renderer.material;

            m.SetInt("_SrcBlend", (int) BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int) BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_ALPHATEST_ON");
            m.EnableKeyword("_ALPHABLEND_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.renderQueue = 3000;

            renderer.material = m;
        }

        /// <summary>
        ///     Set rendering mode to "Opaque" after animation
        /// </summary>
        /// <param name="go"></param>
        private static void SetOpaque(GameObject go)
        {
            var renderer = go.GetComponent<Renderer>();
            var m = renderer.material;

            m.SetInt("_SrcBlend", (int) BlendMode.One);
            m.SetInt("_DstBlend", (int) BlendMode.Zero);
            m.SetInt("_ZWrite", 1);
            m.DisableKeyword("_ALPHATEST_ON");
            m.DisableKeyword("_ALPHABLEND_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.renderQueue = -1;
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
        }
    }
}