using System.Windows.Forms;
using TMPro;
using UnityEngine;

namespace Shared
{
    public class TooltipController : MonoBehaviour
    {
        /// <summary>
        ///     The text of the tooltip
        /// </summary>
        public TextMeshProUGUI textLabel;

        /// <summary>
        ///     The canvas for the main screen
        /// </summary>
        public Canvas mainCanvas;

        /// <summary>
        ///     The padding of the tooltip
        /// </summary>
        private const float Padding = 12;
        
        /// <summary>
        ///     Show the tooltip
        /// </summary>
        /// <param name="x">Position of the tooltip</param>
        /// <param name="y">Position of the tooltip</param>
        /// <param name="text">Text of the tooltip</param>
        public void ShowTooltip(float x, float y, string text)
        {
            // Set the text
            textLabel.SetText(text);
            
            // Calculate the size of the tooltip
            var width = textLabel.preferredWidth + 2 * Padding;
            
            // Set the bounds of the rect transform
            var rectTransform = gameObject.GetComponent<RectTransform>();  
            rectTransform.position = new Vector2(x, y);
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