using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shared.SwitchableButton
{
    public class SwitchableButton : MonoBehaviour
    {
        /// <summary>
        ///     The enabled color
        /// </summary>
        public new Color enabled;
        
        /// <summary>
        ///     The disabled color
        /// </summary>
        public Color disabled;
        
        /// <summary>
        ///     The image of the button
        /// </summary>
        public RawImage image;
        
        /// <summary>
        ///     The actual button
        /// </summary>
        public Button button;

        /// <summary>
        ///     Enable/Disable the button
        /// </summary>
        /// <param name="enable">True if the button shall be enabled</param>
        public void Enable(bool enable)
        {
            button.interactable = enable;
            image.color = enable ? enabled : disabled;
        }
    }
}