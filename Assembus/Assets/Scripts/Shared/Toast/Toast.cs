using System.Collections;
using TMPro;
using UnityEngine;

namespace Shared.Toast
{
    public class Toast : MonoBehaviour
    {
        /// <summary>
        ///     The constants for the duration
        /// </summary>
        public const int Short = 3, Long = 6;

        /// <summary>
        ///     The bool variable of the animator
        /// </summary>
        private static readonly int Open = Animator.StringToHash("open");

        /// <summary>
        ///     The text element
        /// </summary>
        public TextMeshProUGUI text;

        /// <summary>
        ///     The animation of the display
        /// </summary>
        public Animator animator;

        /// <summary>
        ///     Show the toast
        /// </summary>
        /// <param name="duration">Duration of the toast (in s)</param>
        /// <param name="newText">Text of the toast</param>
        public void Show(int duration, string newText)
        {
            StartCoroutine(ShowToast(duration, newText));
        }

        /// <summary>
        ///     Show the toast
        /// </summary>
        /// <param name="duration">Duration of the toast (in s)</param>
        /// <param name="newText">Text of the toast</param>
        private IEnumerator ShowToast(int duration, string newText)
        {
            // Change the text
            text.text = newText;

            // Show the toast
            animator.SetBool(Open, true);
            yield return new WaitForSeconds(duration);
            animator.SetBool(Open, false);

            // Delete the toast
            yield return new WaitForSeconds(3);
            Destroy(gameObject);
        }
    }
}