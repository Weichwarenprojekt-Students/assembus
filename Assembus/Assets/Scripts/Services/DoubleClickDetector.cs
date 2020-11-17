using System.Collections;
using UnityEngine;

namespace Services
{
    /// <summary>
    ///     Delegate for double click
    /// </summary>
    public delegate void Notify();

    public class DoubleClickDetector : MonoBehaviour
    {
        /// <summary>
        ///     Time frame for double click detection
        /// </summary>
        private const float TimeBetweenClicks = 0.25f;

        /// <summary>
        ///     Allow coroutine for double click detection
        /// </summary>
        private bool _coroutineAllowed = true;

        /// <summary>
        ///     Time when left mouse button is clicked first time
        /// </summary>
        private float _firstLeftClickTime;

        /// <summary>
        ///     Left mouse click counter for double click detection
        /// </summary>
        private int _leftClickCounter;

        /// <summary>
        ///     Delegate which gets called when double click occured
        /// </summary>
        public event Notify DoubleClickOccured;


        /// <summary>
        ///     Perform a click and increment the click counter by 1
        /// </summary>
        public void Click()
        {
            _leftClickCounter++;
        }

        /// <summary>
        ///     Checks for second click
        /// </summary>
        public void CheckForSecondClick()
        {
            if (_leftClickCounter == 1 && _coroutineAllowed)
            {
                _firstLeftClickTime = Time.time;
                StartCoroutine(DoubleClickDetection());
            }
        }

        /// <summary>
        ///     Coroutine, detect double clicking
        /// </summary>
        private IEnumerator DoubleClickDetection()
        {
            _coroutineAllowed = false;
            while (Time.time < _firstLeftClickTime + TimeBetweenClicks)
            {
                if (_leftClickCounter == 2)
                {
                    // Call the delegate
                    DoubleClickOccured?.Invoke();
                    break;
                }

                yield return new WaitForEndOfFrame();
            }

            _leftClickCounter = 0;
            _firstLeftClickTime = 0f;
            _coroutineAllowed = true;
        }
    }
}