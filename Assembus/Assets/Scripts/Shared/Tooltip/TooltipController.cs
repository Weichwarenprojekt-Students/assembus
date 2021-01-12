using TMPro;
using UnityEngine;

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
        public void ShowTooltip(float x, float y, string text, bool center)
        {
            // Set the text
            textLabel.SetText(text);

            // Calculate the size of the tooltip
            var width = textLabel.preferredWidth + 2 * Padding;

            // Set the bounds of the rect transform
            var scale = mainCanvas.transform.localScale.x;
            var rectTransform = gameObject.GetComponent<RectTransform>();
            var offset = center ? -scale * width / 2 : 0;
            rectTransform.position = new Vector3(x + offset, y, 0);
            var sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
            rectTransform.sizeDelta = sizeDelta;

            // Check if the position has to be adjusted (if tooltip is out of bounds)
            var margin = 16 * scale;
            var delta = rectTransform.position.x + margin + sizeDelta.x * scale - Screen.width;
            if (delta > 0) rectTransform.position = new Vector3(rectTransform.position.x - delta, y, 0);
            else if (rectTransform.position.x - margin < 0) rectTransform.position = new Vector3(margin, y, 0);

            // Show the tooltip
            gameObject.SetActive(true);
        }

        public void HideTooltip()
        {
            gameObject.SetActive(false);
        }
    }
}