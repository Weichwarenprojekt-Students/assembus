using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace Shared.LoadingScreen
{
    public class LoadingController : MonoBehaviour
    {
        /// <summary>
        ///     The bool variable of the animator
        /// </summary>
        private static readonly int Open = Animator.StringToHash("open");

        /// <summary>
        ///     The logo object
        /// </summary>
        public GameObject logoObject;

        /// <summary>
        ///     The animators
        /// </summary>
        public Animator leftPanel, rightPanel, logo;

        /// <summary>
        ///     The result of the loading process
        /// </summary>
        private (bool, string) _result;

        /// <summary>
        ///     Show the loading screen
        /// </summary>
        /// <param name="action">The workload</param>
        /// <param name="reaction">The reaction to the result of the workload</param>
        /// <param name="delay">Delay to be inserted on positive result</param>
        public void ShowLoadingScreen(Func<(bool, string)> action, Action<(bool, string)> reaction, int delay = 0)
        {
            // Show the object and reset the rotation
            logoObject.SetActive(true);
            logo.transform.rotation = Quaternion.Euler(0, 0, 0);

            // Start the animations
            leftPanel.SetBool(Open, true);
            rightPanel.SetBool(Open, true);
            logo.SetBool(Open, true);

            // Start the coroutine
            StartCoroutine(ShowLoadingScreen(delay, action, reaction));
        }

        /// <summary>
        ///     Show the loading screen
        /// </summary>
        /// <param name="delay">Delay to be inserted on positive result</param>
        /// <param name="action">The workload</param>
        /// <param name="reaction">The reaction to the result of the workload</param>
        private IEnumerator ShowLoadingScreen(int delay, Func<(bool, string)> action, Action<(bool, string)> reaction)
        {
            yield return new WaitForSeconds(1);
            var thread = new Thread(
                () =>
                {
                    _result = action();
                    if (_result.Item1) Thread.Sleep(delay);
                }
            );
            thread.Start();

            // Wait for the thread to be finished
            while (thread.IsAlive) yield return new WaitForEndOfFrame();

            // Hide the screen and process the result
            reaction(_result);
            HideLoadingScreen();
        }

        /// <summary>
        ///     Hide the loading screen
        /// </summary>
        private void HideLoadingScreen()
        {
            leftPanel.SetBool(Open, false);
            rightPanel.SetBool(Open, false);
            logo.SetBool(Open, false);
            logoObject.SetActive(false);
        }
    }
}