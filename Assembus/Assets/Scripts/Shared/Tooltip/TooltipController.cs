using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shared.Tooltip
{
    public class TooltipController : MonoBehaviour
    {
        /// <summary>
        ///     The padding of the tooltip
        /// </summary>
        private const float Padding = 12;

        /// <summary>
        ///     The text of the tooltip
        /// </summary>
        public TextMeshProUGUI textLabel;

        /// <summary>
        ///     The canvas for the main screen
        /// </summary>
        public Canvas mainCanvas;

        /// <summary>
        ///     Show the tooltip
        /// </summary>
        /// <param name="x">Position of the tooltip</param>
        /// <param name="y">Position of the tooltip</param>
        /// <param name="text">Text of the tooltip</param>
        /// <param name="center">True if the tooltip shall be centered</param>
        /// <param name="scaleX">The scale of the canvas</param>
        public void ShowTooltip(float x, float y, string text, bool center, float scaleX)
        {
            // Set the text
            textLabel.SetText(text);

            // Calculate the size of the tooltip
            var width = textLabel.preferredWidth + 2 * Padding;

            // Set the bounds of the rect transform
            var rectTransform = gameObject.GetComponent<RectTransform>();
            var offset = center ? -scaleX * width / 2 : 0;
            rectTransform.position = new Vector2(x + offset, y);
            rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);

            // Show the tooltip
            gameObject.SetActive(true);
        }

        public void HideTooltip()
        {
            gameObject.SetActive(false);
        }
    }
}