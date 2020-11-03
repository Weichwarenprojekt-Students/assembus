using UnityEngine;

namespace Shared.Toast
{
    public class ToastController : MonoBehaviour
    {
        /// <summary>
        ///     The object for a success toast
        /// </summary>
        public Toast successToast;

        /// <summary>
        ///     The object for an error toast
        /// </summary>
        public Toast errorToast;

        /// <summary>
        ///     Show an error toast
        /// </summary>
        /// <param name="duration">Duration of the toast (in s)</param>
        /// <param name="text">Text of the toast</param>
        public void Error(int duration, string text)
        {
            var newToast = Instantiate(
                errorToast,
                transform,
                true
            );
            newToast.gameObject.transform.SetAsFirstSibling();
            newToast.Show(duration, text);
        }

        /// <summary>
        ///     Show an error toast
        /// </summary>
        /// <param name="duration">Duration of the toast (in s)</param>
        /// <param name="text">Text of the toast</param>
        public void Success(int duration, string text)
        {
            var newToast = Instantiate(
                successToast,
                transform,
                true
            );
            newToast.gameObject.transform.SetAsFirstSibling();
            newToast.Show(duration, text);
        }
    }
}