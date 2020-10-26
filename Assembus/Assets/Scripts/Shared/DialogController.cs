using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shared
{
    public class DialogController : MonoBehaviour
    {
        /// <summary>
        ///     The dialog panel
        /// </summary>
        public GameObject dialog;

        /// <summary>
        ///     The dialog text
        /// </summary>
        public Text title, description;

        /// <summary>
        ///     The current action for the dialog
        /// </summary>
        private Action _dialogAction;

        /// <summary>
        ///     Show the dialog
        /// </summary>
        /// <param name="newTitle">The title of the dialog</param>
        /// <param name="newDescription">The description of the dialog</param>
        /// <param name="action">The action that shall be executed on confirm</param>
        public void Show(string newTitle, string newDescription, Action action)
        {
            // Set the title
            title.text = newTitle;
            // Set the description
            description.text = newDescription;
            // Set the action
            _dialogAction = action;
            // Show the dialog
            dialog.SetActive(true);
        }

        /// <summary>
        ///     Execute the dialog action
        /// </summary>
        public void ConfirmDialog()
        {
            dialog.SetActive(false);
            _dialogAction();
        }

        /// <summary>
        ///     Close the dialog
        /// </summary>
        public void CancelDialog()
        {
            dialog.SetActive(false);
        }
    }
}
